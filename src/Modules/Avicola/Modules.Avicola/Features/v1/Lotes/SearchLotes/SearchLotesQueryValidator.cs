using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Lotes;

namespace FSH.Modules.Avicola.Features.v1.Lotes.SearchLotes;

public sealed class SearchLotesQueryValidator : AbstractValidator<SearchLotesQuery>
{
    public SearchLotesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
