using RobotShared.Model.CommandResult;

namespace RobotShared.Model.Http.TechnicianUser;

public class ListReceivedCommandResultResponse
{
    public List<ReceivedCommandResult> ReceivedCommandResults { get; set; }
}
