using Microsoft.CodeAnalysis;

namespace Nabs.EndpointGenerator.Helpers;

public static class TypeSymbolExtensions
{
    public static string GetFullyQualifiedName(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
        {
            // For nested types, recursively get the containing type's full name
            var containingType = namedTypeSymbol.ContainingType;
            if (containingType != null)
            {
                return containingType.GetFullyQualifiedName() + "+" + namedTypeSymbol.MetadataName;
            }

            // Combine the namespace and type name
            var namespaceName = namedTypeSymbol.ContainingNamespace.IsGlobalNamespace
                ? string.Empty
                : namedTypeSymbol.ContainingNamespace.ToDisplayString() + ".";

            return namespaceName + namedTypeSymbol.MetadataName;
        }

        // Fallback for non-named types (e.g., array types, pointer types, etc.)
        return typeSymbol.ToDisplayString();
    }
}
