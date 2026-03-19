using System.ComponentModel.DataAnnotations;

namespace RobotShared.Model.Http;

public class LogInAsRobotRequest
{
    [StringLength(256, MinimumLength = 1)]
    public string Username { get; set; }

    [StringLength(256, MinimumLength = 1)]
    public string Password { get; set; }
}
