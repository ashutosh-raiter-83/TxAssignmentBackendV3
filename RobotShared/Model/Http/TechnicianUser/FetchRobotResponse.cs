namespace RobotShared.Model.Http.TechnicianUser;

public class FetchRobotResponse
{
    public string RobotId { get; set; }

    /// <summary>
    /// If null, the robot has never logged in.
    /// </summary>
    public DateTime? LastLogInAt { get; set; }

    /// <summary>
    /// If true, the robot is currently online and can receive commands.
    /// </summary>
    public bool IsOnline { get; set; }
}
