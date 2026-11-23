using CineVerse.Domain;

namespace CineVerse.Application;

public interface ICatalog
{
    Task<IReadOnlyList<Title>> ListAsync(CancellationToken cancellationToken = default);
}
