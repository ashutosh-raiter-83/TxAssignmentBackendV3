using MessagePack;

namespace RobotShared.Model.CommandResult;

[MessagePackObject(keyAsPropertyName: true)]
public class ReceivedCommandResult
{
    public string CommandId { get; set; }
    public string RobotId { get; set; }

    public CommandResultBase CommandResult { get; set; }

    public DateTime ReceivedAt { get; set; }
}
