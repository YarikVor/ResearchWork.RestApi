using EdjCase.JsonRpc.Router.Swagger.Extensions;
using ResearchWork.RestApi.GraphQLs;
using ResearchWork.RestApi.Mappers;
using ResearchWork.SqlTypes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var services = builder.Services;

services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();
services.AddDbContext<SchoolDbContext>();
services.AddAutoMapper(typeof(StudentProfile));
services.AddGraphQLServer()
    .AddQueryType<SchoolQuery>();
services.AddJsonRpc();
services.AddJsonRpcWithSwagger();

var app = builder.Build();

app.UseCors();
app.UseCertificateForwarding();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();
app.UseJsonRpc();

app.Run();