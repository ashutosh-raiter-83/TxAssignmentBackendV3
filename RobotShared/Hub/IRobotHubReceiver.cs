using RobotShared.Model.Command;

namespace RobotShared.Hub;

public interface IRobotHubReceiver
{
    /// <summary>
    /// Pings the client
    /// </summary>
    Task<string> Ping(CancellationToken cancellationToken = default);

    Task OnCommandReceived(SentCommand sentCommand);
}
