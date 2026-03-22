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
    public class RobotSimulator_PickItems_Test : WebAppFactoryFixture
    {
        /// <summary>
        /// Task 2 : Missing Unit/Integration Tests
        /// </summary>
        public RobotSimulator_PickItems_Test(WebAppFactory factory) : base(factory)
        {

        }
        [Fact]
        public async Task WhenItemPickedfromUnSortedBin_ShouldHoldThatItem()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler sendCommand = null;
            var events = new Mock<IRobotHubReceiverEvents>();
            events
                .SetupAdd(x => x.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) => {
                    sendCommand = handler;
                });

            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance: 0.0f
            );
            simulator.Environment.UnsortedBins = [new() {
            ZPosition = 0.0f,
            Items = ["X","Y"],
        }];
            simulator.Environment.LabeledBins = [new() {
            ZPosition = 0.0f,
            Label = "X",
            CurrentCount = 0,
            MaxCount = 5,
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
                sendCommand(sentCommand);
                return sentCommand;
            }

            simulator.Environment.Robot.IsHoldingItem.Should().BeFalse();

            
            await SendCommand(new MoveToZPositionCommand()
            {
                ZPosition = 0.0f,
            });
            await SendCommand(new FaceDirectionCommand()
            {
                FacingDirection = RobotShared.Model.FacingDirection.UnsortedBin,
            });

            await SendCommand(new PickItemCommand());

            
            await simulator.WaitForCommandQueueEmpty();

            simulator.Environment.Robot.IsHoldingItem.Should().BeTrue();
            simulator.Environment.Robot.HeldItemLabel.Should().Be("X");
            simulator.Environment.Robot.FlagReason.Should().BeNull();
        }

        [Fact]
        public async Task WhenItemPickedAlreadyonHold_ShouldStateNotTobeChanged()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler sendCommand = null;
            var events = new Mock<IRobotHubReceiverEvents>();
            events
                .SetupAdd(x => x.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) => {
                    sendCommand = handler;
                });

            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance: 0.0f
            );
            simulator.Environment.UnsortedBins = [new() {
            ZPosition = 0.0f,
            Items = ["X","Y"],
        }];
            simulator.Environment.LabeledBins = [new() {
            ZPosition = 0.0f,
            Label = "X",
            CurrentCount = 0,
            MaxCount = 5,
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
                sendCommand(sentCommand);
                return sentCommand;
            }

            simulator.Environment.Robot.IsHoldingItem.Should().BeFalse();


            await SendCommand(new MoveToZPositionCommand()
            {
                ZPosition = 0.0f,
            });
            await SendCommand(new FaceDirectionCommand()
            {
                FacingDirection = RobotShared.Model.FacingDirection.UnsortedBin,
            });

            await SendCommand(new PickItemCommand());
            await simulator.WaitForCommandQueueEmpty();


            simulator.Environment.Robot.HeldItemLabel.Should().Be("X");
            simulator.Environment.Robot.FlagReason.Should().BeNull();
        }
    }
}
