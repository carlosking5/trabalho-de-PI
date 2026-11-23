using System.Net;
using System.Text.RegularExpressions;
using CineVerse.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CineVerse.Tests;

public sealed class AccountTests : IAsyncLifetime
{
    private readonly string directory = Directory.CreateTempSubdirectory("cineverse-account-").FullName;
    private WebApplicationFactory<Program> factory = null!;
    private const string Email = "espectador@example.test";
    private const string Password = "UmaPalavra!42";

    public Task InitializeAsync()
    {
        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.UseEnvironment("Development").UseSetting("ConnectionStrings:CineVerse",
                $"Data Source={Path.Combine(directory, "test.db")};Pooling=False"));
        return Task.CompletedTask;
    }

    [Fact]
    public async Task RegistrationCreatesMemberAndLoginAndLogoutUseSessionCookie()
    {
        using var client = CreateClient();
        using var registered = await SubmitAsync(client, "/conta/registar", "registar",
            ("Input.Email", Email), ("Input.Password", Password), ("Input.ConfirmPassword", Password),
            ("Input.Role", "Administrador"));
        Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);
        Assert.Contains(Email, await client.GetStringAsync("/conta"));
        Assert.Equal("/conta/acesso-negado", (await client.GetAsync("/administracao")).Headers.Location?.AbsolutePath);

        using var logout = await SubmitAsync(client, "/conta", "sair");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);
        Assert.Equal("/conta/entrar", (await client.GetAsync("/conta")).Headers.Location?.AbsolutePath);

        using var loggedIn = await SubmitAsync(client, "/conta/entrar", "entrar",
            ("Fields.Email", Email), ("Fields.Password", Password));
        Assert.Equal(HttpStatusCode.Redirect, loggedIn.StatusCode);
        Assert.Contains(Email, await client.GetStringAsync("/conta"));
    }

    [Fact]
    public async Task AdminPageRequiresAdministratorRoleAndFormsRequireAntiforgeryToken()
    {
        using var client = CreateClient();
        using var denied = await client.GetAsync("/administracao");
        Assert.Equal("/conta/entrar", denied.Headers.Location?.AbsolutePath);

        using var invalid = await client.PostAsync("/conta/entrar", new FormUrlEncodedContent(
            new Dictionary<string, string> { ["_handler"] = "entrar" }));
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var admin = new ApplicationUser { Email = "admin@example.test", UserName = "admin" };
        Assert.True((await manager.CreateAsync(admin, Password)).Succeeded);
        Assert.True((await manager.AddToRoleAsync(admin, "Administrador")).Succeeded);

        using var loggedIn = await SubmitAsync(client, "/conta/entrar", "entrar",
            ("Fields.Email", admin.Email), ("Fields.Password", Password));
        Assert.Equal(HttpStatusCode.Redirect, loggedIn.StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/administracao")).StatusCode);
    }

    private HttpClient CreateClient() => factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Fact]
    public async Task FiveWrongPasswordsTemporarilyBlockLogin()
    {
        using var client = CreateClient();
        using var registered = await SubmitAsync(client, "/conta/registar", "registar",
            ("Input.Email", Email), ("Input.Password", Password), ("Input.ConfirmPassword", Password));
        Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);
        using var logout = await SubmitAsync(client, "/conta", "sair");
        Assert.Equal(HttpStatusCode.Redirect, logout.StatusCode);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var failed = await SubmitAsync(client, "/conta/entrar", "entrar",
                ("Fields.Email", Email), ("Fields.Password", "PalavraErrada!42"));
            Assert.Equal(HttpStatusCode.OK, failed.StatusCode);
        }

        using var locked = await SubmitAsync(client, "/conta/entrar", "entrar",
            ("Fields.Email", Email), ("Fields.Password", Password));
        Assert.Equal(HttpStatusCode.OK, locked.StatusCode);
        Assert.Equal("/conta/entrar", (await client.GetAsync("/conta")).Headers.Location?.AbsolutePath);
    }

    private static async Task<HttpResponseMessage> SubmitAsync(HttpClient client, string route, string handler,
        params (string Name, string Value)[] fields)
    {
        var page = await client.GetStringAsync(route);
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.NotEmpty(token);
        var data = fields.ToDictionary(field => field.Name, field => field.Value);
        data["_handler"] = handler;
        data["__RequestVerificationToken"] = WebUtility.HtmlDecode(token);
        using var body = new FormUrlEncodedContent(data);
        return await client.PostAsync(route, body);
    }

    public async Task DisposeAsync()
    {
        await factory.DisposeAsync();
        Directory.Delete(directory, recursive: true);
    }
}
