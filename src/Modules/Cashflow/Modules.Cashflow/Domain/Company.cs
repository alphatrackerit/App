using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Company : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    private Company() { }

    public static Company Create(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Company { Id = Guid.CreateVersion7(), Name = name.Trim(), Code = code?.Trim() };
    }

    public void Update(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
    }
}
