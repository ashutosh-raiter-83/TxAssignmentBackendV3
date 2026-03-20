using RobotShared.Hub;
using System.Collections.Concurrent;

namespace Server.Hub;

public interface IConnectedRobotCollection
{
    Task OnConnected(string userId, IRobotHubReceiver client);
    Task OnDisconnected(string userId);

    bool TryGetClient(string userId, out IRobotHubReceiver? client);
}

public class ConnectedRobotCollection : IConnectedRobotCollection
{
    //<summary>
    /// Task 1: Bug Fixes
    // Dictionary is not thread safe, instead of it I have added ConcurrentDictionary 
    // With Race condition, multiple threads can access and modify the dictionary at the same
    // time, which can lead to unpredictable issues like IndexOutOfRange or KeyNotfound may cause datastructure corrupted.
    //</summary>
    private readonly ConcurrentDictionary<string, IRobotHubReceiver> _userId2Client = new();

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
        // Instead of Remove, Added TryRemove to avoid potential issues when multiple threads
        // try to remove the same userId at the same time
        _userId2Client.TryRemove(userId, out _);
        return Task.CompletedTask;
    }
}
