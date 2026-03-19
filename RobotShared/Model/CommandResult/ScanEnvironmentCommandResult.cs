using MessagePack;

namespace RobotShared.Model.CommandResult;

/// <summary>
/// On success, the robot has scanned its environment.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public class ScanEnvironmentCommandResult : CommandResultBase
{
    public Robot Robot { get; set; }
    public List<float> UnsortedBinZPositions { get; set; }
    public List<float> LabeledBinZPositions { get; set; }
}
