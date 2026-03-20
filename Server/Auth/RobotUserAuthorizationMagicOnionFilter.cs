using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using RobotShared.Model.Http;

namespace Server.Auth;

public class RobotUserAuthorizationMagicOnionFilter : MagicOnionFilterAttribute
{
    /// <summary>
    /// Task 2: Missing Unit/Integration Tests
    /// Filter was only checking UserId, No  verification for UserType which would lead to security issue .
    /// </summary>
    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        //var userIdClaim = context.CallContext.GetHttpContext().User.FindFirst("userId")?.Value;
        var httpContext = context.CallContext.GetHttpContext();
        var usrIdClaim = httpContext.User.FindFirst("userId")?.Value;
        if (usrIdClaim == null)
        {
            throw new ReturnStatusException(StatusCode.PermissionDenied, "Invalid userId");
        }
        var usrTypeClaim = httpContext.User.FindFirst("userType")?.Value;
        if(usrTypeClaim != UserType.Robot.ToString())
        {
            //In case of UserType mismatch should throw exception with Permission Denied.
            throw new ReturnStatusException(StatusCode.PermissionDenied, "Invalid userType");
        }
        await next(context);
    }
}
