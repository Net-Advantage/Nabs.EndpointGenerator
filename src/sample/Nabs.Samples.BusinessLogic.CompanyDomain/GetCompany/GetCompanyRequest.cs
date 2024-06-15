
namespace Nabs.Samples.BusinessLogic.CompanyDomain.GetCompany;

[HttpGetEndpoint("company/{CompanyId}")]
public sealed class GetCompanyRequest : IRequest<GetCompanyResponse>
{
    public int CompanyId { get; set; }
}
