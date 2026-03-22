using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Http.TechnicianUser;
using Server.AutoPilot;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Xunit;

namespace IntegrationTest.Controller.TechnicianUser
{
    public class AutoPilotController_State_Statistics_Tests : WebAppFactoryFixture
    {
        public AutoPilotController_State_Statistics_Tests(WebAppFactory factory) : base(factory)
        {

        }
        /// <summary>
        /// Task 3 :Feature Development - AutoPilotController Testing : State Statistics Test
        /// </summary>
        /// //****************************************** State Test cases*********************************************
        [Fact]
        public async Task State_WhenNotAutheticated_ShouldReceiveUnAuthorze()
        {
            var clnt = Factory.CreateClient();
            var resp = await clnt.GetAsync("technician-user/robot/Robot1/autopilot/state");
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task State_WhenRobotNotExist_ShouldReceiveNotFound()
        {
            var clnt = Factory.CreateTechnicianClient();
            var robotId = new Fixture().Create<string>();

            var resp = await clnt.GetAsync($"technician-user/robot/{robotId}/autopilot/state");
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        [Fact]
        public async Task State_WhenNoAutoPilot_ShouldReturnDeActivate()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            
            var clnt = Factory.CreateTechnicianClient();
            var resp = await clnt.GetAsync($"technician-user/robot/{robotId}/autopilot/state");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStateResponse>();
            body!.State.Should().Be(AutoPilotState.Deactivated.ToString());

           
        }
        [Fact]
        public async Task State_WhenActoPiliotRunning_ShouldReturnsRunning()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOnline(robotId);

            var clnt = Factory.CreateTechnicianClient();
            await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);


            var resp = await clnt.GetAsync($"technician-user/robot/{robotId}/autopilot/state");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStateResponse>();
            body!.State.Should().Be(AutoPilotState.Running.ToString());

            ////CLeaning up

            Factory.GetAutoPilotManager().DeActivateAutoPilot(robotId);
        }
        /// //****************************************** Statistics Test cases*********************************************

        [Fact]
        public async Task Statistics_WhenNotAutheticated_ShouldReceiveUnAuthorze()
        {
            var clnt = Factory.CreateClient();
            var resp = await clnt.GetAsync("technician-user/robot/Robot1/autopilot/statistics");
            resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Statistics_WhenRobotNotExist_ShouldReceiveNotFound()
        {
            var clnt = Factory.CreateTechnicianClient();
            var robotId = new Fixture().Create<string>();

            var resp = await clnt.GetAsync($"technician-user/robot/{robotId}/autopilot/statistics");
            resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        [Fact]
        public async Task Statistics_WhenNoAutoPilot_ShouldReturnZeroStatistics()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);

            var clnt = Factory.CreateTechnicianClient();
            var resp = await clnt.GetAsync($"technician-user/robot/{robotId}/autopilot/statistics");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStatisticResponse>();
            body.Should().NotBeNull();
            body!.State.Should().Be(AutoPilotState.Deactivated.ToString());
            body.ItemsSorted.Should().Be(0);
            body.CommandsSent.Should().Be(0);
            body.ErrorsEncountered.Should().Be(0);
        }
        [Fact]
        public async Task Statistics_WhenActoPiliotActive_ShouldReturnsStatisistics()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOnline(robotId);

            var clnt = Factory.CreateTechnicianClient();
            await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);


            var resp = await clnt.GetAsync($"technician-user/robot/{robotId}/autopilot/statistics");
            resp.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await resp.Content.ReadFromJsonAsync<AutoPilotStatisticResponse>();
            body.Should().NotBeNull();
            body!.State.Should().Be(AutoPilotState.Running.ToString());

            ////CLeaning up

            Factory.GetAutoPilotManager().DeActivateAutoPilot(robotId);
        }
    }
}
