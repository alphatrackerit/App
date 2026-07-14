using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Sanidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Sanidad.GetSanidadById;

public sealed class GetSanidadByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetSanidadByIdQuery, SanidadDto>
{
    public async ValueTask<SanidadDto> Handle(GetSanidadByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.RegistrosSanitarios
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == query.SanidadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Sanidad {query.SanidadId} not found.");

        return new SanidadDto(x.Id, x.LoteId, x.Fecha, x.Tipo, x.Producto, x.Dosis, x.ViaAplicacion, x.Costo, x.Notas);
    }
}
