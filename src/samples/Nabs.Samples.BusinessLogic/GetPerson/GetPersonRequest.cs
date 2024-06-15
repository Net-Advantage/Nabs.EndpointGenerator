using MediatR;
using Nabs.EndpointGenerator.Abstractions;

namespace Nabs.Samples.BusinessLogic.GetPerson;

[HttpGetEndpoint("person/{PersonId}")]
public class GetPersonRequest : IRequest<GetPersonResponse>
{
    public int PersonId { get; set; }
}
