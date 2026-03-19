using RobotShared.Model.Command;

namespace RobotShared.Model.Http.TechnicianUser;

public class ListSentCommandResponse
{
    public List<SentCommand> SentCommands { get; set; }
}
