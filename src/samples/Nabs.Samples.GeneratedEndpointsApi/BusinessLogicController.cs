using Microsoft.AspNetCore.Mvc;
using Nabs.EndpointGenerator.Abstractions;
using Nabs.Samples.BusinessLogic;

namespace Nabs.Samples.GeneratedEndpointsApi;


[ApiController]
[Route("[controller]")]
[RequestEndpointController("Nabs.Samples.BusinessLogic")]
public partial class BusinessLogicController : ControllerBase
{
	[HttpGet("api/person")]
	public IActionResult GetPerson()
	{
		return Ok(new Person { Id = 1, Name = "John Doe" });
	}
}
