using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Invoices;

namespace FSH.Modules.Cashflow.Features.v1.Invoices.ApplyLinkSuggestions;

public sealed class ApplyLinkSuggestionsCommandValidator : AbstractValidator<ApplyLinkSuggestionsCommand>
{
    public ApplyLinkSuggestionsCommandValidator()
    {
        RuleFor(x => x.Pairs).NotEmpty();
        RuleFor(x => x.Pairs.Count).LessThanOrEqualTo(5000);
        RuleForEach(x => x.Pairs).ChildRules(p =>
        {
            p.RuleFor(x => x.InvoiceId).NotEmpty();
            p.RuleFor(x => x.LineId).NotEmpty();
            p.RuleFor(x => x.Kind).IsInEnum();
        });
    }
}
