namespace Nabs.EndpointGenerator.Abstractions;

public enum HttpVerb
{
	Get,
	Post,
	Put,
	Delete
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class MethodEndpointAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public sealed class ClassEndpointsAttribute : Attribute
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
