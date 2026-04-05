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
        var isAuthenticated = context.HttpContext.Session.GetString("UserId") != null;

        if (!isAuthenticated)
        {
            // Сохраняем URL, куда хотел перейти пользователь
            var returnUrl = context.HttpContext.Request.Path;
            context.Result = new RedirectResult($"/Auth/Login?returnUrl={returnUrl}");
            return;
        }

        if (_requireAdmin)
        {
            var isAdmin = context.HttpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                context.Result = new RedirectResult("/Auth/AccessDenied");
                return;
            }
        }
    }
}