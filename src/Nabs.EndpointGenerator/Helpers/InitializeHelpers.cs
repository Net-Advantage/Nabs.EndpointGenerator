using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace Nabs.EndpointGenerator.Helpers;

internal static class InitializeHelpers
{
    private static readonly string[] _attributeNames =
        [
        "GenerateEndpoints",
        "GenerateEndpoints`1",
        "GenerateEndpointsAttribute",
        "GenerateEndpointsAttribute`1"
        ];

    internal static bool Predicate(SyntaxNode node, CancellationToken cancellationToken)
    {
        if (node is ClassDeclarationSyntax classDeclaration)
        {
            var hasAttribute = classDeclaration.AttributeLists
                .SelectMany(attrList => attrList.Attributes)
                .Any(IsGenerateEndpointsAttribute);

            return hasAttribute;
        }
        return false;
    }

    internal static ClassDeclarationSyntax Transform(
        GeneratorSyntaxContext ctx,
        CancellationToken cancellationToken)
    {
        var result = (ClassDeclarationSyntax)ctx.Node;
        return result;
    }

    public static bool IsGenerateEndpointsAttribute(AttributeSyntax attributeSyntax)
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
