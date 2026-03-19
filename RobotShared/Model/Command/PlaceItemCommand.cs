using MessagePack;

namespace RobotShared.Model.Command;

/// <summary>
/// If the robot is,
/// - At an labeled bin
/// - Facing the labeled bin
/// - Holding an item
/// 
/// It will place the item into the bin.
/// 
/// Note, always scan before placing!
/// If the bin is full and the robot attempts to place an item there,
/// it will cause the item to fall to the ground and trigger an UnsafeEnvironment flag!
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class PlaceItemCommand : CommandBase
{
}
