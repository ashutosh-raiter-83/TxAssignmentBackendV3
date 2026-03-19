using MessagePack;

namespace RobotShared.Model;

[MessagePackObject(keyAsPropertyName: true)]
public class Robot
{
    /// <summary>
    /// The position of the robot along the Z-axis.
    /// Unit is in metres.
    ///
    /// The robot moves along a rail to reach a given unsorted bin.
    /// - When ZPosition = 0.0, the robot is at the start of the rail.
    /// - When ZPosition = 0.5, the robot is 0.5m away from the start of the rail.
    /// - When ZPosition = 1.0, the robot is 1m away from the start of the rail.
    /// </summary>
    public float ZPosition { get; set; }

    /// <summary>
    /// Which direction the robot is facing.
    /// </summary>
    public FacingDirection FacingDirection { get; set; }

    /// <summary>
    /// If true, the robot is holding an item and can place it in a labeled bin.
    /// If false, the robot is not holding an item and can pick one up from an unsorted bin.
    /// </summary>
    public bool IsHoldingItem { get; set; }

    /// <summary>
    /// If null, the robot has not been flagged.
    /// Otherwise, it contains the reason the robot has been flagged.
    /// </summary>
    public RobotFlagReason? FlagReason { get; set; }

    /// <summary>
    /// Contains all flagged unsorted bins.
    /// If empty, no unsorted bins have been flagged.
    /// </summary>
    public List<FlaggedUnsortedBin> FlaggedUnsortedBins { get; set; }

    /// <summary>
    /// Contains all flagged labeled bins.
    /// If empty, no labeled bins have been flagged.
    /// </summary>
    public List<FlaggedLabeledBin> FlaggedLabeledBins { get; set; }

    /// <summary>
    /// How many times the robot was flagged due to a fault in Server code.
    /// </summary>
    public int ServerCausedFlagCount { get; set; }

    /// <summary>
    /// How many times the robot placed an item into the correct bin.
    /// </summary>
    public int CorrectSortCount { get; set; }
}
