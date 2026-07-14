using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Preparaciones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Preparaciones.GetPreparacionById;

public sealed class GetPreparacionByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetPreparacionByIdQuery, PreparacionDto>
{
    public async ValueTask<PreparacionDto> Handle(GetPreparacionByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Preparaciones
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PreparacionId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Preparacion {query.PreparacionId} not found.");

        return new PreparacionDto(
            x.Id, x.GalponId, x.LoteAnteriorId, x.FechaRetiro, x.FechaInicio, x.FechaFin,
            x.RetiradaCama, x.Lavado, x.Desinfeccion, x.Desinsectacion, x.CamaNueva, x.Costo, x.Estado, x.Notas);
    }
}
