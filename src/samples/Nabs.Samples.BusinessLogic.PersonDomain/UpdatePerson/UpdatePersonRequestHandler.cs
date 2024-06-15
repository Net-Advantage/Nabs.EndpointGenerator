namespace Nabs.Samples.BusinessLogic.PersonDomain.UpdatePerson;

public sealed class UpdatePersonRequestHandler : IRequestHandler<UpdatePersonRequest, UpdatePersonResponse>
{
    public async Task<UpdatePersonResponse> Handle(UpdatePersonRequest request, CancellationToken cancellationToken)
    {
        var result = new UpdatePersonResponse
        {
            Success = true
        };

        return await Task.FromResult(result);
    }
}
