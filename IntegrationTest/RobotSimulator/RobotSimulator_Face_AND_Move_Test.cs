using AutoFixture;
using FluentAssertions;
using Moq;
using RobotShared.Hub;
using RobotShared.Model;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using RobotSimulator;
using Server.AutoPilot;
using Server.Util;
using System.ComponentModel.Design;
using Xunit;
using MyRobotSimulator = RobotSimulator.RobotSimulator;

namespace IntegrationTest.RobotSimulator
{
    public class RobotSimulator_Face_AND_Move_Test : WebAppFactoryFixture
    {
        /// <summary>
        /// Task 3 :Feature Development - Adding newTest cases for RobotSimulator Move and Face Testing
        /// </summary>
        public RobotSimulator_Face_AND_Move_Test(WebAppFactory factory) : base(factory)
        {

        }
        [Fact]
        public async Task WhenMovedToCorrectPosition_ShouldUpdateZPosition()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler Recvercommand = null;
            var events = new Mock<IRobotHubReceiverEvents>();
            events.SetupAdd(p => p.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) =>
                {
                    Recvercommand = handler;
                });

            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance: 0.0f);

            simulator.Environment.UnsortedBins = [new()
            {
                ZPosition = 1.0f,
                Items=["X"],
            }];
            simulator.Environment.LabeledBins = [new() {
                ZPosition = 1.0f,
                Label= "X",
                CurrentCount=0,
                MaxCount=5,
            }];
            async Task<SentCommand> SendCommand(CommandBase command)
            {
                var sentCommand = new SentCommand()
                {
                    CommandId = new Fixture().Create<string>(),
                    RobotId = robotId,
                    Command = command,
                    SentAt = DateTimeUtil.UtcNowMs,
                };
                await Factory.GetSentCommandRepository().Create(sentCommand);

                Recvercommand(sentCommand);
                return sentCommand;

            }

            simulator.Environment.Robot.ZPosition.Should().Be(0.0f);
            await SendCommand(new MoveToZPositionCommand() { ZPosition = 1.0f, });
            await simulator.WaitForCommandQueueEmpty();

            simulator.Environment.Robot.ZPosition.Should().Be(1.0f);
            simulator.Environment.Robot.FlagReason.Should().BeNull();
        }


        [Fact]
        public async Task WhenFaceDirection_ShouldUpdateFaceDirection()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler Recvercommand = null;
            var events = new Mock<IRobotHubReceiverEvents>();
            events.SetupAdd(p => p.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) =>
                {
                    Recvercommand = handler;
                });

            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance: 0.0f);

            simulator.Environment.UnsortedBins = [new()
            {
                ZPosition = 1.0f,
                Items=["X"],
            }];
            simulator.Environment.LabeledBins = [new() {
                ZPosition = 1.0f,
                Label= "X",
                CurrentCount=0,
                MaxCount=5,
            }];
            async Task<SentCommand> SendCommand(CommandBase command)
            {
                var sentCommand = new SentCommand()
                {
                    CommandId = new Fixture().Create<string>(),
                    RobotId = robotId,
                    Command = command,
                    SentAt = DateTimeUtil.UtcNowMs,
                };
                await Factory.GetSentCommandRepository().Create(sentCommand);

                Recvercommand(sentCommand);
                return sentCommand;

            }

            simulator.Environment.Robot.FacingDirection.Should().Be(FacingDirection.Neutral);
            await SendCommand(new FaceDirectionCommand() { FacingDirection = FacingDirection.UnsortedBin, });
            await simulator.WaitForCommandQueueEmpty();
            simulator.Environment.Robot.FacingDirection.Should().Be(FacingDirection.UnsortedBin);

            await SendCommand(new FaceDirectionCommand() { FacingDirection = FacingDirection.LabeledBin, });
            await simulator.WaitForCommandQueueEmpty();
            simulator.Environment.Robot.FacingDirection.Should().Be(FacingDirection.LabeledBin);

        }
        [Fact]
        public async Task WhenCommandWhileFlagged_ShouldBeFailed()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler Recvercommand = null;
            var events = new Mock<IRobotHubReceiverEvents>();
            events.SetupAdd(p => p.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) =>
                {
                    Recvercommand = handler;
                });

            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance: 0.0f);

            simulator.Environment.UnsortedBins = [new()
            {
                ZPosition = 1.0f,
                Items=["X"],
            }];
            simulator.Environment.LabeledBins = [new() {
                ZPosition = 1.0f,
                Label= "X",
                CurrentCount=0,
                MaxCount=5,
            }];
            async Task<SentCommand> SendCommand(CommandBase command)
            {
                var sentCommand = new SentCommand()
                {
                    CommandId = new Fixture().Create<string>(),
                    RobotId = robotId,
                    Command = command,
                    SentAt = DateTimeUtil.UtcNowMs,
                };
                await Factory.GetSentCommandRepository().Create(sentCommand);

                Recvercommand(sentCommand);
                return sentCommand;

            }
            //Move robot to negative position
            await SendCommand(new MoveToZPositionCommand() { ZPosition =-1.0f, });
            await simulator.WaitForCommandQueueEmpty();
            simulator.Environment.Robot.FlagReason.Should().NotBeNull();
            // Move command should fail without chaning posution
            var beforePosition = simulator.Environment.Robot.ZPosition;
            await SendCommand(new MoveToZPositionCommand() { ZPosition = 0.5f, });
            await simulator.WaitForCommandQueueEmpty();
            simulator.Environment.Robot.ZPosition.Should().Be(beforePosition);
            
            //Face direction should alsofail

            await SendCommand(new FaceDirectionCommand() { FacingDirection = FacingDirection.UnsortedBin, });
            await simulator.WaitForCommandQueueEmpty();
            simulator.Environment.Robot.FacingDirection.Should().Be(FacingDirection.Neutral);
        }

        [Fact]
        public async Task WhenMoveNegativePosition_ShouldFlagToRobot()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler Recvercommand = null;
            var events = new Mock<IRobotHubReceiverEvents>();
            events.SetupAdd(p => p.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) =>
                {
                    Recvercommand = handler;
                });

            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance: 0.0f);

            simulator.Environment.UnsortedBins = [new()
            {
                ZPosition = 1.0f,
                Items=["X"],
            }];
            simulator.Environment.LabeledBins = [new() {
                ZPosition = 1.0f,
                Label= "X",
                CurrentCount=0,
                MaxCount=5,
            }];
            async Task<SentCommand> SendCommand(CommandBase command)
            {
                var sentCommand = new SentCommand()
                {
                    CommandId = new Fixture().Create<string>(),
                    RobotId = robotId,
                    Command = command,
                    SentAt = DateTimeUtil.UtcNowMs,
                };
                await Factory.GetSentCommandRepository().Create(sentCommand);

                Recvercommand(sentCommand);
                return sentCommand;

            }
            await SendCommand(new MoveToZPositionCommand() { ZPosition = -1.0f, });
            await simulator.WaitForCommandQueueEmpty();

            simulator.Environment.Robot.ZPosition.Should().Be(0.0f);
            simulator.Environment.Robot.FlagReason.Should().Be(RobotFlagReason.HardwareFault);
            simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(1);


           
        }
    }
}
