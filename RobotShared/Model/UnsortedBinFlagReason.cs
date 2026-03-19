namespace RobotShared.Model;

public enum UnsortedBinFlagReason
{
    /// <summary>
    /// The bin is empty.
    /// It must be replaced by a new non-empty unsorted bin.
    /// </summary>
    BinEmpty,

    /// <summary>
    /// The topmost item has an invalid label.
    /// The topmost item must be removed by the technician.
    /// </summary>
    TopmostItemHasNoMatchingLabeledBin,
}
