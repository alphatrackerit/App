using FSH.Framework.Core.Domain;

namespace FSH.Modules.Cashflow.Domain;

// MovimientoBanco — a single imported bank-statement line. (spec §33, optional)
public sealed class BankMovement : AggregateRoot<Guid>
{
    public DateTime? Date { get; private set; }         // Fecha (Unspecified, like Income/Payment)
    public string Concept { get; private set; } = default!; // Concepto
    public decimal Amount { get; private set; }         // Importe
    public decimal Balance { get; private set; }        // Saldo
    public string BankName { get; private set; } = default!; // Banco (free-text name of the source bank)

    private BankMovement() { }

    public static BankMovement Create(DateTime? date, string concept, decimal amount, decimal balance, string bankName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concept);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        return new BankMovement
        {
            Id = Guid.CreateVersion7(),
            Date = date,
            Concept = concept.Trim(),
            Amount = amount,
            Balance = balance,
            BankName = bankName.Trim(),
        };
    }

    public void Update(DateTime? date, string concept, decimal amount, decimal balance, string bankName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(concept);
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        Date = date;
        Concept = concept.Trim();
        Amount = amount;
        Balance = balance;
        BankName = bankName.Trim();
    }
}
