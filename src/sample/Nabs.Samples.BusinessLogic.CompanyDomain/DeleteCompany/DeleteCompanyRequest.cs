namespace Nabs.Samples.BusinessLogic.CompanyDomain.DeleteCompany;

[HttpDeleteEndpoint("company", "Delete a company by Id.", "Delete Company")]
public sealed class DeleteCompanyRequest : IRequest<DeleteCompanyResponse>
{
    public int CompanyId { get; set; }
}
