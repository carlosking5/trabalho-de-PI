namespace CineVerse.Domain;

public sealed record Genre(int Id, string Name);
public sealed record ContentGenre(int TitleId, int GenreId);
public sealed record Season(int Id, int TitleId, int Number);
public sealed record Episode(int Id, int SeasonId, int TitleId, int Number, string Name, string Synopsis,
    int DurationMinutes, string Image, DateTime PublishedAtUtc, string? VideoUrl = null,
    string? AudioLanguage = null, string? SubtitleUrl = null);

public sealed record Favorite(string UserId, int TitleId);
public sealed record Rating(string UserId, int TitleId, int Score);
public sealed record WatchProgress(long Id, string UserId, int TitleId, int? EpisodeId,
    int PositionSeconds, DateTime UpdatedAtUtc);
