using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Country : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;

    /// <summary>ISO code (CodigoIso in the schema).</summary>
    public string? Code { get; private set; }
    public string? Description { get; private set; }

    private Country() { }

    public static Country Create(string name, string? code, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Country
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Code = code?.Trim(),
            Description = description?.Trim(),
        };
    }

    public void Update(string name, string? code, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
        Description = description?.Trim();
    }
}
