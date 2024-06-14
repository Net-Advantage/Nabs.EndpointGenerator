using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Nabs.EndpointGenerator.Helpers;

internal class InitializeHelpers
{
    private static readonly string[] _attributeNames =
        [
        "RequestEndpointController",
        "RequestEndpointController`1"
        ];

    public AttributeSyntax RequestEndpointControllerAttribute { get; private set; } = default!;

    internal bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            var hasAttribute = classDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .Any(IsRequestEndpointControllerAttribute);

            return hasAttribute;
        }
        return false;
    }

    internal ClassDeclarationSyntax Transform(
        GeneratorSyntaxContext ctx,
        CancellationToken cancellationToken)
    {
        var result = (ClassDeclarationSyntax)ctx.Node;

        RequestEndpointControllerAttribute = result.AttributeLists
            .SelectMany(attrList => attrList.Attributes)
            .FirstOrDefault(IsRequestEndpointControllerAttribute)
            ?? throw new InvalidOperationException("Missing RequestEndpointControllerAttribute should not happen!");

        return result;
    }

    private static bool IsRequestEndpointControllerAttribute(AttributeSyntax attributeSyntax)
    {
        string attributeName = attributeSyntax.Name switch
        {
            GenericNameSyntax genericName => genericName.Identifier.Text,
            IdentifierNameSyntax identifierName => identifierName.Identifier.Text,
            _ => attributeSyntax.Name.ToString()  // Fallback for other unhandled cases
        };
        return _attributeNames.Contains(attributeName);
    }
}
