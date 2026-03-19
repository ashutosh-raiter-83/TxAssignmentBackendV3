using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// Commands the robot to scan its environment and return
/// brief information about its surroundings and itself.
/// 
/// Note: The robot cannot scan its environment if it is holding an item.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ScanEnvironmentCommand : CommandBase
{
}
