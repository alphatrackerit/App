using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Galpones;

namespace FSH.Modules.Avicola.Features.v1.Galpones.SearchGalpones;

public sealed class SearchGalponesQueryValidator : AbstractValidator<SearchGalponesQuery>
{
    public SearchGalponesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
