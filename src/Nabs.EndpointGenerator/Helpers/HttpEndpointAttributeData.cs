namespace Nabs.EndpointGenerator.Helpers;

public sealed class HttpEndpointAttributeData
{
    public string RouteTemplate { get; set; } = default!;
    public string? Description { get; set; }
    public string? OperationId { get; set; }
}