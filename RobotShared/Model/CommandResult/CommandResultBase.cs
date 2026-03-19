using MessagePack;
using System.Text.Json.Serialization;

namespace RobotShared.Model.CommandResult;

[MessagePackObject(keyAsPropertyName: true)]
[Union(0, typeof(ClearFlagsCommandResult))]
[Union(1, typeof(FaceDirectionCommandResult))]
[Union(2, typeof(MoveToZPositionCommandResult))]
[Union(3, typeof(PickItemCommandResult))]
[Union(4, typeof(PlaceItemCommandResult))]
[Union(5, typeof(QueryRobotStateCommandResult))]
[Union(6, typeof(ScanEnvironmentCommandResult))]
[Union(7, typeof(ScanLabeledBinCommandResult))]
[Union(8, typeof(ScanUnsortedBinCommandResult))]
[JsonDerivedType(typeof(ClearFlagsCommandResult), "ClearFlagsCommandResult")]
[JsonDerivedType(typeof(FaceDirectionCommandResult), "FaceDirectionCommandResult")]
[JsonDerivedType(typeof(MoveToZPositionCommandResult), "MoveToZPositionCommandResult")]
[JsonDerivedType(typeof(PickItemCommandResult), "PickItemCommandResult")]
[JsonDerivedType(typeof(PlaceItemCommandResult), "PlaceItemCommandResult")]
[JsonDerivedType(typeof(QueryRobotStateCommandResult), "QueryRobotStateCommandResult")]
[JsonDerivedType(typeof(ScanEnvironmentCommandResult), "ScanEnvironmentCommandResult")]
[JsonDerivedType(typeof(ScanLabeledBinCommandResult), "ScanLabeledBinCommandResult")]
[JsonDerivedType(typeof(ScanUnsortedBinCommandResult), "ScanUnsortedBinCommandResult")]
public abstract class CommandResultBase
{
    public string CommandId { get; set; }

    /// <summary>
    /// - If true, the robot executed the command.
    /// - If false, the robot did not execute the command.
    ///   - Check the FailureReason for the reason.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Contains a human-readable string for the failure.
    /// </summary>
    public string? FailureReason { get; set; }
}
