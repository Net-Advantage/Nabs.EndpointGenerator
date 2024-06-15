namespace Nabs.EndpointGenerator.Helpers;

internal static class ActionMethodHelpers
{
    private const string _namespace = "Nabs.EndpointGenerator";

    internal static string BuildActionMethods(this IAssemblySymbol assemblySymbol, ClassDeclarationSyntax classDeclarationSyntax)
    {
        var actionMethods = new StringBuilder();
        if (assemblySymbol != null)
        {
            var namespaces = assemblySymbol.GlobalNamespace.GetAllNamespaces();
            foreach (var namespaceSymbol in namespaces)
            {
                var namedTypeSymbols = namespaceSymbol.GetPublicClassesImplementingInterface("IRequest");
                foreach (var namedTypeSymbol in namedTypeSymbols)
                {
                    var attributeData = namedTypeSymbol.GetAttributeByName("Nabs.EndpointGenerator.Abstractions", "HttpEndpointAttribute");

                    if(attributeData is null)
                    {
                        continue;
                    }
                    var actionMethod = namedTypeSymbol.BuildHttpActionMethod(classDeclarationSyntax, attributeData);
                    actionMethods.AppendLine(actionMethod);
                }
            }
        }

        return actionMethods.ToString();
    }

    internal static string BuildHttpActionMethod(
        this INamedTypeSymbol namedTypeSymbol,
        ClassDeclarationSyntax classDeclarationSyntax,
        AttributeData attributeData)
    {
        var (className, requestType, responseType, httpEndpointAttributeData) = namedTypeSymbol
            .GetAttributeData(attributeData);

        var swaggerOperation = httpEndpointAttributeData
            .GetSwaggerOperation(classDeclarationSyntax);

        (string httpVerb, string from) = attributeData!.AttributeClass!.Name switch
        {
            "HttpGetEndpointAttribute" => ("HttpGet", "FromRoute"),
            "HttpPostEndpointAttribute" => ("HttpPost", "FromBody"),
            "HttpPushEndpointAttribute" => ("HttpPush", "FromBody"),
            "HttpDeleteEndpointAttribute" => ("HttpDelete", "FromBody"),
            _ => throw new InvalidOperationException("Invalid attribute")
        };

        var sourceText = $@"
        [{httpVerb}(""{httpEndpointAttributeData.RouteTemplate}"")]
        {swaggerOperation}
        public async Task<{responseType}> {className}Action([{from}]{requestType} request)
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
        if (string.IsNullOrWhiteSpace(httpEndpointAttributeData.OperationId))
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

        if (attributeData.ConstructorArguments.Length >= 2)
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
