using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RobotShared.Model.Http;

namespace Server.Auth;

public class TechnicianUserAuthorizationFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var claim = context.HttpContext.User.FindFirst("userType")?.Value;
        if (claim != UserType.Technician.ToString())
        {
            context.Result = new UnauthorizedResult();
            return;
        }
    }
}
