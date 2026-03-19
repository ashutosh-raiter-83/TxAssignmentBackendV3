namespace Server.Repository.Model;

public class Robot
{
    public string RobotId { get; set; }

    /// <summary>
    /// If null, the robot has never logged in.
    /// </summary>
    public DateTime? LastLogInAt { get; set; }
}
