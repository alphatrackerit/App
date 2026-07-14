using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.SearchAlimentacion;

public sealed class SearchAlimentacionQueryValidator : AbstractValidator<SearchAlimentacionQuery>
{
    public SearchAlimentacionQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
