using FSH.Framework.Core.Domain;

namespace FSH.Modules.Administration.Domain;

public sealed class Country : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    private Country() { }

    public static Country Create(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Country { Id = Guid.CreateVersion7(), Name = name.Trim(), Code = code?.Trim() };
    }

    public void Update(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
    }
}
