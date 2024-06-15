namespace Nabs.EndpointGenerator;

[Generator]
public class RequestControllerGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        InitializationHelpers.InitializeGenerator(context);
        PostInitializationHelpers.InitializeGenerator(context);
    }

    private static string CreateActionMethod(
        SourceProductionContext ctx,
        ClassDeclarationSyntax classDeclarationSyntax,
        INamedTypeSymbol namedTypeSymbol)
    {
        _ = ctx;

        var attributeData = namedTypeSymbol.GetAttributeByName("Nabs.EndpointGenerator.Abstractions", "HttpEndpointAttribute");

        if(attributeData is null)
        {
            return string.Empty;
        }

        var sourceText = namedTypeSymbol.BuildHttpActionMethod(classDeclarationSyntax, attributeData);

        return sourceText;
    }
}
