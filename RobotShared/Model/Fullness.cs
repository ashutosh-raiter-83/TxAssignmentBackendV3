namespace RobotShared.Model;

/// <summary>
/// How full the labeled bin is.
/// </summary>
public enum Fullness
{
    /// <summary>
    /// The labeled bin is empty and can accept more items.
    /// </summary>
    Empty,

    /// <summary>
    /// The labeled bin is partially filled and can accept more items.
    /// </summary>
    PartiallyFilled,

    /// <summary>
    /// The labeled bin is completely filled and cannot accept more items.
    /// The labeled bin must be flagged so it is switched out for a new labeled bin.
    /// </summary>
    CompletelyFilled,
}
