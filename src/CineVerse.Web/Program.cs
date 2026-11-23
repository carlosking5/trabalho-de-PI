using CineVerse.Application;
using CineVerse.Infrastructure;
using CineVerse.Web.Components;
using CineVerse.Web.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var createAdmin = args is ["--criar-admin"];
var builder = WebApplication.CreateBuilder(createAdmin ? [] : args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, AccountAuthenticationStateProvider>();
builder.Services.AddAuthorization();

var connection = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("CineVerse")
    ?? "Data Source=cineverse.db")
{
    ForeignKeys = true
};
if (!Path.IsPathRooted(connection.DataSource))
    connection.DataSource = Path.Combine(builder.Environment.ContentRootPath, connection.DataSource);

builder.Services.AddDbContextFactory<CatalogDb>(options => options.UseSqlite(connection.ToString()));
builder.Services.AddScoped<ICatalog, SqliteCatalog>();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 10;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<CatalogDb>()
    .AddErrorDescriber<PortugueseIdentityErrors>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/conta/entrar";
    options.AccessDeniedPath = "/conta/acesso-negado";
    options.Cookie.Name = "CineVerse.Sessao";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
});

var app = builder.Build();
await Accounts.InitializeAsync(app.Services, createAdmin);
if (createAdmin) return;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/erro", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

await app.RunAsync();

public partial class Program;
