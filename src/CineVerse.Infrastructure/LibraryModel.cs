using CineVerse.Domain;
using Microsoft.EntityFrameworkCore;

namespace CineVerse.Infrastructure;

internal static class LibraryModel
{
    public static void Configure(ModelBuilder model)
    {
        var genres = model.Entity<Genre>().ToTable("Genres");
        genres.HasIndex(genre => genre.Name).IsUnique();
        genres.HasData(new Genre(1, "Ficção científica"), new Genre(2, "Suspense"),
            new Genre(3, "Aventura"), new Genre(4, "Drama"), new Genre(5, "Fantasia"));

        var links = model.Entity<ContentGenre>().ToTable("ContentGenres");
        links.HasKey(link => new { link.TitleId, link.GenreId });
        links.HasOne<Title>().WithMany().HasForeignKey(link => link.TitleId);
        links.HasOne<Genre>().WithMany().HasForeignKey(link => link.GenreId);
        links.HasData(new ContentGenre(1, 1), new ContentGenre(2, 2), new ContentGenre(3, 3),
            new ContentGenre(4, 4), new ContentGenre(5, 5), new ContentGenre(6, 1));

        var seasons = model.Entity<Season>().ToTable("Seasons",
            table => table.HasCheckConstraint("CK_Season_Number", "Number > 0"));
        seasons.HasAlternateKey(season => new { season.TitleId, season.Id });
        seasons.HasIndex(season => new { season.TitleId, season.Number }).IsUnique();
        seasons.HasOne<Title>().WithMany().HasForeignKey(season => season.TitleId);
        seasons.HasData(new Season(1, 2, 1), new Season(2, 4, 2), new Season(3, 5, 1));

        var episodes = model.Entity<Episode>().ToTable("Episodes", table =>
        {
            table.HasCheckConstraint("CK_Episode_Number", "Number > 0");
            table.HasCheckConstraint("CK_Episode_Duration", "DurationMinutes > 0");
        });
        episodes.HasIndex(episode => new { episode.SeasonId, episode.Number }).IsUnique();
        episodes.HasAlternateKey(episode => new { episode.TitleId, episode.Id });
        episodes.HasOne<Season>().WithMany()
            .HasForeignKey(episode => new { episode.TitleId, episode.SeasonId })
            .HasPrincipalKey(season => new { season.TitleId, season.Id });
        var published = new DateTime(2025, 11, 1, 12, 0, 0, DateTimeKind.Utc);
        episodes.HasData(
            new Episode(1, 1, 2, 4, "A chamada", "Uma chamada interrompe a investigação.", 45, "cidade.jpg", published),
            new Episode(2, 2, 4, 3, "O último metro", "As duas mulheres voltam à estação.", 38, "meia-noite.jpg", published.AddDays(1)),
            new Episode(3, 3, 5, 8, "A luz no campo", "Uma nova luz surge sobre a aldeia.", 24, "verao.jpg", published.AddDays(2), AudioLanguage: "ja"));

        var favorites = model.Entity<Favorite>().ToTable("Favorites");
        favorites.HasKey(item => new { item.UserId, item.TitleId });
        favorites.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.UserId);
        favorites.HasOne<Title>().WithMany().HasForeignKey(item => item.TitleId);

        var ratings = model.Entity<Rating>().ToTable("Ratings",
            table => table.HasCheckConstraint("CK_Rating_Score", "Score BETWEEN 1 AND 10"));
        ratings.HasKey(item => new { item.UserId, item.TitleId });
        ratings.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.UserId);
        ratings.HasOne<Title>().WithMany().HasForeignKey(item => item.TitleId);

        var progress = model.Entity<WatchProgress>().ToTable("WatchProgress",
            table => table.HasCheckConstraint("CK_WatchProgress_Position", "PositionSeconds >= 0"));
        progress.HasOne<ApplicationUser>().WithMany().HasForeignKey(item => item.UserId);
        progress.HasOne<Title>().WithMany().HasForeignKey(item => item.TitleId);
        progress.HasOne<Episode>().WithMany()
            .HasForeignKey(item => new { item.TitleId, item.EpisodeId })
            .HasPrincipalKey(episode => new { episode.TitleId, episode.Id })
            .OnDelete(DeleteBehavior.Cascade);
        progress.HasIndex(item => new { item.UserId, item.TitleId })
            .IsUnique().HasFilter("EpisodeId IS NULL");
        progress.HasIndex(item => new { item.UserId, item.EpisodeId })
            .IsUnique().HasFilter("EpisodeId IS NOT NULL");
    }
}
