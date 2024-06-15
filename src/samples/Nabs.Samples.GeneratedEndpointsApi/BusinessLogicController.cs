using Microsoft.AspNetCore.Mvc;
using Nabs.EndpointGenerator;
using Nabs.Samples.BusinessLogic;
using System;

namespace Nabs.Samples.GeneratedEndpointsApi;

[ApiController]
[Route("[controller]")]
[GenerateEndpoints<Nabs.Samples.BusinessLogic.Person>()]
public partial class BusinessLogicController : ControllerBase
{
    
}
