namespace Nabs.EndpointGenerator.Extensions;

internal static class ClassDeclarationSyntaxExtensions
{
    internal static string GetNamespace(this ClassDeclarationSyntax classDeclaration)
    {
        // Check for a regular NamespaceDeclarationSyntax ancestor
        var namespaceDeclaration = classDeclaration.FirstAncestorOrSelf<NamespaceDeclarationSyntax>();
        if (namespaceDeclaration != null)
        {
            return namespaceDeclaration.Name.ToString();
        }

        // If not found, check for a FileScopedNamespaceDeclarationSyntax ancestor
        var fileScopedNamespace = classDeclaration.FirstAncestorOrSelf<FileScopedNamespaceDeclarationSyntax>();
        if (fileScopedNamespace != null)
        {
            return fileScopedNamespace.Name.ToString();
        }

        // Handle cases where the class might not be in a namespace
        // This could be a top-level class, a class within a script, etc.
        return string.Empty; // Or any default namespace indication as per your use case
    }
}
