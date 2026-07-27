using System.Net;
using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Prefixes;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Prefixes;

public sealed class CreatePrefixCommandHandler(CashflowDbContext db) : ICommandHandler<CreatePrefixCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreatePrefixCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Prefix.Create(command.Name, command.Description, command.PrefixGroupId, command.Type, command.IsActive);
        db.Prefixes.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreatePrefixCommandValidator : AbstractValidator<CreatePrefixCommand>
{
    public CreatePrefixCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public sealed class UpdatePrefixCommandHandler(CashflowDbContext db) : ICommandHandler<UpdatePrefixCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdatePrefixCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Prefixes.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Prefix {command.Id} not found.");

        // Business rule: a prefix that still has projects assigned cannot be deactivated.
        if (entity.IsActive && !command.IsActive)
        {
            bool hasProjects = await db.Projects.AnyAsync(p => p.PrefixId == command.Id, cancellationToken).ConfigureAwait(false);
            if (hasProjects)
            {
                throw new CustomException(
                    "Cannot deactivate a prefix that has projects assigned.",
                    Array.Empty<string>(),
                    HttpStatusCode.Conflict);
            }
        }

        entity.Update(command.Name, command.Description, command.PrefixGroupId, command.Type, command.IsActive);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdatePrefixCommandValidator : AbstractValidator<UpdatePrefixCommand>
{
    public UpdatePrefixCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Description).MaximumLength(1024);
        RuleFor(x => x.Type).IsInEnum();
    }
}

public sealed class DeletePrefixCommandHandler(CashflowDbContext db) : ICommandHandler<DeletePrefixCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeletePrefixCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Prefixes.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Prefix {command.Id} not found.");
        db.Prefixes.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeletePrefixCommandValidator : AbstractValidator<DeletePrefixCommand>
{
    public DeletePrefixCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchPrefixesQueryHandler(CashflowDbContext db) : IQueryHandler<SearchPrefixesQuery, PagedResponse<PrefixDto>>
{
    public async ValueTask<PagedResponse<PrefixDto>> Handle(SearchPrefixesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = db.Prefixes.AsNoTracking().AsQueryable();
        if (query.Type is not null)
        {
            q = q.Where(x => x.Type == query.Type);
        }
        if (query.OnlyActive == true)
        {
            q = q.Where(x => x.IsActive);
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
        return new PagedResponse<PrefixDto>
        {
            Items = items.Select(x => new PrefixDto(x.Id, x.Name, x.Description, x.PrefixGroupId, x.Type, x.IsActive)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchPrefixesQueryValidator : AbstractValidator<SearchPrefixesQuery>
{
    public SearchPrefixesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
