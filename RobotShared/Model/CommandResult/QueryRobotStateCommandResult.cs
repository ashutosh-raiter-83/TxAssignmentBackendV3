using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has returned information about itself.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class QueryRobotStateCommandResult : CommandResultBase
{
    public Robot Robot { get; set; }
}
