using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace SoitMed.Common
{
    /// <summary>
    /// Custom authorization attribute that checks roles case-insensitively
    /// </summary>
    public class CaseInsensitiveRoleAuthorizationAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _allowedRoles;

        public CaseInsensitiveRoleAuthorizationAttribute(params string[] roles)
        {
            _allowedRoles = roles ?? Array.Empty<string>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Check if user is authenticated
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(
                    ResponseHelper.CreateErrorResponse("Authentication required"));
                return;
            }

            // Get user's roles from claims
            var userRoles = context.HttpContext.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Check if user has any of the allowed roles (case-insensitive)
            var hasAccess = _allowedRoles.Any(allowedRole =>
                userRoles.Any(userRole =>
                    string.Equals(userRole, allowedRole, StringComparison.OrdinalIgnoreCase)));

            if (!hasAccess)
            {
                context.Result = new ObjectResult(
                    ResponseHelper.CreateErrorResponse($"Access denied. Required roles: {string.Join(", ", _allowedRoles)}"))
                {
                    StatusCode = 403
                };
            }
        }
    }
}

