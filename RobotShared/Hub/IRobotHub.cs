using MagicOnion;
using RobotShared.Model;
using RobotShared.Model.CommandResult;

namespace RobotShared.Hub;

public interface IRobotHub : IStreamingHub<IRobotHub, IRobotHubReceiver>
{
    Task<string> GetHelloWorld();

    Task ReportCommandResult(CommandResultBase commandResult);

    /// <summary>
    /// When invoked, the following may be flagged,
    /// - Robot
    ///   - Technician must intervene and fix the issue
    /// - Unsorted bin
    ///   - Technician must intervene and fix the issue, or replace the bin
    /// - Labeled bin
    ///   - Technician must intervene and fix the issue, or replace the bin
    ///
    /// After performing the necessary actions, the technician may clear all flags.
    /// </summary>
    Task ReportFlag(Robot robot);
}
