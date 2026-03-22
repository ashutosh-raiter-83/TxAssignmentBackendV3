using Xunit;
using Moq;
using RobotShared.Model;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using Server.AutoPilot;
using FluentAssertions;
using System.Net;
using AutoFixture;
using System.Net.Http.Json;
using RobotShared.Model.Http.TechnicianUser;


namespace IntegrationTest.Controller.TechnicianUser
{
    public class AutoPilotController_ActivateDeActivateTest : WebAppFactoryFixture
    {
        public AutoPilotController_ActivateDeActivateTest(WebAppFactory factory) : base(factory)
        {
        }


        /// <summary>
        /// Task 3 :Feature Development - AutoPilotController Testing : Activation and DeActivation
        /// </summary>
        /// //****************************************** Activation Test cases*********************************************
        [Fact]
        public async Task Activate_WhenNotAutheticated_ShouldReceiveUnAuthorze()
        {
            var clnt= Factory.CreateClient();
            var resp = await clnt.PostAsync("technician-user/robot/Robot1/autopilot/activate",null);
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        [Fact]
        public async Task Activate_WhenRobotNotExist_ShouldReceiveNotFound()
        {
            var clnt = Factory.CreateTechnicianClient();
            var robotId = new Fixture().Create<string>();

            var resp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Activate_WhenRobotExistwithOffline_ShouldReceiveUnProceableEntity()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOffline(robotId);

            var clnt = Factory.CreateTechnicianClient();

            var resp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        }
        [Fact]
        public async Task Activate_WhenRobotOnline_ShouldActivateReturnsRunning()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOnline(robotId);

            var clnt = Factory.CreateTechnicianClient();
            var resp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStateResponse>();
            body.Should().NotBeNull();
            body!.State.Should().Be(AutoPilotState.Running.ToString());

            ////CLeaning up

            Factory.GetAutoPilotManager().DeActivateAutoPilot(robotId);
        }

        //****************************************** DeActivation Test cases*********************************************

        [Fact]
        public async Task DeAcitvate_WhenNotAutheticated_ShouldReceiveUnAuthorze()
        {
            var clnt = Factory.CreateClient();
            var resp = await clnt.PostAsync($"technician-user/robot/Robot1/autopilot/deactivate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task DeActivate_WhenRobotNotExist_ShouldReceiveNotFound()
        {
            var clnt = Factory.CreateTechnicianClient();
            var robotId = new Fixture().Create<string>();

            var resp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/deactivate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeActivate_WhenAutoPilotActive_ShouldDeActivate()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOnline(robotId);

            //Activate first
            var clnt = Factory.CreateTechnicianClient();
            await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);

            //No DeActivate
            var resp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/deactivate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStateResponse>();
            body.Should().NotBeNull();
            body!.State.Should().Be(AutoPilotState.Deactivated.ToString());
        }
        [Fact]
        public async Task DeActivate_WhenAlreadyADetactive_ShouldRetunDeActivate()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);

            var clnt = Factory.CreateTechnicianClient();
            var resp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/deactivate", null);
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStateResponse>();
            body!.State.Should().Be(AutoPilotState.Deactivated.ToString());
        }
    }
}
