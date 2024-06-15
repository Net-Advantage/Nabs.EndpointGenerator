namespace Nabs.Samples.BusinessLogic.PersonDomain.UpdatePerson;

[HttpPostEndpoint("person", "Update the person", "Update Person")]
public sealed class UpdatePersonRequest : IRequest<UpdatePersonResponse>
{
    public int PersonId { get; set; }
    public string Username { get; set; } = "";
}
