using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has picked an item from the unsorted bin.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class PickItemCommandResult : CommandResultBase
{
}
