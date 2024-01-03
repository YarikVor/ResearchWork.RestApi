using System;
using System.IO;
using System.Reflection;
using System.Xml.XPath;
using EdjCase.JsonRpc.Router.Abstractions;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EdjCase.JsonRpc.Router.Swagger;

public interface IXmlDocumentationService
{
    string GetSummaryForType(Type type);
    string GetSummaryForMethod(IRpcMethodInfo methodInfo);
    string GetMethodParameterExample(IRpcMethodInfo methodInfo, IRpcParameterInfo parameterInfo);
    string GetPropertyExample(PropertyInfo propertyInfo);
}

public class XmlDocumentationService : IXmlDocumentationService
{
    private const string MemberXPath = "/doc/members/member[@name='{0}']";
    private const string SummaryTag = "summary";
    private readonly XPathNavigator xpathNavigator;

    public XmlDocumentationService()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml");
        if (File.Exists(filePath))
        {
            var xmlComments = File.OpenText(filePath);
            var xpathDocument = new XPathDocument(xmlComments);
            xpathNavigator = xpathDocument.CreateNavigator();
        }
        else
        {
            var xpathDocument = new XPathDocument(new StringReader("<none></none>"));
            xpathNavigator = xpathDocument.CreateNavigator();
        }
    }

    public string GetSummaryForType(Type type)
    {
        var memberName = XmlCommentsNodeNameHelper.GetMemberNameForType(type);
        var typeNode = xpathNavigator.SelectSingleNode(string.Format(MemberXPath, memberName));
        if (typeNode == null) return string.Empty;
        var summaryNode = typeNode.SelectSingleNode(SummaryTag);
        return XmlCommentsTextHelper.Humanize(summaryNode!.InnerXml);
    }

    public string GetSummaryForMethod(IRpcMethodInfo methodInfo)
    {
        var methodNode = xpathNavigator.SelectSingleNode($"/doc/members/member[@name='{methodInfo.Name}']");
        var summaryNode = methodNode?.SelectSingleNode("summary");
        return summaryNode != null ? XmlCommentsTextHelper.Humanize(summaryNode.InnerXml) : string.Empty;
    }

    public string GetMethodParameterExample(IRpcMethodInfo methodInfo, IRpcParameterInfo parameterInfo)
    {
        var paramNode = xpathNavigator.SelectSingleNode(
            $"/doc/members/member[@name='{methodInfo.Name}']/param[@name='{parameterInfo.Name}']");
        if (paramNode == null) return string.Empty;
        var example = paramNode.GetAttribute("example", "");
        return example;
    }

    public string GetPropertyExample(PropertyInfo propertyInfo)
    {
        var propertyMemberName = XmlCommentsNodeNameHelper.GetMemberNameForFieldOrProperty(propertyInfo);
        var propertyExampleNode =
            xpathNavigator.SelectSingleNode($"/doc/members/member[@name='{propertyMemberName}']/example");
        if (propertyExampleNode == null) return string.Empty;
        var example = XmlCommentsTextHelper.Humanize(propertyExampleNode.InnerXml);
        return example;
    }
}