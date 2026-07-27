using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Clients;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Clients;

public sealed class CreateClientCommandHandler(CashflowDbContext db) : ICommandHandler<CreateClientCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateClientCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Client.Create(
            command.Name, command.Code, command.TaxId, command.Address, command.ClientType,
            command.Contact, command.LegalName, command.Phone, command.Email, command.RegisteredOn, command.ColorHex);
        db.Clients.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateClientCommandValidator : AbstractValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
        RuleFor(x => x.TaxId).MaximumLength(64);
        RuleFor(x => x.Address).MaximumLength(512);
        RuleFor(x => x.ClientType).MaximumLength(128);
        RuleFor(x => x.Contact).MaximumLength(256);
        RuleFor(x => x.LegalName).MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(64);
        RuleFor(x => x.Email).MaximumLength(256);
        RuleFor(x => x.ColorHex).MaximumLength(32);
    }
}

public sealed class UpdateClientCommandHandler(CashflowDbContext db) : ICommandHandler<UpdateClientCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateClientCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Clients.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Client {command.Id} not found.");
        entity.Update(
            command.Name, command.Code, command.TaxId, command.Address, command.ClientType,
            command.Contact, command.LegalName, command.Phone, command.Email, command.RegisteredOn, command.ColorHex);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateClientCommandValidator : AbstractValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
        RuleFor(x => x.TaxId).MaximumLength(64);
        RuleFor(x => x.Address).MaximumLength(512);
        RuleFor(x => x.ClientType).MaximumLength(128);
        RuleFor(x => x.Contact).MaximumLength(256);
        RuleFor(x => x.LegalName).MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(64);
        RuleFor(x => x.Email).MaximumLength(256);
        RuleFor(x => x.ColorHex).MaximumLength(32);
    }
}

public sealed class DeleteClientCommandHandler(CashflowDbContext db) : ICommandHandler<DeleteClientCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteClientCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Clients.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Client {command.Id} not found.");
        db.Clients.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteClientCommandValidator : AbstractValidator<DeleteClientCommand>
{
    public DeleteClientCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchClientsQueryHandler(CashflowDbContext db) : IQueryHandler<SearchClientsQuery, PagedResponse<ClientDto>>
{
    public async ValueTask<PagedResponse<ClientDto>> Handle(SearchClientsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = db.Clients.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string t = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{t}%") || (x.Code != null && EF.Functions.ILike(x.Code, $"%{t}%")));
        }
        bool desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        bool byCode = string.Equals(query.SortBy, "code", StringComparison.OrdinalIgnoreCase);
        q = (byCode, desc) switch
        {
            (true, true) => q.OrderByDescending(x => x.Code),
            (true, false) => q.OrderBy(x => x.Code),
            (false, true) => q.OrderByDescending(x => x.Name),
            _ => q.OrderBy(x => x.Name),
        };

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
        return new PagedResponse<ClientDto>
        {
            Items = items.Select(x => new ClientDto(
                x.Id, x.Name, x.Code, x.TaxId, x.Address, x.ClientType,
                x.Contact, x.LegalName, x.Phone, x.Email, x.RegisteredOn, x.ColorHex)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchClientsQueryValidator : AbstractValidator<SearchClientsQuery>
{
    public SearchClientsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
