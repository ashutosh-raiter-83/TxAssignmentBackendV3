using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has placed an item to the labeled bin.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class PlaceItemCommandResult : CommandResultBase
{
}
