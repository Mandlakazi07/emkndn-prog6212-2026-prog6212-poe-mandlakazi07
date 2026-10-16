// Student:     Mandlakazi Sibiya (ST10473547)
// Module:      PROG6212 Programming 2B - PoE Part 2
// Institution: The Independent Institute of Education (IIE)
// Date:        16 October 2026
// AI use:      Claude (Anthropic) helped draft and explain this file. Reviewed and understood by the student.
// Reference:   Microsoft Learn, n.d. Session and state management in ASP.NET Core. [online] Available at:
//              https://learn.microsoft.com/aspnet/core/fundamentals/app-state [Accessed 16 October 2026].

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Security;

// Checks the session before the endpoint runs.
// No allowed roles listed = any logged-in user. Roles listed = only those roles.
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;

    public SessionAuthorizeAttribute(params string[] allowedRoles)
    {
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = context.HttpContext.Session.GetInt32(SessionKeys.UserId);
        var role = context.HttpContext.Session.GetString(SessionKeys.Role);

        // Nobody is logged in -> 401 Unauthorized
        if (userId is null || role is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "You must be logged in." });
            return;
        }

        // Logged in, but the wrong role -> 403 Forbidden
        if (_allowedRoles.Length > 0 && !_allowedRoles.Contains(role))
        {
            context.Result = new ObjectResult(new { message = "You do not have permission to do this." })
            {
                StatusCode = StatusCodes.Status403Forbidden
            };
        }
    }
}
