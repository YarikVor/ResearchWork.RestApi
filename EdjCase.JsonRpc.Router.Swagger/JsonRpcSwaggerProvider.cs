using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using EdjCase.JsonRpc.Router.Abstractions;
using EdjCase.JsonRpc.Router.Swagger.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EdjCase.JsonRpc.Router.Swagger;

public class JsonRpcSwaggerProvider : ISwaggerProvider
{
    private readonly ISchemaGenerator schemaGenerator;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly SwaggerConfiguration swagerOptions;
    private readonly IXmlDocumentationService xmlDocumentationService;
    private OpenApiDocument? cacheDocument;
    private readonly JsonNamingPolicy namePolicy;

    public JsonRpcSwaggerProvider(
        ISchemaGenerator schemaGenerator,
        IXmlDocumentationService xmlDocumentationService,
        IOptions<SwaggerConfiguration> swaggerOptions,
        IServiceScopeFactory scopeFactory
    )
    {
        this.schemaGenerator = schemaGenerator;
        swagerOptions = swaggerOptions.Value;
        namePolicy = swaggerOptions.Value.NamingPolicy;
        this.scopeFactory = scopeFactory;
        this.xmlDocumentationService = xmlDocumentationService;
    }

    public OpenApiDocument GetSwagger(string documentName, string? host = null, string? basePath = null)
    {
        if (cacheDocument != null) return cacheDocument;

        var schemaRepository = new SchemaRepository();
        var methodProvider =
            scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IRpcMethodProvider>();
        var metaData = methodProvider.Get();
        var paths = GetOpenApiPaths(metaData, schemaRepository);

        cacheDocument = new OpenApiDocument
        {
            Info = new OpenApiInfo
            {
                Title = Assembly.GetEntryAssembly()!.GetName().Name,
                Version = "rpc/v1"
            },
            Servers = swagerOptions.Endpoints.Select(x => new OpenApiServer
            {
                Url = x
            }).ToList(),
            Components = new OpenApiComponents
            {
                Schemas = schemaRepository.Schemas
            },
            Paths = paths
        };

        return cacheDocument;
    }

    private List<UniqueMethod> GetUniqueKeyMethodPairs(RpcRouteMetaData metaData)
    {
        var methodList = Convert(metaData.BaseRoute, null).ToList();

        foreach ((var path, var pathRoutes) in metaData.PathRoutes) methodList.AddRange(Convert(pathRoutes, path));

        return methodList;
    }

    private IEnumerable<UniqueMethod> Convert(IEnumerable<IRpcMethodInfo> routeInfo, RpcPath? path)
    {
        //group by name for generate unique url similar method names
        foreach (var methodsGroup in routeInfo.GroupBy(x => x.Name))
        {
            int? methodCounter = methodsGroup.Count() > 1 ? 1 : null;
            foreach (var methodInfo in methodsGroup)
            {
                var methodName = namePolicy.ConvertName(methodInfo.Name);
                var uniqueUrl = $"/{path}#{methodName}";

                if (methodCounter != null) uniqueUrl += $"#{methodCounter++}";

                yield return new UniqueMethod(uniqueUrl, methodInfo);
            }
        }
    }

    private OpenApiPaths GetOpenApiPaths(RpcRouteMetaData metaData, SchemaRepository schemaRepository)
    {
        var paths = new OpenApiPaths();

        var uniqueMethods = GetUniqueKeyMethodPairs(metaData);

        foreach (var method in uniqueMethods)
        {
            var operationKey = method.UniqueUrl.Replace("/", "_").Replace("#", "|");
            var operation = GetOpenApiOperation(operationKey, method.Info, schemaRepository);

            var pathItem = new OpenApiPathItem
            {
                Operations = new Dictionary<OperationType, OpenApiOperation>
                {
                    [OperationType.Post] = operation
                }
            };
            paths.Add(method.UniqueUrl, pathItem);
        }

        return paths;
    }

