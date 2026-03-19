using MessagePack;

namespace RobotShared.Model.Command;

[MessagePackObject(keyAsPropertyName: true)]
public class FaceDirectionCommand : CommandBase
{
    public FacingDirection FacingDirection { get; set; }
}
