using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Alimentacion;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Alimentacion.GetAlimentacionById;

public sealed class GetAlimentacionByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetAlimentacionByIdQuery, AlimentacionDto>
{
    public async ValueTask<AlimentacionDto> Handle(GetAlimentacionByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Alimentaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == query.AlimentacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Alimentacion {query.AlimentacionId} not found.");

        return new AlimentacionDto(x.Id, x.LoteId, x.Fecha, x.TipoAlimento, x.CantidadKg, x.CostoUnitario, x.Notas);
    }
}
