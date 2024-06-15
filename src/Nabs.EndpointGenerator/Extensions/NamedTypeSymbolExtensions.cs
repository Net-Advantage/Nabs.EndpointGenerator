namespace Nabs.EndpointGenerator.Extensions;

public static class NamedTypeSymbolExtensions
{
    public static ITypeSymbol GetTypeSymbolForInterface(this INamedTypeSymbol namedTypeSymbol, string interfaceName)
    {
        foreach (var iRequestInterfaceType in namedTypeSymbol.AllInterfaces)
        {
            if (iRequestInterfaceType.IsGenericType && iRequestInterfaceType.ConstructedFrom.Name == interfaceName)
            {
                var typeArgument = iRequestInterfaceType.TypeArguments[0];
                return typeArgument;
            }
        }

        return default!;
    }

    public static AttributeData? GetAttributeByName(
        this INamedTypeSymbol namedTypeSymbol,
        string attributeNamespace,
        string baseAttributeName)
    {
        // Check all attributes applied to the type
        foreach (var attribute in namedTypeSymbol.GetAttributes())
        {
            // Get the attribute class symbol and its base types
            var attributeClass = attribute.AttributeClass;
            if (attributeClass == null)
                continue;

            // Check if the attribute's namespace matches
            if (attributeClass.ContainingNamespace.ToString() == attributeNamespace)
            {
                // Traverse the base class hierarchy to check if any base class matches the given base attribute name
                var baseType = attributeClass.BaseType;
                while (baseType != null)
                {
                    if (baseType.Name == baseAttributeName &&
                        baseType.ContainingNamespace.ToString() == attributeNamespace)
                    {
                        return attribute;
                    }
                    baseType = baseType.BaseType;
                }
            }
        }

        return null;

        //// Iterate through the attributes of the class
        //foreach (var attributeData in namedTypeSymbol.GetAttributes())
        //{
        //    // Check if the attribute is the one you're interested in (RequestEndpoint in this case)
        //    var a = attributeData.AttributeClass!.ToDisplayString();
        //    if (attributeData.AttributeClass!.ToDisplayString().EndsWith(".HttpGetEndpointAttribute"))
        //    {
        //        // Extract the constructor arguments
        //        // Assuming the first argument is the HttpVerb and the second is the endpoint template
        //        if (attributeData.ConstructorArguments.Length >= 1)
        //        {
        //            var endpointTemplate = attributeData.ConstructorArguments[0].Value!.ToString();

        //            // Use the extracted information as needed
        //            // For example, log the information, store it, or use it to generate code
        //            return endpointTemplate;
        //        }
        //    }
        //}

        //return "";
    }
}
