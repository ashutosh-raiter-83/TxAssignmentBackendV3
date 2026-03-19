using MessagePack;

namespace RobotShared.Model.Command;

[MessagePackObject(keyAsPropertyName: true)]
public class MoveToZPositionCommand : CommandBase
{
    public float ZPosition { get; set; }
}
