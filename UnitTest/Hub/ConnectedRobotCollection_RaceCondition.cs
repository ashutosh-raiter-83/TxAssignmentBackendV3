using AutoFixture;
using Moq;
using RobotShared.Hub;
using Server.Hub;
using Xunit;

namespace UnitTest.Hub;

public class ConnectedRobotCollection_RaceCondition
{
    /// <summary>
    /// Task 1: Bug Fixes
    /// Fix added in ConnectedRobotCollection.cs 
    /// Dictionary is not thread safe, instead of it I have added ConcurrentDictionary 
    ///With Race condition, multiple threads can access and modify the dictionary at the same
    ///time, which can lead to unpredictable issues like IndexOutOfRange or KeyNotfound may cause datastructure corrupted.
    ///Instead of Remove, Added TryRemove to avoid potential issues when multiple thread try to remove the same userId at the same time
    /// </summary>
    [Fact]
    public async Task WhenConcurrentAccess_ShouldNotThrow()
    {
        var collection = new ConnectedRobotCollection();
        var userId = new Fixture().Create<string>();
        var receiver = new Mock<IRobotHubReceiver>();

        var tasks = new List<Task>();
        for (var i=0; i<1000; ++i)
        {
            tasks.Add(Task.Run(async () =>
            {
                await collection.OnConnected(userId, receiver.Object);
            }));
            tasks.Add(Task.Run(async () =>
            {
                await collection.OnDisconnected(userId);
            }));
            tasks.Add(Task.Run(async () =>
            {
                collection.TryGetClient(userId, out var _);
            }));
        }

        await Task.WhenAll(tasks);
    }
}
