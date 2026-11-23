using CineVerse.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CineVerse.Web.Identity;

internal static class Accounts
{
    public const string Member = "Utilizador";
    public const string Admin = "Administrador";

    public static ApplicationUser NewUser(string email) => new()
    {
        Email = email.Trim(),
        UserName = Guid.NewGuid().ToString("N")
    };

    public static async Task InitializeAsync(IServiceProvider services, bool createAdmin)
    {
        await using var scope = services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<CatalogDb>();
        await database.Database.MigrateAsync();
        await using var transaction = await database.Database.BeginTransactionAsync();
        var roles = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var name in new[] { Member, Admin })
        {
            if (!await roles.RoleExistsAsync(name))
                RequireSuccess(await roles.CreateAsync(new IdentityRole(name)));
        }

        if (createAdmin)
            await CreateAdministratorAsync(scope.ServiceProvider);

        await transaction.CommitAsync();
        if (createAdmin) Console.WriteLine("Conta de administrador criada.");
    }

    private static async Task CreateAdministratorAsync(IServiceProvider services)
    {
        var email = Environment.GetEnvironmentVariable("CINEVERSE_ADMIN_EMAIL");
        var password = Environment.GetEnvironmentVariable("CINEVERSE_ADMIN_PASSWORD");
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
            throw new InvalidOperationException("Define CINEVERSE_ADMIN_EMAIL e CINEVERSE_ADMIN_PASSWORD.");

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        var user = NewUser(email);
        RequireSuccess(await users.CreateAsync(user, password));
        RequireSuccess(await users.AddToRoleAsync(user, Admin));
    }

    private static void RequireSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
    }
}
