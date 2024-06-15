namespace Nabs.EndpointGenerator.Abstractions;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpGetEndpointAttribute : HttpEndpointAttribute
{
	public HttpGetEndpointAttribute(string template)
	{
		_ = template;
	}

    public HttpGetEndpointAttribute(string template, string description, string operationId)
    {
        _ = template;
        _ = description;
        _ = operationId;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpPostEndpointAttribute : HttpEndpointAttribute
{
    public HttpPostEndpointAttribute(string template)
    {
        _ = template;
    }

    public HttpPostEndpointAttribute(string template, string description, string operationId)
    {
        _ = template;
        _ = description;
        _ = operationId;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpPutEndpointAttribute : HttpEndpointAttribute
{
    public HttpPutEndpointAttribute(string template)
    {
        _ = template;
    }

    public HttpPutEndpointAttribute(string template, string description, string operationId)
    {
        _ = template;
        _ = description;
        _ = operationId;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class HttpDeleteEndpointAttribute : HttpEndpointAttribute
{
    public HttpDeleteEndpointAttribute(string template)
    {
        _ = template;
    }

    public HttpDeleteEndpointAttribute(string template, string description, string operationId)
    {
        _ = template;
        _ = description;
        _ = operationId;
    }
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public abstract class HttpEndpointAttribute : Attribute;
