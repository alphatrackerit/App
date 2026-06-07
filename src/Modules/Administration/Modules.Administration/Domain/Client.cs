using FSH.Framework.Core.Domain;

namespace FSH.Modules.Administration.Domain;

public sealed class Client : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    private Client() { }

    public static Client Create(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Client { Id = Guid.CreateVersion7(), Name = name.Trim(), Code = code?.Trim() };
    }

    public void Update(string name, string? code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
    }
}
