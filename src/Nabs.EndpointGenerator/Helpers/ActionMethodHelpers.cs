using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text;

namespace Nabs.EndpointGenerator.Helpers;

public static class ActionMethodHelpers
{
    private const string _namespace = "Nabs.EndpointGenerator";

    public static string BuildHttpGetActionMethod(
        this INamedTypeSymbol namedTypeSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        AttributeData attributeData)
    {
        var (className, requestType, responseType, httpEndpointAttributeData) = namedTypeSymbol
            .GetAttributeData(attributeData);

        var swaggerOperation = httpEndpointAttributeData.GetSwaggerOperation(classDeclarationSyntax);

        var sourceText = $@"
        [HttpGet(""{httpEndpointAttributeData.RouteTemplate}"")]
        {swaggerOperation}
        public async Task<{responseType}> {className}Action([FromRoute]{requestType} request)
        {{
            var response = await _mediator.Send(request);
            return response;
        }}
";
        return sourceText;
    }

    public static string BuildHttpPostActionMethod(
        this INamedTypeSymbol namedTypeSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        AttributeData attributeData)
    {
        var (className, requestType, responseType, httpEndpointAttributeData) = namedTypeSymbol
            .GetAttributeData(attributeData);

        var swaggerOperation = httpEndpointAttributeData.GetSwaggerOperation(classDeclarationSyntax);

        var sourceText = $@"
        [HttpPost(""{httpEndpointAttributeData.RouteTemplate}"")]
        {swaggerOperation}
        public async Task<{responseType}> {className}Action([FromBody]{requestType} request)
        {{
            var response = await _mediator.Send(request);
            return response;
        }}
";
        return sourceText;
    }

    public static string BuildHttpPushActionMethod(
        this INamedTypeSymbol namedTypeSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        AttributeData attributeData)
    {
        var (className, requestType, responseType, httpEndpointAttributeData) = namedTypeSymbol
            .GetAttributeData(attributeData);

        var swaggerOperation = httpEndpointAttributeData.GetSwaggerOperation(classDeclarationSyntax);

        var sourceText = $@"
        [HttpPush(""{httpEndpointAttributeData.RouteTemplate}"")]
        {swaggerOperation}
        public async Task<{responseType}> {className}Action([FromBody]{requestType} request)
        {{
            var response = await _mediator.Send(request);
            return response;
        }}
";
        return sourceText;
    }

    public static string BuildHttpDeleteActionMethod(
        this INamedTypeSymbol namedTypeSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        AttributeData attributeData)
    {
        var (className, requestType, responseType, httpEndpointAttributeData) = namedTypeSymbol
            .GetAttributeData(attributeData);

        var swaggerOperation = httpEndpointAttributeData.GetSwaggerOperation(classDeclarationSyntax);

        var sourceText = $@"
        [HttpDelete(""{httpEndpointAttributeData.RouteTemplate}"")]
        {swaggerOperation}
        public async Task<{responseType}> {className}Action([FromBody]{requestType} request)
        {{
            var response = await _mediator.Send(request);
            return response;
        }}
";
        return sourceText;
    }

    private static string GetSwaggerOperation(
        this HttpEndpointAttributeData httpEndpointAttributeData,
        ClassDeclarationSyntax classDeclarationSyntax)
    {
        if(string.IsNullOrWhiteSpace(httpEndpointAttributeData.OperationId))
        {
            return string.Empty;
        }

        var operation = new StringBuilder();
        operation.AppendLine("[SwaggerOperation(");
        operation.AppendLine($"Summary = \"{httpEndpointAttributeData.Description}\", ");
        operation.AppendLine($"Description = \"{httpEndpointAttributeData.Description}\", ");
        operation.AppendLine($"OperationId = \"{httpEndpointAttributeData.OperationId}\", ");
        operation.AppendLine($"Tags = new[] {{ \" {classDeclarationSyntax.Identifier.Text} \" }}");
        operation.AppendLine(")]");
        return operation.ToString();
    }

    private static (string className, 
        string requestType, 
        ITypeSymbol responseType, 
        HttpEndpointAttributeData httpEndpointAttributeData) 
        GetAttributeData(this INamedTypeSymbol namedTypeSymbol, AttributeData attributeData)
    {
        var httpEndpointAttributeData = new HttpEndpointAttributeData()
        {
            RouteTemplate = attributeData.ConstructorArguments[0].Value!.ToString()
        };

        if(attributeData.ConstructorArguments.Length >= 2)
        {
            httpEndpointAttributeData.Description = attributeData.ConstructorArguments[1].Value!.ToString();
        }

        if (attributeData.ConstructorArguments.Length >= 3)
        {
            httpEndpointAttributeData.OperationId = attributeData.ConstructorArguments[2].Value!.ToString();
        }

        return (
            namedTypeSymbol.Name, 
            namedTypeSymbol.GetFullyQualifiedName(), 
            namedTypeSymbol.GetTypeSymbolForInterface("IRequest"),
            httpEndpointAttributeData);
    }
}

public sealed class HttpEndpointAttributeData
{
    public string RouteTemplate { get; set; } = default!;
    public string? Description { get; set; }
    public string? OperationId { get; set; }
}