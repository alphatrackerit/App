using FSH.Framework.Core.Exceptions;
using FSH.Modules.Avicola.Contracts.Dtos;
using FSH.Modules.Avicola.Contracts.v1.Despachos;
using FSH.Modules.Avicola.Data;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Avicola.Features.v1.Despachos.GetDespachoById;

public sealed class GetDespachoByIdQueryHandler(AvicolaDbContext dbContext)
    : IQueryHandler<GetDespachoByIdQuery, DespachoDto>
{
    public async ValueTask<DespachoDto> Handle(GetDespachoByIdQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var x = await dbContext.Despachos
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == query.DespachoId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new NotFoundException($"Despacho {query.DespachoId} not found.");

        return new DespachoDto(x.Id, x.LoteId, x.Fecha, x.Cantidad, x.PesoTotalKg, x.PrecioPorKg, x.ClienteId, x.Notas);
    }
}
