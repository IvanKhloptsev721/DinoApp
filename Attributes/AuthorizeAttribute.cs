using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DinoApp.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly bool _requireAdmin;

    public AuthorizeAttribute(bool requireAdmin = false)
    {
        _requireAdmin = requireAdmin;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Проверяем, есть ли атрибут AllowAnonymous
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata
            .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));

        if (allowAnonymous)
            return;

        // Проверяем аутентификацию через Identity
        var user = context.HttpContext.User;
        var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;

        if (!isAuthenticated)
        {
            var returnUrl = context.HttpContext.Request.Path;
            context.Result = new RedirectResult($"/Auth/Login?returnUrl={returnUrl}");
            return;
        }

        if (_requireAdmin)
        {
            var isAdmin = user?.IsInRole("Admin") ?? false;
            if (!isAdmin)
            {
                context.Result = new RedirectResult("/Auth/AccessDenied");
                return;
            }
        }
    }
}