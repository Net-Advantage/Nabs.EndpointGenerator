using Microsoft.CodeAnalysis;

namespace Nabs.EndpointGenerator.Helpers;

public static class ActionMethodHelpers
{
    private const string _namespace = "Nabs.EndpointGenerator";

    public static string BuildHttpGetActionMethod(
        this INamedTypeSymbol namedTypeSymbol,
        AttributeData attributeData)
    {
        var (className, requestType, responseType, routeTemplate) = namedTypeSymbol.GetStuff(attributeData);

        var sourceText = $@"
        [HttpGet(""{routeTemplate}"")]
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
        AttributeData attributeData)
    {
        var (className, requestType, responseType, routeTemplate) = namedTypeSymbol.GetStuff(attributeData);

        var sourceText = $@"
        [HttpPost(""{routeTemplate}"")]
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
        AttributeData attributeData)
    {
        var (className, requestType, responseType, routeTemplate) = namedTypeSymbol.GetStuff(attributeData);

        var sourceText = $@"
        [HttpPush(""{routeTemplate}"")]
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
        AttributeData attributeData)
    {
        var (className, requestType, responseType, routeTemplate) = namedTypeSymbol.GetStuff(attributeData);

        var sourceText = $@"
        [HttpDelete(""{routeTemplate}"")]
        public async Task<{responseType}> {className}Action([FromBody]{requestType} request)
        {{
            var response = await _mediator.Send(request);
            return response;
        }}
";
        return sourceText;
    }

    private static (string className, string requestType, ITypeSymbol responseType, string routeTemplate) 
        GetStuff(this INamedTypeSymbol namedTypeSymbol, AttributeData attributeData)
    {
        return (
            namedTypeSymbol.Name, 
            namedTypeSymbol.GetFullyQualifiedName(), 
            namedTypeSymbol.GetTypeSymbolForInterface("IRequest"),
            attributeData!.ConstructorArguments[0].Value!.ToString());
    }
}
