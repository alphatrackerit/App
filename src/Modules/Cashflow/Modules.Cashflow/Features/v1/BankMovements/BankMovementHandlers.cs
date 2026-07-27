using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.BankMovements;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.BankMovements;

public sealed class CreateBankMovementCommandHandler(CashflowDbContext db) : ICommandHandler<CreateBankMovementCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateBankMovementCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = BankMovement.Create(command.Date, command.Concept, command.Amount, command.Balance, command.BankName);
        db.BankMovements.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateBankMovementCommandValidator : AbstractValidator<CreateBankMovementCommand>
{
    public CreateBankMovementCommandValidator()
    {
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Concept).NotEmpty();
    }
}

public sealed class UpdateBankMovementCommandHandler(CashflowDbContext db) : ICommandHandler<UpdateBankMovementCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateBankMovementCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.BankMovements.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"BankMovement {command.Id} not found.");
        entity.Update(command.Date, command.Concept, command.Amount, command.Balance, command.BankName);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateBankMovementCommandValidator : AbstractValidator<UpdateBankMovementCommand>
{
    public UpdateBankMovementCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.BankName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Concept).NotEmpty();
    }
}

public sealed class DeleteBankMovementCommandHandler(CashflowDbContext db) : ICommandHandler<DeleteBankMovementCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteBankMovementCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.BankMovements.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"BankMovement {command.Id} not found.");
        db.BankMovements.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteBankMovementCommandValidator : AbstractValidator<DeleteBankMovementCommand>
{
    public DeleteBankMovementCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchBankMovementsQueryHandler(CashflowDbContext db) : IQueryHandler<SearchBankMovementsQuery, PagedResponse<BankMovementDto>>
{
    public async ValueTask<PagedResponse<BankMovementDto>> Handle(SearchBankMovementsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = db.BankMovements.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string t = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Concept, $"%{t}%") || EF.Functions.ILike(x.BankName, $"%{t}%"));
        }
        bool desc = !string.Equals(query.SortDir, "asc", StringComparison.OrdinalIgnoreCase);
        // Default: most-recent first.
        q = desc ? q.OrderByDescending(x => x.Date) : q.OrderBy(x => x.Date);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
        return new PagedResponse<BankMovementDto>
        {
            Items = items.Select(x => new BankMovementDto(x.Id, x.Date, x.Concept, x.Amount, x.Balance, x.BankName)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchBankMovementsQueryValidator : AbstractValidator<SearchBankMovementsQuery>
{
    public SearchBankMovementsQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
