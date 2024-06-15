namespace Nabs.Samples.BusinessLogic.PersonDomain.GetPerson;

[HttpGetEndpoint("person/{PersonId}")]
public sealed class GetPersonRequest : IRequest<GetPersonResponse>
{
    public int PersonId { get; set; }
}
