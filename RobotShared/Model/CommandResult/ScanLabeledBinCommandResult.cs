using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has scanned the labeled bin.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ScanLabeledBinCommandResult : CommandResultBase
{
    /// <summary>
    /// The label on the bin.
    /// The robot must place items in the correct labeled bins.
    /// The label on the item must match the label on the bin.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// How full the labeled bin is.
    ///
    /// If null, the robot has not scanned the bin
    /// and does not know how full the bin is.
    /// </summary>
    public Fullness Fullness { get; set; }
}
