namespace Nabs.Samples.BusinessLogic.PersonDomain.UpdatePerson;

[HttpPostEndpoint("/person")]
public sealed class UpdatePersonRequest : IRequest<UpdatePersonResponse>
{
    public int PersonId { get; set; }
    public string Username { get; set; } = "";
}
