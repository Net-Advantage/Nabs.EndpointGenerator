namespace Nabs.EndpointGenerator.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpGetEndpointAttribute : HttpEndpointAttribute
{
	public HttpGetEndpointAttribute(string template)
	{
		_ = template;
	}
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpPostEndpointAttribute : HttpEndpointAttribute
{
    public HttpPostEndpointAttribute(string template)
    {
        _ = template;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public abstract class HttpEndpointAttribute : Attribute;