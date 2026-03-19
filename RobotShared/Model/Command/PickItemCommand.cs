using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// If the robot is,
/// - At an unsorted bin
/// - Facing the unsorted bin
/// - Not holding an item
/// 
/// It will pick the topmost item, if any.
/// 
/// Note, always scan before picking!
/// The robot cannot scan an item's label after picking it up.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class PickItemCommand : CommandBase
{
}
