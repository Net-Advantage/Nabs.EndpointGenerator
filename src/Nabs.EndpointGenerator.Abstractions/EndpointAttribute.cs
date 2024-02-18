namespace Nabs.EndpointGenerator.Abstractions;

public enum HttpVerb
{
	Get,
	Post,
	Put,
	Delete
}

[AttributeUsage(AttributeTargets.Method)]
public class MethodToEndpointAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public class ClassToEndpointsAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public class RequestEndpointAttribute : Attribute
{
	public RequestEndpointAttribute(string httpVerb, string template)
	{
		HttpVerb = httpVerb;
		Template = template;
	}

	public string HttpVerb { get; }
	public string Template { get; }
}

[AttributeUsage(AttributeTargets.Class)]
public class RequestEndpointControllerAttribute : Attribute
{
	public RequestEndpointControllerAttribute(string assemblyToScan)
	{
		AssemblyToScan = assemblyToScan;
	}

	public string AssemblyToScan { get; }
}