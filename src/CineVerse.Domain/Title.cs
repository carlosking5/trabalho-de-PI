namespace CineVerse.Domain;

public enum ContentKind { Film, Series, Anime }

public sealed record Title(int Id, string Name, ContentKind Kind, string Genre, int Year,
    double Score, string Synopsis, string Image, int Popularity, string? Episode = null)
{
    public int? DurationMinutes { get; init; }
    public string? AgeRating { get; init; }
    public string? Country { get; init; }
    public string? Language { get; init; }
    public string? Cast { get; init; }
    public string? Director { get; init; }
    public string? Banner { get; init; }
    public string? TrailerUrl { get; init; }
    public string? VideoUrl { get; init; }
    public string? SeriesStatus { get; init; }
    public DateTime? PublishedAtUtc { get; init; }
    public long ViewCount { get; init; }
    public bool IsPublished { get; init; } = true;
}
