using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MusicPlatform.UI.Services;

namespace MusicPlatform.UI.Filters;


public class RequireLoginAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var tokenStore = context.HttpContext.RequestServices.GetRequiredService<ITokenStore>();

        if (!tokenStore.IsAuthenticated)
        {
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("Login", "Auth", new { returnUrl });
        }

        base.OnActionExecuting(context);
    }
}