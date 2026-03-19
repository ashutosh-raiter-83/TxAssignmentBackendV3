using Grpc.Core;
using MagicOnion;
using MagicOnion.Server;
using RobotShared.Model.Http;

namespace Server.Auth;

public class RobotUserAuthorizationMagicOnionFilter : MagicOnionFilterAttribute
{
    /// <summary>
    /// Task 2: Missing Unit/Integration Tests
    /// </summary>
    public override async ValueTask Invoke(ServiceContext context, Func<ServiceContext, ValueTask> next)
    {
        var userIdClaim = context.CallContext.GetHttpContext().User.FindFirst("userId")?.Value;
        if (userIdClaim == null)
        {
            throw new ReturnStatusException(StatusCode.PermissionDenied, "Invalid userId");
        }

        await next(context);
    }
}
