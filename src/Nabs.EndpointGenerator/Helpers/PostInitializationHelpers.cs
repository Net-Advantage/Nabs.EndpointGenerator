namespace Nabs.EndpointGenerator.Helpers;

internal static class PostInitializationHelpers
{
    internal static void InitializeGenerator(
        IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(SourceOutputAction);
    }

    private static void SourceOutputAction(IncrementalGeneratorPostInitializationContext context)
    {
        var source = @"
using System;

namespace Nabs.EndpointGenerator
{

    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateEndpointsAttribute : Attribute
    {
        public GenerateEndpointsAttribute(string assemblyToScan)
        {
            AssemblyToScan = assemblyToScan;
        }

        public string AssemblyToScan { get; }
    }


    [AttributeUsage(AttributeTargets.Class)]
    public class GenerateEndpointsAttribute<TAssemblyType> : Attribute
    {
        public GenerateEndpointsAttribute()
        {
            AssemblyToScan = typeof(TAssemblyType).Assembly.FullName;
        }

        public string AssemblyToScan { get; }
    }
}
";

        context.AddSource("Nabs.EndpointGenerator.GenerateEndpointsAttribute.g.cs", source);
    }
}