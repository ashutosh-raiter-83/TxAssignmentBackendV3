using System.Text;
using Xunit;
using Moq;
using RobotShared.Hub;
using Server.Hub;
using FluentAssertions;
using AutoFixture;

namespace UnitTest.Hub
{
    /// <summary>
    /// Task 2 : Missing Test Implementation 
    /// 1. for ConnectionRobotOnEvents with multiple user
    /// </summary>
    public class ConnectionRobotOnEvents_WithMultiUserFuncTest
    {
        [Fact]
        public async Task WhenMultipleRobotsConnected_ShouldRetrieveIndepndently()
        {
            var robotCollector = new ConnectedRobotCollection();

            var userId1 = new Fixture().Create<string>();
            var userId2 = new Fixture().Create<string>();

            var reciever1 = new Mock<IRobotHubReceiver>();
            var reciever2 = new Mock<IRobotHubReceiver>();

            await robotCollector.OnConnected(userId1, reciever1.Object);
            await robotCollector.OnConnected(userId2, reciever2.Object);

            var dataFound1 = robotCollector.TryGetClient(userId1, out var client1);
            var dataFound2 = robotCollector.TryGetClient(userId2, out var client2);

            dataFound1.Should().BeTrue();
            client1.Should().BeSameAs(reciever1.Object);

            dataFound2.Should().BeTrue();
            client2.Should().BeSameAs(reciever2.Object);
        }
        /// <summary>
        /// Task 2 : Missing Test Implementation 
        /// 1. for ConnectionRobotOnEvents with multiple user
        /// </summary>
        [Fact]
        public async Task WhenMultipleRobotsDisconnected_ShouldNotRetrieveResults()
        {
            var robotCollector = new ConnectedRobotCollection();

            var userId1 = new Fixture().Create<string>();
            var userId2 = new Fixture().Create<string>();

            var reciever1 = new Mock<IRobotHubReceiver>();
            var reciever2 = new Mock<IRobotHubReceiver>();

            await robotCollector.OnConnected(userId1, reciever1.Object);
            await robotCollector.OnConnected(userId2, reciever2.Object);

            await robotCollector.OnDisconnected(userId1);
            await robotCollector.OnDisconnected(userId2);

            var dataNotFound1 = robotCollector.TryGetClient(userId1, out var client1);
            var dataNotFound2 = robotCollector.TryGetClient(userId2, out var client2);

            dataNotFound1.Should().BeFalse();
            client1.Should().BeNull();

            dataNotFound2.Should().BeFalse();
            client2.Should().BeNull();
        }
    }
}
