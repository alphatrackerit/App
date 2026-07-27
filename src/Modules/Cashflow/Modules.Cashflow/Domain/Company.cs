using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Company : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    public string? LegalName { get; private set; }          // RazonSocial
    public string? TaxRegistration { get; private set; }    // RegistroFiscal
    public bool ShowInProjects { get; private set; }        // VisibleEnProyectos — appears as a card in Proyectos

    private Company() { }

    public static Company Create(string name, string? code, string? legalName, string? taxRegistration, bool showInProjects = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Company
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Code = code?.Trim(),
            LegalName = legalName?.Trim(),
            TaxRegistration = taxRegistration?.Trim(),
            ShowInProjects = showInProjects,
        };
    }

    public void Update(string name, string? code, string? legalName, string? taxRegistration, bool showInProjects)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
        LegalName = legalName?.Trim();
        TaxRegistration = taxRegistration?.Trim();
        ShowInProjects = showInProjects;
    }
}
