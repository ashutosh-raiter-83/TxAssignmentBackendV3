using MessagePack;

namespace RobotShared.Model;

/// <summary>
/// Labeled bins are placed on the robot's right side.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class LabeledBin
{
    /// <summary>
    /// The position of the bin along the Z-axis.
    /// Unit is in metres.
    ///
    /// The robot moves along a rail to reach a given labeled bin.
    /// - When ZPosition = 0.0, the bin is at the start of the rail.
    /// - When ZPosition = 0.5, the bin is 0.5m away from the start of the rail.
    /// - When ZPosition = 1.0, the bin is 1m away from the start of the rail.
    ///
    /// Assumed no two labeled bins share the same ZPosition.
    /// </summary>
    public float ZPosition { get; set; }

    /// <summary>
    /// The label on the bin.
    /// The robot must place items in the correct labeled bins.
    /// The label on the item must match the label on the bin.
    ///
    /// If null, the robot has not scanned the bin
    /// and does not know what label it has.
    /// </summary>
    public string? Label { get; set; }

    /// <summary>
    /// How full the labeled bin is.
    ///
    /// If null, the robot has not scanned the bin
    /// and does not know how full the bin is.
    ///
    /// This may contain outdated information.
    /// The only way to know the current fullness is to scan the bin.
    /// </summary>
    public Fullness? Fullness { get; set; }
}
