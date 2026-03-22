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
using Server.Util;
using RobotShared.Model.CommandResult;
namespace IntegrationTest.Hub
{
    public class RobotHub_AutoPilot_Test : WebAppFactoryFixture
    {
        /// <summary>
        /// Task 3 :Feature Development - Robot Hub with  AutoPilot Testing 
        /// </summary>
        public RobotHub_AutoPilot_Test(WebAppFactory factory) : base(factory)
        {

        }
        [Fact]
        public async Task WhenReportedFlag_AutoPilotShouldbePaused()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);

            var (clnt,_) = await Factory.CreateMagicOnionClient(robotId);
            Factory.SetRobotOnline(robotId);

            var autopilot = Factory.GetAutoPilotManager();
            await autopilot.ActivateAutoPilot(robotId);
            autopilot.GetState(robotId).Should().Be(AutoPilotState.Running);

            //Here now reporting Flag
            await clnt.ReportFlag(new RobotShared.Model.Robot
            {
                FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault,
                FlaggedLabeledBins = [],
                FlaggedUnsortedBins = [],
            });
            autopilot.GetState(robotId).Should().Be(AutoPilotState.Paused);
            autopilot.DeActivateAutoPilot(robotId);
        }

        [Fact]
        public async Task WhenAutoPilotNotActive_FlaShouldNotInitiateSession()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);

            var (clnt, _) = await Factory.CreateMagicOnionClient(robotId);
            
            var autopilot = Factory.GetAutoPilotManager();
            autopilot.GetState(robotId).Should().Be(AutoPilotState.Deactivated);

            //Here now reporting Flag without autopilot active
            await clnt.ReportFlag(new RobotShared.Model.Robot
            {
                FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault,
                FlaggedLabeledBins = [],
                FlaggedUnsortedBins = [],
            });
            autopilot.GetState(robotId).Should().Be(AutoPilotState.Deactivated);
            
        }
        [Fact]
        public async Task WhenClearFlagSucces_AutoPilotShouldbeResume()
        {
            var robotId = new Fixture().Create<string>();
            await Factory.GetRobotRepository().Create(robotId, robotId, robotId);

            var (clnt, _) = await Factory.CreateMagicOnionClient(robotId);
            Factory.SetRobotOnline(robotId);

            var autopilot = Factory.GetAutoPilotManager();
            await autopilot.ActivateAutoPilot(robotId);

            autopilot.OnFlagReported(robotId);
            autopilot.GetState(robotId).Should().Be(AutoPilotState.Paused);

            var comandId = new Fixture().Create<string>();

            await Factory.GetSentCommandRepository().Create(new SentCommand
            {
                CommandId = comandId,
                RobotId=robotId,
                Command = new ClearFlagsCommand(),
                SentAt = DateTimeUtil.UtcNowMs,
            });

            await clnt.ReportCommandResult(new ClearFlagsCommandResult
            {
                CommandId = comandId,
                Success = true,
                FailureReason = null,
            });


            autopilot.GetState(robotId).Should().Be(AutoPilotState.Running);

            autopilot.DeActivateAutoPilot(robotId);
        }
    }
}
