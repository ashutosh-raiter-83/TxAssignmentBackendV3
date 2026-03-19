using RobotShared.Model.Command;

namespace RobotShared.Model.Http.TechnicianUser;

public class SendCommandRequest
{
    public CommandBase Command { get; set; }
}
