using FSH.Framework.Shared.Persistence;
using Mediator;

namespace FSH.Modules.Cashflow.Contracts.v1.Companies;

public sealed record CreateCompanyCommand(
    string Name,
    string? Code = null,
    string? LegalName = null,
    string? TaxRegistration = null,
    bool ShowInProjects = true,
    string? Nif = null,
    string? Address = null,
    string? PostalCode = null,
    string? City = null,
    string? Phone = null,
    string? Email = null) : ICommand<Guid>;

public sealed record UpdateCompanyCommand(
    Guid Id,
    string Name,
    string? Code = null,
    string? LegalName = null,
    string? TaxRegistration = null,
    bool ShowInProjects = true,
    string? Nif = null,
    string? Address = null,
    string? PostalCode = null,
    string? City = null,
    string? Phone = null,
    string? Email = null) : ICommand<Guid>;

public sealed record DeleteCompanyCommand(Guid Id) : ICommand<Unit>;

/// <summary>Uploads (or replaces) the company logo shown on invoice/proforma PDFs. Returns the
/// stored path.</summary>
public sealed record SetCompanyLogoCommand(
    Guid CompanyId,
    byte[] Content,
    string FileName,
    string ContentType) : ICommand<string>;

public sealed record CompanyDto(
    Guid Id, string Name, string? Code, string? LegalName, string? TaxRegistration, bool ShowInProjects,
    string? Nif, string? Address, string? PostalCode, string? City, string? Phone, string? Email, string? LogoPath);

public sealed record SearchCompaniesQuery(
    string? Search = null,
    int PageNumber = 1,
    int PageSize = 20,
    string? SortBy = null,
    string? SortDir = null,
    bool? ShowInProjects = null) : IQuery<PagedResponse<CompanyDto>>;
