using Microsoft.AspNetCore.Mvc;

namespace Server.Auth;

public class TechnicianUserAuthorization : TypeFilterAttribute
{
    public TechnicianUserAuthorization() : base(typeof(TechnicianUserAuthorizationFilter))
    {
    }
}
