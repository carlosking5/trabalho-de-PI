using CineVerse.Application;
using CineVerse.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace CineVerse.Infrastructure;

public sealed class CatalogDb(DbContextOptions<CatalogDb> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Title> Titles => Set<Title>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        LibraryModel.Configure(model);
        BookingModel.Configure(model);
        AccountModel.Configure(model);

        var titles = model.Entity<Title>();
        titles.Property(title => title.IsPublished).HasDefaultValue(true);
        titles.HasIndex(title => title.Popularity);
        titles.Property(title => title.Kind).HasConversion<string>();
        titles.HasData(
            new Title(1, "Horizonte Perdido", ContentKind.Film, "Ficção científica", 2025, 8.9,
                "Uma astronauta deteta um sinal impossível junto de uma estrela em colapso e tem de decidir se regressa à Terra ou segue a transmissão.", "horizonte.jpg", 100),
            new Title(2, "Cidade em Silêncio", ContentKind.Series, "Suspense", 2025, 8.6,
                "Um detetive investiga uma série de desaparecimentos nas ruas de Lisboa.", "cidade.jpg", 95, "T01 · EP04"),
            new Title(3, "Acima das Nuvens", ContentKind.Film, "Aventura", 2024, 8.1,
                "Uma expedição fica isolada na montanha quando a tempestade apaga o caminho de regresso.", "nuvens.jpg", 90),
            new Title(4, "Depois da Meia-Noite", ContentKind.Series, "Drama", 2025, 8.4,
                "Duas desconhecidas encontram-se no último metro e reconstroem uma noite que mudou as suas vidas.", "meia-noite.jpg", 85, "T02 · EP03"),
            new Title(5, "O Verão das Estrelas", ContentKind.Anime, "Fantasia", 2025, 8.7,
                "Durante as férias, um rapaz segue uma estrela que caiu nos campos da sua aldeia.", "verao.jpg", 80, "T01 · EP08"),
            new Title(6, "Última Órbita", ContentKind.Film, "Ficção científica", 2024, 7.9,
                "Um piloto aceita uma última missão para resgatar uma tripulação em órbita.", "orbita.jpg", 75));
    }
}

public sealed class SqliteCatalog(IDbContextFactory<CatalogDb> factory) : ICatalog
{
    public async Task<IReadOnlyList<Title>> ListAsync(CancellationToken cancellationToken = default)
    {
        await using var database = await factory.CreateDbContextAsync(cancellationToken);
        return await database.Titles.AsNoTracking().Where(title => title.IsPublished)
            .OrderByDescending(title => title.Popularity).ThenBy(title => title.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<EpisodeRelease>> RecentEpisodesAsync(CancellationToken cancellationToken = default)
    {
        await using var database = await factory.CreateDbContextAsync(cancellationToken);
        var now = DateTime.UtcNow;
        return await (
            from episode in database.Set<Episode>().AsNoTracking()
            join season in database.Set<Season>() on episode.SeasonId equals season.Id
            join title in database.Titles on episode.TitleId equals title.Id
            where title.IsPublished && episode.PublishedAtUtc <= now
            orderby episode.PublishedAtUtc descending, episode.Id descending
            select new EpisodeRelease(title, season.Number, episode.Number, episode.Name, episode.Image)
        ).Take(4).ToListAsync(cancellationToken);
    }
}
