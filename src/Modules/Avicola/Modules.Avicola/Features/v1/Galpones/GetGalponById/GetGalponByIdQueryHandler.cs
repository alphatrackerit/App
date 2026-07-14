using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Galpones;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Galpones.GetGalponById;

public sealed class GetGalponByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetGalponByIdQuery, GalponDto>
{
    public async ValueTask<GalponDto> Handle(GetGalponByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Galpones
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == query.GalponId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Galpon {query.GalponId} not found.");

        return new GalponDto(
            x.Id, x.Nombre, x.Codigo, x.Capacidad, x.SuperficieM2, x.Ubicacion, x.Activo, x.Notas);
    }
}
