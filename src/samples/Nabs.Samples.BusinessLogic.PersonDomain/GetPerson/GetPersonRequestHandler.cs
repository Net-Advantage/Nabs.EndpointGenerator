namespace Nabs.Samples.BusinessLogic.PersonDomain.GetPerson;

public sealed class GetPersonRequestHandler : IRequestHandler<GetPersonRequest, GetPersonResponse>
{
    public async Task<GetPersonResponse> Handle(GetPersonRequest request, CancellationToken cancellationToken)
    {
        var result = new GetPersonResponse
        {
            PersonId = request.PersonId,
            Username = $"Username {request.PersonId}"
        };

        return await Task.FromResult(result);
    }
}
