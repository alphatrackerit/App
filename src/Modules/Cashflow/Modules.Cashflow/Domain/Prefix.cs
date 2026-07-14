using FSH.Framework.Core.Domain;
using FSH.Modules.Cashflow.Contracts.Enums;

namespace FSH.Modules.Cashflow.Domain;

/// <summary>Hierarchical project categorization: a GRUPO or a CATEGORIA that points at its group.</summary>
public sealed class Prefix : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    /// <summary>Parent group (self-reference, ON DELETE NO ACTION). Null for a GRUPO.</summary>
    public Guid? PrefixGroupId { get; private set; }

    public PrefixType Type { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Prefix() { }

    public static Prefix Create(string name, string? description, Guid? prefixGroupId, PrefixType type, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Prefix
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            Description = description?.Trim(),
            PrefixGroupId = prefixGroupId,
            Type = type,
            IsActive = isActive,
        };
    }

    public void Update(string name, string? description, Guid? prefixGroupId, PrefixType type, bool isActive)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        Description = description?.Trim();
        PrefixGroupId = prefixGroupId;
        Type = type;
        IsActive = isActive;
    }
}
