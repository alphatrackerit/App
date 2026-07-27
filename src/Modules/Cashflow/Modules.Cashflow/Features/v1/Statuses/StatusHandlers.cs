using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Statuses;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Statuses;

public sealed class CreateStatusCommandHandler(CashflowDbContext db) : ICommandHandler<CreateStatusCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateStatusCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Status.Create(command.Name, command.Code, command.Type, command.ColorHex);
        db.Statuses.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateStatusCommandValidator : AbstractValidator<CreateStatusCommand>
{
    public CreateStatusCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
        RuleFor(x => x.ColorHex).MaximumLength(32);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type is not null);
    }
}

public sealed class UpdateStatusCommandHandler(CashflowDbContext db) : ICommandHandler<UpdateStatusCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateStatusCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Statuses.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Status {command.Id} not found.");
        entity.Update(command.Name, command.Code, command.Type, command.ColorHex);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateStatusCommandValidator : AbstractValidator<UpdateStatusCommand>
{
    public UpdateStatusCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
        RuleFor(x => x.ColorHex).MaximumLength(32);
        RuleFor(x => x.Type).IsInEnum().When(x => x.Type is not null);
    }
}

public sealed class DeleteStatusCommandHandler(CashflowDbContext db) : ICommandHandler<DeleteStatusCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteStatusCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Statuses.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Status {command.Id} not found.");
        db.Statuses.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteStatusCommandValidator : AbstractValidator<DeleteStatusCommand>
{
    public DeleteStatusCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchStatusesQueryHandler(CashflowDbContext db) : IQueryHandler<SearchStatusesQuery, PagedResponse<StatusDto>>
{
    public async ValueTask<PagedResponse<StatusDto>> Handle(SearchStatusesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = db.Statuses.AsNoTracking().AsQueryable();
        if (query.Type is not null)
        {
            q = q.Where(x => x.Type == query.Type);
        }
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
        return new PagedResponse<StatusDto>
        {
            Items = items.Select(x => new StatusDto(x.Id, x.Name, x.Code, x.Type, x.ColorHex)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchStatusesQueryValidator : AbstractValidator<SearchStatusesQuery>
{
    public SearchStatusesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
