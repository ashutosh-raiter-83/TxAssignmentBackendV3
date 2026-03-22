using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Command;
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
    public class CommandControllerWithAutoPilot_Test : WebAppFactoryFixture
    {
        public CommandControllerWithAutoPilot_Test(WebAppFactory factory) : base(factory)
        {

        }
        ///// <summary>
        ///// Task 3 :Feature Development - CommandController with  AutoPilot Testing 
        ///// </summary>
        //[Fact]
        //public async Task WhenAutoPilotRunning_CommandsByUserShouldBeRejected()
        //{
        //    var robotId = new Fixture().Create<string>();
        //    await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
        //    Factory.SetRobotOnline(robotId);

        //    var clnt = Factory.CreateTechnicianClient();
        //    var actResp = await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);
        //    actResp.StatusCode.Should().Be(HttpStatusCode.OK);

        //    // Since copilot is activateed and running...sending command
        //    var commandResp = await clnt.PostAsync(
        //    $"/technician-user/robot/{robotId}/command",
        //    JsonContent.Create(new SendCommandRequest()
        //    {
        //        Command = new ScanEnvironmentCommand()
        //    })
        //);
        //    commandResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        //}

        [Fact]
        public async Task WhenAutoDeActive_CommandsByUserShouldBeSucceesed()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOnline(robotId);

            var clnt = Factory.CreateTechnicianClient();
            await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);
            await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/deactivate", null);
            

            // Since auto pilot is activateed and running...sending command
            var commandResp = await clnt.PostAsync(
            $"/technician-user/robot/{robotId}/command",
            JsonContent.Create(new SendCommandRequest()
            {
                Command = new ScanEnvironmentCommand()
            })
        );
            commandResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task WhenAutoPilotPaused_CommandsByUserShouldBeSucceesed()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);
            Factory.SetRobotOnline(robotId);

            var clnt = Factory.CreateTechnicianClient();
            await clnt.PostAsync($"technician-user/robot/{robotId}/autopilot/activate", null);

            Factory.GetAutoPilotManager().OnFlagReported(robotId);

            var commandResp = await clnt.PostAsJsonAsync(
                $"/technician-user/robot/{robotId}/command",
                new SendCommandRequest { Command =new QueryRobotStateCommand() });
        
            commandResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
