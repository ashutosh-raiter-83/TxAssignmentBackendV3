using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, all flags on robot and bins have been cleared.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ClearFlagsCommandResult : CommandResultBase
{
}
