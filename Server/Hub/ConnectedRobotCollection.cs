using RobotShared.Hub;

namespace Server.Hub;

public interface IConnectedRobotCollection
{
    Task OnConnected(string userId, IRobotHubReceiver client);
    Task OnDisconnected(string userId);

    bool TryGetClient(string userId, out IRobotHubReceiver? client);
}

public class ConnectedRobotCollection : IConnectedRobotCollection
{
    private readonly Dictionary<string, IRobotHubReceiver> _userId2Client = new();

    public bool TryGetClient(string userId, out IRobotHubReceiver? client)
    {
        return _userId2Client.TryGetValue(userId, out client);
    }

    public Task OnConnected(string userId, IRobotHubReceiver client)
    {
        _userId2Client[userId] = client;
        return Task.CompletedTask;
    }

    public Task OnDisconnected(string userId)
    {
        _userId2Client.Remove(userId, out _);
        return Task.CompletedTask;
    }
}
