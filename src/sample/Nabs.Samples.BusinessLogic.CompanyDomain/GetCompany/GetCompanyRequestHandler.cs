
namespace Nabs.Samples.BusinessLogic.CompanyDomain.GetCompany;

public sealed class GetCompanyRequestHandler : IRequestHandler<GetCompanyRequest, GetCompanyResponse>
{
    public async Task<GetCompanyResponse> Handle(GetCompanyRequest request, CancellationToken cancellationToken)
    {
        var result = new GetCompanyResponse
        {
            CompanyId = request.CompanyId,
            CompanyName = $"CompanyName {request.CompanyId}"
        };

        return await Task.FromResult(result);
    }
}
