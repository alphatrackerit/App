using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Pesos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Pesos.GetPesoById;

public sealed class GetPesoByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetPesoByIdQuery, PesoDto>
{
    public async ValueTask<PesoDto> Handle(GetPesoByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Pesos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == query.PesoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Peso {query.PesoId} not found.");

        return new PesoDto(x.Id, x.LoteId, x.Fecha, x.PesoPromedioGramos, x.CantidadMuestra, x.Notas);
    }
}
