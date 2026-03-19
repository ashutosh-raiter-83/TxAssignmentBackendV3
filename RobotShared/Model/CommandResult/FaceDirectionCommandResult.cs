using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has faced the specified FacingDirection.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class FaceDirectionCommandResult : CommandResultBase
{
}
