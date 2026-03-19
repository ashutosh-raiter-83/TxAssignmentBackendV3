using MessagePack;

namespace RobotShared.Model;

/// <summary>
/// Bins of unsorted items are placed on the robot's left side.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class UnsortedBin
{
    /// <summary>
    /// The position of the bin along the Z-axis.
    /// Unit is in metres.
    ///
    /// The robot moves along a rail to reach a given unsorted bin.
    /// - When ZPosition = 0.0, the bin is at the start of the rail.
    /// - When ZPosition = 0.5, the bin is 0.5m away from the start of the rail.
    /// - When ZPosition = 1.0, the bin is 1m away from the start of the rail.
    /// 
    /// Assumed no two unsorted bins share the same ZPosition.
    /// </summary>
    public float ZPosition { get; set; }

    /// <summary>
    /// The robot may only scan the topmost item of the unsorted bin.
    /// The robot may only pick the topmost item of the unsorted bin.
    /// 
    /// If null, the robot has not scanned the item
    /// and does not know what label it has.
    /// 
    /// If empty string, the unsorted bin is empty.
    /// </summary>
    public string? TopmostItemLabel { get; set; }
}
