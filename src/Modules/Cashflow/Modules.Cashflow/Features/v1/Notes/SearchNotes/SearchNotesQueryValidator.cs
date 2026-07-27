using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Notes;

namespace FSH.Modules.Cashflow.Features.v1.Notes.SearchNotes;

public sealed class SearchNotesQueryValidator : AbstractValidator<SearchNotesQuery>
{
    public SearchNotesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
