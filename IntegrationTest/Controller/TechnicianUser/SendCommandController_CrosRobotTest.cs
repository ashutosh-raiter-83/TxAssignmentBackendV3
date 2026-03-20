using RobotShared.Model.Command;
using Server.Util;
using Xunit;
using System.Net;
using AutoFixture;
using System.Net.Http.Json;
using RobotShared.Model.Http.TechnicianUser;
using FluentAssertions;

namespace IntegrationTest.Controller.TechnicianUser
{
    /// <summary>
    /// Task : Missing Unit/Integration Tests
    /// </summary>
    public class SendCommandController_CrosRobotTest : WebAppFactoryFixture
    {
        public SendCommandController_CrosRobotTest(WebAppFactory factory) : base(factory)
        {
            
        }
        /// Command belong to a different robot, should return NotFound
        [Fact]
        public async Task WhenCommandBelongsToDifferentRobot_ShouldReturnNotFound()
        {
            var robotId1 = new Fixture().Create<string>();
            var usernameA = new Fixture().Create<string>();
            var passwordA = new Fixture().Create<string>();

            var robotId2 = new Fixture().Create<string>();
            var usernameB = new Fixture().Create<string>();
            var passwordB = new Fixture().Create<string>();


            await Factory.GetRobotRepository().Create(robotId1, usernameA, passwordA);
            await Factory.GetRobotRepository().Create(robotId2, usernameB, passwordB);

            var sentCommand = new Fixture().Build<SentCommand>()
                .With(a => a.RobotId, robotId1)
                .With(a => a.Command, new Fixture().Create<MoveToZPositionCommand>())
                .With(a => a.SentAt, DateTimeUtil.UtcNowMs)
                .Create();

            await Factory.GetSentCommandRepository().Create(sentCommand);

            var client = Factory.CreateTechnicianClient();
            var response = await client.GetAsync(
                $"/technician-user/robot/{robotId2}/command/{sentCommand.Command}/sent");


            response?.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
