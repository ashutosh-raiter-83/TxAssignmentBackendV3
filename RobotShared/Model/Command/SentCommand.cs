using MessagePack;

namespace RobotShared.Model.Command;

[MessagePackObject(keyAsPropertyName: true)]
public class SentCommand
{
    public string CommandId { get; set; }
    public string RobotId { get; set; }

    public CommandBase Command { get; set; }

    public DateTime SentAt { get; set; }
}
