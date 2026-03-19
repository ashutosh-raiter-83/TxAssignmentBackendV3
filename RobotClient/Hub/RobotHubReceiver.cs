using RobotShared.Hub;
using RobotShared.Model.Command;
using static RobotShared.Hub.IRobotHubReceiverEvents;

namespace RobotClient.Hub;

internal class RobotHubReceiver : IRobotHubReceiver, IRobotHubReceiverEvents
{
    public Task<string> Ping(CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Received Ping");
        return Task.FromResult("Pong");
    }

    public event OnCommandReceivedHandler? OnCommandReceivedEvent;
    public Task OnCommandReceived(SentCommand sentCommand)
    {
        Console.WriteLine($"Received Command={sentCommand.Command.GetType()}");
        if (OnCommandReceivedEvent != null)
        {
            OnCommandReceivedEvent(sentCommand);
        }
        return Task.CompletedTask;
    }

}
