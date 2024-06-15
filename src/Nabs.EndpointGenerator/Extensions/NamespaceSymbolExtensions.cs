using System;
using System.Collections.Generic;
using System.Text;

namespace Nabs.EndpointGenerator.Extensions;

internal static class NamespaceSymbolExtensions
{
    internal static IEnumerable<INamespaceSymbol> GetAllNamespaces(this INamespaceSymbol root)
    {
        var stack = new Stack<INamespaceSymbol>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            yield return current;

            foreach (var ns in current.GetNamespaceMembers())
            {
                stack.Push(ns);
            }
        }
    }

    internal static IEnumerable<INamedTypeSymbol> GetPublicClassesImplementingInterface(
        this INamespaceSymbol namespaceSymbol,
        string interfaceName)
    {
        var allTypes = namespaceSymbol.GetTypeMembers();
        var publicClasses = allTypes
            .Where(t => t.DeclaredAccessibility == Accessibility.Public && t.TypeKind == TypeKind.Class);

        var classesImplementingIRequest = publicClasses.Where(c => c.ImplementsInterface(interfaceName));
        return classesImplementingIRequest;
    }

    private static bool ImplementsInterface(this INamedTypeSymbol classSymbol, string interfaceName)
    {
        foreach (var iRequestInterfaceTypes in classSymbol.AllInterfaces)
        {
            if (iRequestInterfaceTypes.IsGenericType && iRequestInterfaceTypes.ConstructedFrom.Name == interfaceName)
            {
                return true;
            }
        }

        return false;
    }
}
