using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Societies;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Societies;

public sealed class CreateSocietyCommandHandler(CashflowDbContext db) : ICommandHandler<CreateSocietyCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateSocietyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Society.Create(
            command.Name, command.TaxId, command.Address, command.PostalCode, command.City, command.Country, command.ClientId);
        db.Societies.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateSocietyCommandValidator : AbstractValidator<CreateSocietyCommand>
{
    public CreateSocietyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TaxId).MaximumLength(64);
        RuleFor(x => x.Address).MaximumLength(512);
        RuleFor(x => x.PostalCode).MaximumLength(32);
        RuleFor(x => x.City).MaximumLength(128);
        RuleFor(x => x.Country).MaximumLength(128);
    }
}

public sealed class UpdateSocietyCommandHandler(CashflowDbContext db) : ICommandHandler<UpdateSocietyCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateSocietyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Societies.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Society {command.Id} not found.");
        entity.Update(
            command.Name, command.TaxId, command.Address, command.PostalCode, command.City, command.Country, command.ClientId);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateSocietyCommandValidator : AbstractValidator<UpdateSocietyCommand>
{
    public UpdateSocietyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.TaxId).MaximumLength(64);
        RuleFor(x => x.Address).MaximumLength(512);
        RuleFor(x => x.PostalCode).MaximumLength(32);
        RuleFor(x => x.City).MaximumLength(128);
        RuleFor(x => x.Country).MaximumLength(128);
    }
}

public sealed class DeleteSocietyCommandHandler(CashflowDbContext db) : ICommandHandler<DeleteSocietyCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteSocietyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Societies.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Society {command.Id} not found.");
        db.Societies.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteSocietyCommandValidator : AbstractValidator<DeleteSocietyCommand>
{
    public DeleteSocietyCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchSocietiesQueryHandler(CashflowDbContext db) : IQueryHandler<SearchSocietiesQuery, PagedResponse<SocietyDto>>
{
    public async ValueTask<PagedResponse<SocietyDto>> Handle(SearchSocietiesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = db.Societies.AsNoTracking().AsQueryable();
        if (query.ClientId is not null)
        {
            q = q.Where(x => x.ClientId == query.ClientId);
        }
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string t = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{t}%"));
        }
        bool desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        q = desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
        return new PagedResponse<SocietyDto>
        {
            Items = items.Select(x => new SocietyDto(
                x.Id, x.Name, x.TaxId, x.Address, x.PostalCode, x.City, x.Country, x.ClientId)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchSocietiesQueryValidator : AbstractValidator<SearchSocietiesQuery>
{
    public SearchSocietiesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
