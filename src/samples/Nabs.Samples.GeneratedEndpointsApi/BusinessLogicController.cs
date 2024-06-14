using Microsoft.AspNetCore.Mvc;
using Nabs.EndpointGenerator.Abstractions;
using Nabs.Samples.BusinessLogic;
using System;

namespace Nabs.Samples.GeneratedEndpointsApi;

[ApiController]
[Route("[controller]")]
[RequestEndpointController<Nabs.Samples.BusinessLogic.Person>()]
public partial class BusinessLogicController : ControllerBase
{
    [HttpGet("api/person")]
    public IActionResult GetPerson()
    {
        var a = new Person();

        return Ok(new Person { Id = 1, Name = "John Doe" });
    }
}
