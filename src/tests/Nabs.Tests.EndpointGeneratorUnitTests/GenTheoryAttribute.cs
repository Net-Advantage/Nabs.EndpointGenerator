using System.Reflection;
using Xunit.Sdk;

namespace Nabs.Tests.EndpointGeneratorUnitTests;

public class ResourceFileDataAttribute : DataAttribute
{
    private readonly string _templateName;

    public ResourceFileDataAttribute(string templateName)
    {
        _templateName = templateName;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        var data = ResourceLoader.LoadData(_templateName);

        // Flatten the array here to match the test method signature
        return data.Select(d => d); // Here 'd' should be object[] containing a single string
    }
}

public static class ResourceLoader
{
    /// <summary>
    /// Gets the data from a resource file.
    /// </summary>
    /// <param name="templateName">Name of the resource file.</param>
    /// <returns>Enumerable of object arrays with file content.</returns>
    public static IEnumerable<object[]> LoadData(string templateName)
    {
        var assembly = typeof(ResourceLoader).Assembly;
        var resourceNames = assembly.GetManifestResourceNames();
        var resourceName = resourceNames.FirstOrDefault(r => r.EndsWith(templateName))
            ?? throw new InvalidOperationException($"Resource file ending with name '{templateName}' not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
            throw new InvalidOperationException($"Could not load the resource stream for '{resourceName}'.");

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        return new[] { new object[] { content } }; // Return content of the file
    }
}