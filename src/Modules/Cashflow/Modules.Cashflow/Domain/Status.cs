using FSH.Framework.Core.Domain;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Domain;

public sealed class Status : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Code { get; private set; }

    /// <summary>PROYECTO | INGRESO | PAGO — filters which context this status belongs to. Nullable for legacy/untyped rows.</summary>
    public StatusType? Type { get; private set; }
    public string? ColorHex { get; private set; }

    private Status() { }

    public static Status Create(string name, string? code, StatusType? type, string? colorHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Status
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Code = code?.Trim(),
            Type = type,
            ColorHex = colorHex?.Trim(),
        };
    }

    public void Update(string name, string? code, StatusType? type, string? colorHex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Code = code?.Trim();
        Type = type;
        ColorHex = colorHex?.Trim();
    }
}
