namespace RobotShared.Model.Http;

public class LogInAsRobotResponse
{
    /// <summary>
    /// To be passed in the Authorization header with the Bearer scheme
    /// </summary>
    public string Token { get; set; }
}
