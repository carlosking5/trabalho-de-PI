using CineVerse.Domain;
using CineVerse.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CineVerse.Tests;

public class PlatformSchemaTests
{
    [Fact]
    public async Task UpgradePreservesExistingTitlesAndAddsDemoData()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var database = CreateContext(connection);
        await database.GetService<IMigrator>().MigrateAsync("20261123164859_InitialCatalog");
        await database.Database.ExecuteSqlRawAsync("UPDATE Titles SET Name = 'Título local' WHERE Id = 1");

        await database.Database.MigrateAsync();

        var existing = await database.Titles.SingleAsync(title => title.Id == 1);
        Assert.Equal("Título local", existing.Name);
        Assert.True(existing.IsPublished);
        Assert.Equal(3, await database.Set<Episode>().CountAsync());
        Assert.Equal(40, await database.Set<Seat>().CountAsync());
        Assert.Equal(3, await database.Set<SubscriptionPlan>().CountAsync());
    }

    [Fact]
    public async Task SqliteRejectsSecondAllocationForTheSameScreeningAndSeat()
    {
        await using var connection = await OpenDatabaseAsync();
        await using var database = CreateContext(connection);
        await database.Database.MigrateAsync();

        var now = DateTime.UtcNow;
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        database.Users.AddRange(new ApplicationUser { Id = "first" }, new ApplicationUser { Id = "second" });
        database.Add(new Screening(1, 1, 1, now.AddDays(1), 900));
        database.AddRange(
            new Reservation(first, "first", 1, ReservationStatus.Pending, now, 900),
            new Reservation(second, "second", 1, ReservationStatus.Pending, now, 900),
            new ReservationSeat(first, 1, 1, 1, 900),
            new ReservationSeat(second, 1, 1, 1, 900),
            new ScreeningSeatAllocation(1, 1, 1, first, now.AddMinutes(5)));
        await database.SaveChangesAsync();

        // Use a separate context so the database itself checks the duplicate key.
        await using var other = CreateContext(connection);
        other.Add(new ScreeningSeatAllocation(1, 1, 1, second, now.AddMinutes(5)));
        var failure = await Assert.ThrowsAsync<DbUpdateException>(() => other.SaveChangesAsync());
        Assert.Equal(1555, Assert.IsType<SqliteException>(failure.InnerException).SqliteExtendedErrorCode);
        Assert.Equal(first, (await database.Set<ScreeningSeatAllocation>().SingleAsync()).ReservationId);
    }

    private static CatalogDb CreateContext(SqliteConnection connection) =>
        new(new DbContextOptionsBuilder<CatalogDb>().UseSqlite(connection).Options);

    private static async Task<SqliteConnection> OpenDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        await connection.OpenAsync();
        return connection;
    }
}
