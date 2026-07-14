using FluentValidation;
using FSH.Modules.Avicola.Contracts.v1.Documentos;

namespace FSH.Modules.Avicola.Features.v1.Documentos.SearchDocumentos;

public sealed class SearchDocumentosQueryValidator : AbstractValidator<SearchDocumentosQuery>
{
    public SearchDocumentosQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
