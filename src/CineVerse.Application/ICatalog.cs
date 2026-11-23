using CineVerse.Domain;

namespace CineVerse.Application;

public interface ICatalog
{
    Task<IReadOnlyList<Title>> ListAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<EpisodeRelease>> RecentEpisodesAsync(CancellationToken cancellationToken = default);
}

public sealed record EpisodeRelease(Title Title, int Season, int Number, string Name, string Image);
