using Application.Abstractions.Integration.Models;

namespace Application.Abstractions.Integration;

public interface IFakturaService
{
    Task<CompanyBasicDetailsDto> GetCompanyDataAsync(string companyInn);
}
