using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.GenerateMilestones;

public sealed class GenerateMilestonesCommandValidator : AbstractValidator<GenerateMilestonesCommand>
{
    public GenerateMilestonesCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty();
    }
}
