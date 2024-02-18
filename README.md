# Nabs.EndpointGenerator

Generate Api endpoints and client assemblies for methods in your application.

## Api Endpoint Generator

Add the `Nabs.EndpointGenerator` package to your project and use the `EndpointGenerator` to generate Api endpoints for your methods.

## Example with CQRS

You can use the `ApiEndpointGenerator` to generate Api endpoints for your CQRS command and query requests.

```csharp
public class GetPersonRequest : IRequest<GetPersonResponse>
{
	public int PersonId { get; set; }
}
```

## Api Client Generator

Add the `Nabs.EndpointGenerator.Client` package to your project and use the `ApiClientGenerator` to generate a client assembly for your Api endpoints.




