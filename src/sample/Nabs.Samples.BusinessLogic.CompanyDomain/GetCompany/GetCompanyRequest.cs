namespace Nabs.Samples.BusinessLogic.CompanyDomain.GetCompany;

[HttpGetEndpoint("company/{CompanyId}", "Get a company by Id.", "Get Company")]
public sealed class GetCompanyRequest : IRequest<GetCompanyResponse>
{
    public int CompanyId { get; set; }
}
