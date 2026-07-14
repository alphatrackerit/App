using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Supplier : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    private Supplier() { }

    public static Supplier Create(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Supplier { Id = Guid.CreateVersion7(), Name = name.Trim(), Code = code?.Trim() };
    }

    public void Update(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
    }
}