    private OpenApiOperation GetOpenApiOperation(string key, IRpcMethodInfo methodInfo,
        SchemaRepository schemaRepository)
    {
        var methodAnnotation = xmlDocumentationService.GetSummaryForMethod(methodInfo);
        var trueReturnType = GetReturnType(methodInfo.RawReturnType);

        return new OpenApiOperation
        {
            Tags = new List<OpenApiTag>(),
            Summary = methodAnnotation,
            RequestBody = GetOpenApiRequestBody(key, methodInfo, schemaRepository),
            Responses = GetOpenApiResponses(key, trueReturnType, schemaRepository)
        };
    }

    private Type GetReturnType(Type returnType)
    {
        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            //Return the `Task` return type
            return returnType.GenericTypeArguments.First();
        if (returnType == typeof(Task))
            //Task with no return type
            return typeof(void);
        return returnType;
    }

    private OpenApiResponses GetOpenApiResponses(string key, Type returnMethodType, SchemaRepository schemaRepository)
    {
        return new OpenApiResponses
        {
            ["200"] = new()
            {
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new()
                    {
                        Schema = GeResposeSchema(key, returnMethodType, schemaRepository)
                    }
                }
            }
        };
    }

    private OpenApiRequestBody GetOpenApiRequestBody(string key, IRpcMethodInfo methodInfo,
        SchemaRepository schemaRepository)
    {
        return new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new()
                {
                    Schema = GetBodyParamsSchema(key, schemaRepository, methodInfo)
                }
            }
        };
    }

    private OpenApiSchema GetBodyParamsSchema(string key, SchemaRepository schemaRepository, IRpcMethodInfo methodInfo)
    {
        var paramsObjectSchema = GetOpenApiEmptyObject();

        foreach (var parameterInfo in methodInfo.Parameters)
        {
            var name = namePolicy.ConvertName(parameterInfo.Name);
            var schema = schemaGenerator.GenerateSchema(parameterInfo.RawType, schemaRepository);
            paramsObjectSchema.Properties.Add(name, schema);
        }

        paramsObjectSchema = schemaRepository.AddDefinition($"{key}", paramsObjectSchema);

        var requestSchema = GetOpenApiEmptyObject();

        requestSchema.Properties.Add("id", schemaGenerator.GenerateSchema(typeof(string), schemaRepository));
        requestSchema.Properties.Add("jsonrpc", schemaGenerator.GenerateSchema(typeof(string), schemaRepository));
        requestSchema.Properties.Add("method", schemaGenerator.GenerateSchema(typeof(string), schemaRepository));
        requestSchema.Properties.Add("params", paramsObjectSchema);

        requestSchema = schemaRepository.AddDefinition($"request_{key}", requestSchema);

        RewriteJrpcAttributesExamples(requestSchema, schemaRepository, namePolicy.ConvertName(methodInfo.Name));

        return requestSchema;
    }

    private OpenApiSchema GeResposeSchema(string key, Type returnMethodType, SchemaRepository schemaRepository)
    {
        var resultSchema = schemaGenerator.GenerateSchema(returnMethodType, schemaRepository);

        var responseSchema = GetOpenApiEmptyObject();
        responseSchema.Properties.Add("id", schemaGenerator.GenerateSchema(typeof(string), schemaRepository));
        responseSchema.Properties.Add("jsonrpc", schemaGenerator.GenerateSchema(typeof(string), schemaRepository));
        responseSchema.Properties.Add("result", resultSchema);

        responseSchema = schemaRepository.AddDefinition($"response_{key}", responseSchema);
        RewriteJrpcAttributesExamples(responseSchema, schemaRepository);
        return responseSchema;
    }

    private OpenApiSchema GetOpenApiEmptyObject()
    {
        return new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>(),
            Required = new SortedSet<string>(),
            AdditionalPropertiesAllowed = false
        };
    }

    private void RewriteJrpcAttributesExamples(OpenApiSchema schema, SchemaRepository schemaRepository,
        string method = "method_name")
    {
        var jrpcAttributesExample =
            new OpenApiObject
            {
                { "id", new OpenApiString(Guid.NewGuid().ToString()) },
                { "jsonrpc", new OpenApiString("2.0") },
                { "method", new OpenApiString(method) }
            };

        foreach (var prop in schemaRepository.Schemas[schema.Reference.Id].Properties)
            if (jrpcAttributesExample.ContainsKey(prop.Key))
                prop.Value.Example = jrpcAttributesExample[prop.Key];
    }
}