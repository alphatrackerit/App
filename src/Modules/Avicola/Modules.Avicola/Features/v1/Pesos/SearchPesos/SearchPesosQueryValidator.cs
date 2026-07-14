using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Pesos;

namespace FSH.Modules.Avicola.Features.v1.Pesos.SearchPesos;

public sealed class SearchPesosQueryValidator : AbstractValidator<SearchPesosQuery>
{
    public SearchPesosQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
