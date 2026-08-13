using FSH.Modules.Cashflow.Contracts.Enums;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;
using FSH.Modules.Cashflow.Features.v1.Invoices.CreateInvoice;

namespace Cashflow.Tests.Features;

public sealed class CreateInvoiceCommandValidatorTests
{
    private readonly CreateInvoiceCommandValidator _validator = new();

    [Fact]
    public void Emitida_Should_RequireNumber()
    {
        var command = new CreateInvoiceCommand(InvoiceType.Emitida, Number: null, Total: 100m, ClientId: Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateInvoiceCommand.Number));
    }

    [Fact]
    public void Recibida_Should_AllowMissingNumber()
    {
        var command = new CreateInvoiceCommand(InvoiceType.Recibida, Number: null, Total: 100m, SupplierId: Guid.NewGuid());

        _validator.Validate(command).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Number_Should_RespectMaxLength_EvenWhenOptional()
    {
        var command = new CreateInvoiceCommand(
            InvoiceType.Recibida, Number: new string('X', 65), Total: 100m, SupplierId: Guid.NewGuid());

        _validator.Validate(command).IsValid.ShouldBeFalse();
    }
}
