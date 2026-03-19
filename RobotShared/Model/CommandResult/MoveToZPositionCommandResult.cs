using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has moved the the specified ZPosition.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class MoveToZPositionCommandResult : CommandResultBase
{
}
