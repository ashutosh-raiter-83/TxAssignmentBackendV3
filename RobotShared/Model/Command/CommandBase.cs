using MessagePack;
using System.Text.Json.Serialization;

namespace RobotShared.Model.Command;

/// <summary>
/// In general, commands are not safe to run if the robot has flagged itself or any bins.
/// 
/// The exception is QueryRobotStateCommand; it is safe to run, even if the robot has flagged itself or any bins.
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
[Union(0, typeof(ClearFlagsCommand))]
[Union(1, typeof(FaceDirectionCommand))]
[Union(2, typeof(MoveToZPositionCommand))]
[Union(3, typeof(PickItemCommand))]
[Union(4, typeof(PlaceItemCommand))]
[Union(5, typeof(QueryRobotStateCommand))]
[Union(6, typeof(ScanEnvironmentCommand))]
[Union(7, typeof(ScanLabeledBinCommand))]
[Union(8, typeof(ScanUnsortedBinCommand))]
[JsonDerivedType(typeof(ClearFlagsCommand), "ClearFlagsCommand")]
[JsonDerivedType(typeof(FaceDirectionCommand), "FaceDirectionCommand")]
[JsonDerivedType(typeof(MoveToZPositionCommand), "MoveToZPositionCommand")]
[JsonDerivedType(typeof(PickItemCommand), "PickItemCommand")]
[JsonDerivedType(typeof(PlaceItemCommand), "PlaceItemCommand")]
[JsonDerivedType(typeof(QueryRobotStateCommand), "QueryRobotStateCommand")]
[JsonDerivedType(typeof(ScanEnvironmentCommand), "ScanEnvironmentCommand")]
[JsonDerivedType(typeof(ScanLabeledBinCommand), "ScanLabeledBinCommand")]
[JsonDerivedType(typeof(ScanUnsortedBinCommand), "ScanUnsortedBinCommand")]
public abstract class CommandBase
{
}
