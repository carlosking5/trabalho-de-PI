namespace CineVerse.Domain;

public enum ContentKind { Film, Series, Anime }

public sealed record Title(int Id, string Name, ContentKind Kind, string Genre, int Year,
    double Score, string Synopsis, string Image, int Popularity, string? Episode = null);
