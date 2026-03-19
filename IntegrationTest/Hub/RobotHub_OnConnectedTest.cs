using FluentAssertions;
using Grpc.Core;
using MagicOnion.Client;
using Moq;
using RobotShared.Hub;
using RobotShared.Model.Http;
using Xunit;

namespace IntegrationTest.Hub;

public class RobotHub_OnConnectedTest : WebAppFactoryFixture
{
    public RobotHub_OnConnectedTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenNoToken_ShouldReceiveUnauthenticated()
    {
        var receiver = new Mock<IRobotHubReceiver>();
        var exception = await Assert.ThrowsAnyAsync<RpcException>(() => StreamingHubClient
            .ConnectAsync<IRobotHub, IRobotHubReceiver>(
                Factory.CreateGrpcChannel(),
                receiver.Object
            )
        );
        exception.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task WhenExpiredToken_ShouldReceiveUnauthenticated()
    {
        var token = Factory.GenerateToken(UserType.Robot, "Robot1", DateTime.UtcNow.AddSeconds(5));
        await Task.Delay(TimeSpan.FromSeconds(20));
        var receiver = new Mock<IRobotHubReceiver>();
        var exception = await Assert.ThrowsAnyAsync<RpcException>(() => StreamingHubClient
            .ConnectAsync<IRobotHub, IRobotHubReceiver>(
                Factory.CreateGrpcChannel(),
                receiver.Object,
                option: new CallOptions(new Metadata()
                {
                    { "authorization", $"Bearer {token}" },
                })
            )
        );
        exception.StatusCode.Should().Be(StatusCode.Unauthenticated);
    }

    [Fact]
    public async Task WhenUserTypeRobot_ShouldConnect()
    {
        var token = Factory.GenerateToken(UserType.Robot, "Robot1");
        var receiver = new Mock<IRobotHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
            Factory.CreateGrpcChannel(),
            receiver.Object,
            option: new CallOptions(new Metadata()
            {
                { "authorization", $"Bearer {token}" },
            })
        );

        Factory.ConnectedRobotCollection.Verify(
            x => x.OnConnected("Robot1", It.IsAny<IRobotHubReceiver>()),
            Times.Once
        );
    }
}
