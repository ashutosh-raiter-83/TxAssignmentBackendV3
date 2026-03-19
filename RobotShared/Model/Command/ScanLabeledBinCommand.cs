using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// If the robot is at a labeled bin and facing the labeled bin,
/// it will scan and reveal the label and fullness.
/// 
/// Note: The robot cannot scan labeled bins if it is holding an item.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ScanLabeledBinCommand : CommandBase
{
}
