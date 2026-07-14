namespace FSH.Modules.Cashflow.Contracts.Dtos;

/// <summary>Uniform shape for the simple lookup catalogs (Clients, Suppliers, Countries, Statuses, Companies).</summary>
public sealed record LookupDto(Guid Id, string Name, string? Code);
