using System.Security.Claims;
using CineVerse.Infrastructure;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CineVerse.Web.Identity;

internal sealed class AccountAuthenticationStateProvider(
    ILoggerFactory loggerFactory, IServiceScopeFactory scopes, IOptions<IdentityOptions> options)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState state, CancellationToken cancellationToken)
    {
        await using var scope = scopes.CreateAsyncScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.GetUserAsync(state.User);
        if (user is null || await users.IsLockedOutAsync(user)) return false;

        var stamp = await users.GetSecurityStampAsync(user);
        return state.User.FindFirstValue(options.Value.ClaimsIdentity.SecurityStampClaimType) == stamp;
    }
}
