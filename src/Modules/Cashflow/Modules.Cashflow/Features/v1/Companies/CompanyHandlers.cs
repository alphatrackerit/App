using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Cashflow.Contracts.Dtos;
using FSH.Modules.Cashflow.Contracts.v1.Companies;
using FSH.Modules.Cashflow.Data;
using FSH.Modules.Cashflow.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Cashflow.Features.v1.Companies;

public sealed class CreateCompanyCommandHandler(CashflowDbContext db) : ICommandHandler<CreateCompanyCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateCompanyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Company.Create(command.Name, command.Code);
        db.Companies.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
    }
}

public sealed class UpdateCompanyCommandHandler(CashflowDbContext db) : ICommandHandler<UpdateCompanyCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateCompanyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Companies.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Company {command.Id} not found.");
        entity.Update(command.Name, command.Code);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
    }
}

public sealed class DeleteCompanyCommandHandler(CashflowDbContext db) : ICommandHandler<DeleteCompanyCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteCompanyCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Companies.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Company {command.Id} not found.");
        db.Companies.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteCompanyCommandValidator : AbstractValidator<DeleteCompanyCommand>
{
    public DeleteCompanyCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchCompaniesQueryHandler(CashflowDbContext db) : IQueryHandler<SearchCompaniesQuery, PagedResponse<LookupDto>>
{
    public async ValueTask<PagedResponse<LookupDto>> Handle(SearchCompaniesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = db.Companies.AsNoTracking().AsQueryable();
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
        return new PagedResponse<LookupDto>
        {
            Items = items.Select(x => new LookupDto(x.Id, x.Name, x.Code)).ToList(),
            PageNumber = page,
            PageSize = size,
            TotalCount = total,
            TotalPages = (int)Math.Ceiling(total / (double)size),
        };
    }
}

public sealed class SearchCompaniesQueryValidator : AbstractValidator<SearchCompaniesQuery>
{
    public SearchCompaniesQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
