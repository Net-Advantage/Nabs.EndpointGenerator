namespace Nabs.Samples.BusinessLogic.PersonDomain.GetPerson;

[HttpGetEndpoint("person/{PersonId}", "Get a person by Id", "Get Person")]
public sealed class GetPersonRequest : IRequest<GetPersonResponse>
{
    public int PersonId { get; set; }
}
