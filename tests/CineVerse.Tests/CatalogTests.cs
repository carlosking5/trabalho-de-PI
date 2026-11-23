using CineVerse.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CineVerse.Tests;

public class CatalogTests
{
    [Fact]
    public async Task MigrationCreatesCatalogAndServiceReturnsTitlesByPopularity()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var factory = new CatalogFactory(new DbContextOptionsBuilder<CatalogDb>().UseSqlite(connection).Options);

        await using (var database = factory.CreateDbContext())
            await database.Database.MigrateAsync();

        var catalog = new SqliteCatalog(factory);
        var titles = await catalog.ListAsync();
        Assert.Equal(6, titles.Count);
        Assert.Equal("Horizonte Perdido", titles[0].Name);
        Assert.Equal(titles.OrderByDescending(title => title.Popularity), titles);

        var episodes = await catalog.RecentEpisodesAsync();
        Assert.Equal(new[] { "A luz no campo", "O último metro", "A chamada" }, episodes.Select(episode => episode.Name));
        Assert.Equal(1, episodes[0].Season);
        Assert.Equal(8, episodes[0].Number);
    }

    private sealed class CatalogFactory(DbContextOptions<CatalogDb> options) : IDbContextFactory<CatalogDb>
    {
        public CatalogDb CreateDbContext() => new(options);
    }
}
