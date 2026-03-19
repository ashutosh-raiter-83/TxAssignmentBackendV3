using FluentAssertions;
using MagicOnion.Client;
using Moq;
using RobotShared.Hub;
using Xunit;

namespace IntegrationTest.Hub;

public class RobotHub_OnDisconnectedTest : WebAppFactoryFixture
{
    public RobotHub_OnDisconnectedTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenDisconnect_ShouldSetRobotOffline()
    {
        var (client, _) = await Factory.CreateMagicOnionClient("Robot1");

        Factory.ConnectedRobotCollection.Verify(
            x => x.OnConnected("Robot1", It.IsAny<IRobotHubReceiver>()),
            Times.Once
        );

        Factory.ConnectedRobotCollection.Verify(
            x => x.OnDisconnected("Robot1"),
            Times.Never
        );

        await client.DisposeAsync();
        var reason = await client.WaitForDisconnectAsync();
        reason.Type.Should().Be(DisconnectionType.CompletedNormally);

        await Task.Delay(5000);

        Factory.ConnectedRobotCollection.Verify(
            x => x.OnConnected("Robot1", It.IsAny<IRobotHubReceiver>()),
            Times.Once
        );

        Factory.ConnectedRobotCollection.Verify(
            x => x.OnDisconnected("Robot1"),
            Times.Once
        );
    }
}
