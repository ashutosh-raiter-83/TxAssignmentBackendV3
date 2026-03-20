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
    }
}
