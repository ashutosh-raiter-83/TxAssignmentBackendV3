namespace RobotShared.Model;

public enum RobotFlagReason
{
    /// <summary>
    /// The robot has detected a change in the environment
    /// that has made it unsafe for it to continue operating.
    ///
    /// Technician must intervene and make the environment safe again.
    ///
    /// Maybe someone walked into the path of the robot.
    /// Maybe a bin fell.
    /// </summary>
    UnsafeEnvironment,

    /// <summary>
    /// The robot has detected a hardware fault.
    ///
    /// Technician must intervene and fix the hardware fault.
    ///
    /// Maybe a joint got stuck.
    /// Maybe a sensor malfunctioned.
    /// </summary>
    HardwareFault,

    /// <summary>
    /// The robot has detected a software fault.
    /// Not much can be done here except to reboot the robot.
    /// </summary>
    SoftwareFault,
}
