using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has scanned the unsorted bin.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ScanUnsortedBinCommandResult : CommandResultBase
{

    /// <summary>
    /// The robot may only scan the topmost item of the unsorted bin.
    /// The robot may only pick the topmost item of the unsorted bin.
    ///
    /// If empty string, the unsorted bin is empty.
    /// </summary>
    public string TopmostItemLabel { get; set; }
}
