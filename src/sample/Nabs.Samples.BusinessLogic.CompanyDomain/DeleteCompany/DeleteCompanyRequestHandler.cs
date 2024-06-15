namespace Nabs.Samples.BusinessLogic.CompanyDomain.DeleteCompany;

public sealed class DeleteCompanyRequestHandler : IRequestHandler<DeleteCompanyRequest, DeleteCompanyResponse>
{
    public async Task<DeleteCompanyResponse> Handle(DeleteCompanyRequest request, CancellationToken cancellationToken)
    {
        var result = new DeleteCompanyResponse();
        return await Task.FromResult(result);
    }
}
