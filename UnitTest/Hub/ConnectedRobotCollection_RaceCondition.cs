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
