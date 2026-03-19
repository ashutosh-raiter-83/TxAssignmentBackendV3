namespace RobotShared.Model;

public enum FacingDirection
{
    /// <summary>
    /// Robot is facing toward neither bin, straight forward.
    /// </summary>
    Neutral,

    /// <summary>
    /// Robot is facing toward unsorted bins; left of the robot.
    /// </summary>
    UnsortedBin,

    /// <summary>
    /// Robot is facing toward labeled bins; right of the robot.
    /// </summary>
    LabeledBin,
}
