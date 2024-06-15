namespace Nabs.EndpointGenerator.Helpers;

public static class AttributeHelpers
{
    private static readonly string[] _attributeNames =
        [
        "GenerateEndpoints",
        "GenerateEndpoints`1",
        "GenerateEndpointsAttribute",
        "GenerateEndpointsAttribute`1"
        ];
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
