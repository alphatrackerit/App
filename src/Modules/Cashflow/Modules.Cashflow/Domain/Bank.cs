using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

// Banco — column-mapping configuration used to import a bank's statement (spreadsheet/CSV):
// which row the data starts on and which column holds each field. (spec §33, optional)
public sealed class Bank : AggregateRoot<Guid>
{
    public string Name { get; private set; } = default!;   // Nombre
    public int StartRow { get; private set; }               // FilaInicio
    public int DateColumn { get; private set; }             // FechaColumna
    public int ConceptColumn { get; private set; }          // ConceptoColumna
    public int AmountColumn { get; private set; }           // ImporteColumna
    public int BalanceColumn { get; private set; }          // SaldoColumna
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }              // Notas

    private Bank() { }

    public static Bank Create(string name, int startRow, int dateColumn, int conceptColumn, int amountColumn, int balanceColumn, bool isActive, string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Bank
        {
            Id = Guid.CreateVersion7(),
            Name = name.Trim(),
            StartRow = startRow,
            DateColumn = dateColumn,
            ConceptColumn = conceptColumn,
            AmountColumn = amountColumn,
            BalanceColumn = balanceColumn,
            IsActive = isActive,
            Notes = notes?.Trim(),
        };
    }

    public void Update(string name, int startRow, int dateColumn, int conceptColumn, int amountColumn, int balanceColumn, bool isActive, string? notes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name.Trim();
        StartRow = startRow;
        DateColumn = dateColumn;
        ConceptColumn = conceptColumn;
        AmountColumn = amountColumn;
        BalanceColumn = balanceColumn;
        IsActive = isActive;
        Notes = notes?.Trim();
    }
}
