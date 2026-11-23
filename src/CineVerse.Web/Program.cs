using CineVerse.Application;
using CineVerse.Infrastructure;
using CineVerse.Web.Components;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

var connection = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("CineVerse")
    ?? "Data Source=cineverse.db");
connection.ForeignKeys = true;
if (!Path.IsPathRooted(connection.DataSource))
    connection.DataSource = Path.Combine(builder.Environment.ContentRootPath, connection.DataSource);

builder.Services.AddDbContextFactory<CatalogDb>(options => options.UseSqlite(connection.ToString()));
builder.Services.AddScoped<ICatalog, SqliteCatalog>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

await using (var database = await app.Services.GetRequiredService<IDbContextFactory<CatalogDb>>().CreateDbContextAsync())
    await database.Database.MigrateAsync();

await app.RunAsync();
