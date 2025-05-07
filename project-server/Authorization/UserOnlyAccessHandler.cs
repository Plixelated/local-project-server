using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;

namespace project_server.Authorization
{
    public class UserOnlyRequirement : IAuthorizationRequirement { }
    public class UserOnlyAccessHandler : AuthorizationHandler<UserOnlyRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserOnlyRequirement requirement)
        {

            var userIDClaim = context.User.FindFirst("UserID")?.Value;

            if (userIDClaim == null)
                return Task.CompletedTask;

            var isAdmin = context.User.IsInRole("Admin");

            var httpContext = (context.Resource as AuthorizationFilterContext)?.HttpContext;
            var routeUserId = httpContext?.Request.RouteValues["userId"]?.ToString();

            if (userIDClaim == routeUserId || isAdmin)
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }

    }
}
