using RobotShared.Model.Command;

namespace RobotShared.Hub;

public interface IRobotHubReceiverEvents
{
    public delegate void OnCommandReceivedHandler(SentCommand sentCommand);
    public event OnCommandReceivedHandler? OnCommandReceivedEvent;
}
