namespace Nabs.EndpointGenerator.Helpers;

internal static class InitializationHelpers
{
    internal static void InitializeGenerator(
        IncrementalGeneratorInitializationContext context)
    {
        var compilationProvider = context.CompilationProvider;
        var classes = context.SyntaxProvider
            .CreateSyntaxProvider(
                Predicate,
                Transform);

        var compilationAndClasses = classes.Combine(compilationProvider);

        context.RegisterSourceOutput(compilationAndClasses, SourceOutputHelpers.SourceOutputAction);
    }

    private static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            var hasAttribute = classDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .Any(AttributeHelpers.IsGenerateEndpointsAttribute);

            return hasAttribute;
        }
        return false;
    }

    private static ClassDeclarationSyntax Transform(
        GeneratorSyntaxContext ctx,
        CancellationToken cancellationToken)
    {
        return (ClassDeclarationSyntax)ctx.Node;
    }

    
}
