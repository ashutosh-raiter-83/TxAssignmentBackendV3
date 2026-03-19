using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// If the robot is at an unsorted bin and facing the unsorted bin,
/// it will scan and reveal the label of the topmost item, if any.
/// 
/// Note: The robot cannot scan unsorted bins if it is holding an item.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ScanUnsortedBinCommand : CommandBase
{
}
