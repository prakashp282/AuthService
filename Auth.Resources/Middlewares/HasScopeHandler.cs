using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Resources
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            // if (!context.User.HasClaim(c => c.Type == "scope" && c.Issuer == requirement.Issuer))
            // {
            //     return Task.CompletedTask;
            // }

            // Split the scopes string into an array
            var scopes = context?.User?.FindAll(c => c.Type == "scope" && c.Issuer == requirement.Issuer);
            // Succeed if the scope array contains the required scope
            if (scopes?.Any(s => s.Value == requirement.Scope)?? false)
            {
                context.Succeed(requirement);
            }
            
            // If user does not have the permission claim, get out of here
            // if (!context.User.HasClaim(c => c.Type == "permissions" && c.Issuer == requirement.Issuer))
            // {
            //     return Task.CompletedTask;
            // }

            System.Collections.Generic.IEnumerable<System.Security.Claims.Claim> permissions = context?.User?.FindAll(c => c.Type == "permissions" && c.Issuer == requirement.Issuer);
            // Succeed if the scope array contains the required scope
            if (permissions.Any(p => p.Value == requirement.Scope))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}