namespace Nabs.EndpointGenerator.Abstractions;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MethodEndpointAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public sealed class ClassEndpointsAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Class)]
public class HttpGetEndpointAttribute : Attribute
{
	public HttpGetEndpointAttribute(string template)
	{
		_ = template;
	}
}
