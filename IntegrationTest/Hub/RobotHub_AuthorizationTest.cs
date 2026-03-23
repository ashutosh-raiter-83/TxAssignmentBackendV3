using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using RobotShared.Hub;
using Xunit;
using RobotShared.Model.Http;
using Grpc.Core;
using MagicOnion.Client;
using FluentAssertions;
using AutoFixture;
// Task2 : Missing Test Implementation for RobotHub Authorization
// Implement tests for RobotHub authorization, ensuring that only authenticated robots can connect and perform actions.
// Use the WebAppFactory to create a test client and simulate robot connections with valid and invalid tokens.
// Here we need to Verify that unauthorized access should be properly handled and authorized robots can successfully interact with the hub.
namespace IntegrationTest.Hub
{
    public class RobotHub_AuthorizationTest : WebAppFactoryFixture
    {
        public RobotHub_AuthorizationTest(WebAppFactory factory) : base(factory)
        {

        }
        [Fact]
        public async Task WhenUsrTypeTechnicianUser_PermissionShouldbeDeclined()
        {
            var token = Factory.GenerateToken(UserType.Technician, "Technician1");
            //var roboReceiver = new RoboReceiver(Factory.CreateClient(), token);
            var roboReceiver = new Mock<IRobotHubReceiver>();
            var errException = await Assert.ThrowsAnyAsync<RpcException>(() => StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
                    Factory.CreateGrpcChannel(), roboReceiver.Object,
                    option: new CallOptions(new Metadata()
                    {
                        { "Authorization", $"Bearer {token}" }
                    }))
                );

            //Onexception should be thrown with Permission Denied status code
            errException.StatusCode.Should().Be(StatusCode.PermissionDenied);
        }
        /// <summary>
        /// Task 2 : Missing Test Implementation for RobotHub Authorization 
        /// When 2 Robots areconnected, Both should have Permission SGranted, and connection should be successful
        ///</summary>
        [Fact]
        public async Task WhenUsrTypeRobotUser_PermissionShouldbeGrantedandShouldConnect()
        {
            var robotId = new Fixture().Create<string>();

            var token = Factory.GenerateToken(UserType.Robot, robotId);

            var roboReceiver = new Mock<IRobotHubReceiver>();

            var hubClient = await StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
                    Factory.CreateGrpcChannel(), roboReceiver.Object,
                    option: new CallOptions(new Metadata()
                    {
                        { "Authorization", $"Bearer {token}" }
                    }))
                ;
            //If connection is successful, hubClient should not be null
            hubClient.Should().NotBeNull();

            Factory.ConnectedRobotCollection.Verify(c => c.OnConnected(robotId, It.IsAny<IRobotHubReceiver>()), Times.Once);

            await hubClient.DisposeAsync();
        }
        /// <summary>
        /// Task 2 : Missing Test Implementation for RobotHub Authorization 
        /// When UserType os Robot, Permission Should be Granted, and connection should be successful
        ///</summary>
        [Fact]
        public async Task When2RobotUserConnected_BothShouldBeTracked()
        {
            var robotId1 = new Fixture().Create<string>();
            var robotId2 = new Fixture().Create<string>();

            var token1 = Factory.GenerateToken(UserType.Robot, robotId1);
            var token2 = Factory.GenerateToken(UserType.Robot, robotId2);

            var roboReceiverA = new Mock<IRobotHubReceiver>();
            var roboReceiverB = new Mock<IRobotHubReceiver>();


            var hubClientA = await StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
                    Factory.CreateGrpcChannel(), roboReceiverA.Object,
                    option: new CallOptions(new Metadata()
                    {
                        { "Authorization", $"Bearer {token1}" }
                    }))
                ;

            var hubClientB = await StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
                    Factory.CreateGrpcChannel(), roboReceiverB.Object,
                    option: new CallOptions(new Metadata()
                    {
                        { "Authorization", $"Bearer {token2}" }
                    }))
                ;
            Factory.ConnectedRobotCollection.Verify(c => c.OnConnected(robotId2, It.IsAny<IRobotHubReceiver>()), Times.Once);
            Factory.ConnectedRobotCollection.Verify(c => c.OnConnected(robotId2, It.IsAny<IRobotHubReceiver>()), Times.Once);

            await hubClientA.DisposeAsync();
            await hubClientB.DisposeAsync();

        }
        ///<summary>
        /// Task 2 : Missing Unit/Integration Test
        /// </summary>
        /// Adding this testcase when Invlidd Token given Should retrurn UnAutheticated
        [Fact]
        public async Task WhenInvalidBearerTokken_ShouldBebeUnAuthenticated()
        {
            var recver = new Mock<IRobotHubReceiver>();
            var excpetion = await Assert.ThrowsAnyAsync<RpcException>(() => StreamingHubClient
                .ConnectAsync<IRobotHub,IRobotHubReceiver>(
                Factory.CreateGrpcChannel(),
                recver.Object,
                    option: new CallOptions(new Metadata()
                    {
                        {"authorization",$"Bearer Invalid Token" },
                    })
                )
            );
            excpetion.StatusCode.Should().Be(StatusCode.Unauthenticated);
        }
    }
}
