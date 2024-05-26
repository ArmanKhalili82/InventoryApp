using Application.Extensions.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace WebUI.Shared.Identity
{
    internal static class SignOutEnpoint
    {
        public static IEndpointConventionBuilder MapSignOutEnpoint(this IEndpointConventionBuilder endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            var accountGroup = endpoint.MapGroup("/Account");
            accountGroup.MapPost("/Logout", async (ClaimsPrincipal user, SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.LocalRedirect("/Account/Login");
            });
            return accountGroup;
        }
    }
}
