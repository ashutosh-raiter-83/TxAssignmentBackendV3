using System;
using System.Collections.Generic;
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
    /// Task 2 : Missing Test Implementation for ConnectionRobotOnEvents
    /// </summary>
    // New class added for teting connectionRonot event like OnConnection, Disconnnection and TryGetClient for ConnectedRobotCollection
    public class ConnectionRobotOnEvents_FuncTest
    {
        //Happy positive scenario test for ConnectionRobotOnEvents Functionality
        // OnConnected should add client and TryGetClient should get same client for the userId.
        [Fact]
        public async Task WhenRobotIsConnected_ShouldRetrieveResults()
        {
            var robotCollector = new ConnectedRobotCollection();
            var userId = new Fixture().Create<string>();
            var reciever = new Mock<IRobotHubReceiver>();

            await robotCollector.OnConnected(userId, reciever.Object);
            var dataFound = robotCollector.TryGetClient(userId, out var client);

            dataFound.Should().BeTrue();
            client.Should().BeSameAs(reciever.Object);
        }
        // OnDisConnected should add client , disconnect and on TryGetClient should not get client for the userId.
        [Fact]
        public async Task WhenRobotIsDisconnected_ShouldNotRetrieveResults()
        {
            var robotCollector = new ConnectedRobotCollection();
            var userId = new Fixture().Create<string>();
            var reciever = new Mock<IRobotHubReceiver>();

            await robotCollector.OnConnected(userId, reciever.Object);
            await robotCollector.OnDisconnected(userId);

            var dataNotFound = robotCollector.TryGetClient(userId, out var client);
            dataNotFound.Should().BeFalse();
            client.Should().BeNull();
        }
    }
}
