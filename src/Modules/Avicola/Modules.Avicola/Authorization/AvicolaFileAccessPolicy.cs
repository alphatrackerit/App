using FSH.Modules.Files.Contracts;

namespace FSH.Modules.Avicola.Authorization;

/// <summary>
/// <see cref="IFileAccessPolicy"/> for Avícola documents (OwnerType = <c>AvicolaDocumento</c>):
/// feed delivery notes, cleaning certificates, mortality/health docs, order albaranes, etc.
///
/// - Attach: any authenticated user (the attach endpoint's <c>Avicola.Documentos.Create</c>
///   permission is the durable gate; orphaned uploads are reaped by the Files orphan-purge job).
/// - Read: any authenticated user in the tenant (tenant scoping is enforced by BaseDbContext).
/// - Delete: the uploader only.
/// </summary>
public sealed class AvicolaFileAccessPolicy : IFileAccessPolicy
{
    public string OwnerType => "AvicolaDocumento";

    public Task<bool> CanAttachAsync(Guid? ownerId, string currentUserId, CancellationToken cancellationToken)
        => Task.FromResult(!string.IsNullOrEmpty(currentUserId));

    public Task<bool> CanReadAsync(FileAccessContext context, string currentUserId, CancellationToken cancellationToken)
        => Task.FromResult(!string.IsNullOrEmpty(currentUserId));

    public Task<bool> CanDeleteAsync(FileAccessContext context, string currentUserId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        return Task.FromResult(
            !string.IsNullOrEmpty(currentUserId)
            && string.Equals(currentUserId, context.CreatedByUserId, StringComparison.Ordinal));
    }
}
