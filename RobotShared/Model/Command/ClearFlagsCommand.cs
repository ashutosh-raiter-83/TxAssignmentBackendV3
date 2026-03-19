using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// Commands the robot to clear all flags.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ClearFlagsCommand : CommandBase
{
}
