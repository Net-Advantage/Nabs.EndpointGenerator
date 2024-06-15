using MediatR;

namespace Nabs.Samples.BusinessLogic.GetPerson;

public sealed class GetPersonRequestHandler : IRequestHandler<GetPersonRequest, GetPersonResponse>
{
    public async Task<GetPersonResponse> Handle(GetPersonRequest request, CancellationToken cancellationToken)
    {
        var result = new GetPersonResponse
        {
            Value = "The Value"
        };

        return await Task.FromResult(result);
    }
}
