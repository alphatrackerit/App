using FSH.Framework.Core.Exceptions;
using FSH.Framework.Shared.Persistence;
using FluentValidation;
using FSH.Modules.Administration.Contracts.Dtos;
using FSH.Modules.Administration.Contracts.v1.Suppliers;
using FSH.Modules.Administration.Data;
using FSH.Modules.Administration.Domain;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace FSH.Modules.Administration.Features.v1.Suppliers;

public sealed class CreateSupplierCommandHandler(AdministrationDbContext db) : ICommandHandler<CreateSupplierCommand, Guid>
{
    public async ValueTask<Guid> Handle(CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = Supplier.Create(command.Name, command.Code);
        db.Suppliers.Add(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
    }
}

public sealed class UpdateSupplierCommandHandler(AdministrationDbContext db) : ICommandHandler<UpdateSupplierCommand, Guid>
{
    public async ValueTask<Guid> Handle(UpdateSupplierCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Supplier {command.Id} not found.");
        entity.Update(command.Name, command.Code);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }
}

public sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Code).MaximumLength(64);
    }
}

public sealed class DeleteSupplierCommandHandler(AdministrationDbContext db) : ICommandHandler<DeleteSupplierCommand, Unit>
{
    public async ValueTask<Unit> Handle(DeleteSupplierCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        var entity = await db.Suppliers.FirstOrDefaultAsync(x => x.Id == command.Id, cancellationToken).ConfigureAwait(false)
            ?? throw new NotFoundException($"Supplier {command.Id} not found.");
        db.Suppliers.Remove(entity);
        await db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}

public sealed class DeleteSupplierCommandValidator : AbstractValidator<DeleteSupplierCommand>
{
    public DeleteSupplierCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

public sealed class SearchSuppliersQueryHandler(AdministrationDbContext db) : IQueryHandler<SearchSuppliersQuery, PagedResponse<LookupDto>>
{
    public async ValueTask<PagedResponse<LookupDto>> Handle(SearchSuppliersQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        int page = query.PageNumber < 1 ? 1 : query.PageNumber;
        int size = query.PageSize is < 1 or > 200 ? 20 : query.PageSize;

        var q = db.Suppliers.AsNoTracking().AsQueryable();
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

public sealed class SearchSuppliersQueryValidator : AbstractValidator<SearchSuppliersQuery>
{
    public SearchSuppliersQueryValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
