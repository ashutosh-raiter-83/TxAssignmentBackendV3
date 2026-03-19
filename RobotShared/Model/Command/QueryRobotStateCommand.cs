using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// Commands the robot to return information about itself.
/// This is faster than ScanEnvironmentCommand because it skips bin scanning.
///
/// This command is safe to run, even if the robot has flagged itself or any bins.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class QueryRobotStateCommand : CommandBase
{
}
