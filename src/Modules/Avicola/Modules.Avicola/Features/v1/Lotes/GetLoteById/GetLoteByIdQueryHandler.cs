using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Lotes;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Lotes.GetLoteById;

public sealed class GetLoteByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetLoteByIdQuery, LoteDto>
{
    public async ValueTask<LoteDto> Handle(GetLoteByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == query.LoteId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Lote {query.LoteId} not found.");

        return new LoteDto(
            x.Id, x.Codigo, x.GalponId, x.Raza, x.FechaIngreso, x.CantidadInicial, x.PesoInicialGramos,
            x.FechaSalidaPrevista, x.FechaSalidaReal, x.Estado, x.ProveedorId, x.CostoPolluelo, x.Notas);
    }
}
