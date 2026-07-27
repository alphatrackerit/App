using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.v1.Banks;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Banks;

public sealed class CreateBankCommandHandler(CashflowDbContext db) : ICommandHandler<CreateBankCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateBankCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Bank.Create(command.Name, command.StartRow, command.DateColumn, command.ConceptColumn, command.AmountColumn, command.BalanceColumn, command.IsActive, command.Notes);
        db.Banks.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateBankCommandValidator : AbstractValidator<CreateBankCommand>
{
    public CreateBankCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.StartRow).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DateColumn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ConceptColumn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AmountColumn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BalanceColumn).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateBankCommandHandler(CashflowDbContext db) : ICommandHandler<UpdateBankCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateBankCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Banks.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Bank {command.Id} not found.");
        entity.Update(command.Name, command.StartRow, command.DateColumn, command.ConceptColumn, command.AmountColumn, command.BalanceColumn, command.IsActive, command.Notes);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateBankCommandValidator : AbstractValidator<UpdateBankCommand>
{
    public UpdateBankCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.StartRow).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DateColumn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ConceptColumn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.AmountColumn).GreaterThanOrEqualTo(0);
        RuleFor(x => x.BalanceColumn).GreaterThanOrEqualTo(0);
    }
}

public sealed class DeleteBankCommandHandler(CashflowDbContext db) : ICommandHandler<DeleteBankCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteBankCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Banks.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Bank {command.Id} not found.");
        db.Banks.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteBankCommandValidator : AbstractValidator<DeleteBankCommand>
{
    public DeleteBankCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchBanksQueryHandler(CashflowDbContext db) : IQueryHandler<SearchBanksQuery, PagedResponse<BankDto>>
{
    public async ValueTask<PagedResponse<BankDto>> Handle(SearchBanksQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 10000 ? 20 : query.PageSize;

        var q = db.Banks.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string t = query.Search.Trim();
            q = q.Where(x => EF.Functions.ILike(x.Name, $"%{t}%"));
        }
        bool desc = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        q = desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name);

        long total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        var items = await q.Skip((page - 1) * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
        return new PagedResponse<BankDto>
        {
            Items = items.Select(x => new BankDto(x.Id, x.Name, x.StartRow, x.DateColumn, x.ConceptColumn, x.AmountColumn, x.BalanceColumn, x.IsActive, x.Notes)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchBanksQueryValidator : AbstractValidator<SearchBanksQuery>
{
    public SearchBanksQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 10000);
    }
}
