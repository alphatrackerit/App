using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Mortalidad;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Mortalidad.GetMortalidadById;

public sealed class GetMortalidadByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetMortalidadByIdQuery, MortalidadDto>
{
    public async ValueTask<MortalidadDto> Handle(GetMortalidadByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Mortalidades
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Id == query.MortalidadId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Mortalidad {query.MortalidadId} not found.");

        return new MortalidadDto(x.Id, x.LoteId, x.Fecha, x.Cantidad, x.Descartes, x.Causa, x.Notas);
    }
}
