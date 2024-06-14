namespace Nabs.EndpointGenerator.Abstractions;

[AttributeUsage(AttributeTargets.Class)]
public class RequestEndpointControllerAttribute : Attribute
{
	public RequestEndpointControllerAttribute(string assemblyToScan)
	{
		AssemblyToScan = assemblyToScan;
	}

	public string AssemblyToScan { get; }
}


[AttributeUsage(AttributeTargets.Class)]
public class RequestEndpointControllerAttribute<TAssemblyType> : Attribute
{
    public RequestEndpointControllerAttribute()
    {
        AssemblyToScan = typeof(TAssemblyType).Assembly.FullName;
    }

    public string AssemblyToScan { get; }
}