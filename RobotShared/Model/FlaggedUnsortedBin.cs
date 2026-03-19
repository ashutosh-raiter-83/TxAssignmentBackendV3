using MessagePack;

namespace RobotShared.Model;

[MessagePackObject(keyAsPropertyName: true)]
public class FlaggedUnsortedBin
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
    /// Contains the reason the unsorted bin has been flagged.
    /// </summary>
    public UnsortedBinFlagReason FlagReason { get; set; }
}
