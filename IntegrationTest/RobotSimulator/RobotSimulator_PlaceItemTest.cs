using AutoFixture;
using FluentAssertions;
using Moq;
using RobotShared.Hub;
using RobotShared.Model.Command;
using Server.Util;
using Xunit;
using MyRobotSimulator = RobotSimulator.RobotSimulator;

namespace IntegrationTest.RobotSimulator;

public class RobotSimulator_PlaceItemTest : WebAppFactoryFixture
{
    public RobotSimulator_PlaceItemTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenPlaceCorrectly_ShouldIncrementSortCount()
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
            Items = ["X"],
        }];
        simulator.Environment.LabeledBins = [new() {
            ZPosition = 0.0f,
            Label = "X",
            CurrentCount = 0,
            MaxCount = 2,
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

        simulator.Environment.Robot.CorrectSortCount.Should().Be(0);
        simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(0);
        simulator.Environment.Robot.FlagReason.Should().BeNull();
        await SendCommand(new MoveToZPositionCommand()
        {
            ZPosition = 0.0f,
        });
        await SendCommand(new FaceDirectionCommand()
        {
            FacingDirection = RobotShared.Model.FacingDirection.UnsortedBin,
        });
        await SendCommand(new PickItemCommand());
        await SendCommand(new FaceDirectionCommand()
        {
            FacingDirection = RobotShared.Model.FacingDirection.LabeledBin,
        });
        await SendCommand(new PlaceItemCommand());
        await simulator.WaitForCommandQueueEmpty();
        simulator.Environment.Robot.CorrectSortCount.Should().Be(1);
        simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(0);
        simulator.Environment.Robot.FlagReason.Should().BeNull();
        simulator.Environment.Robot.HeldItemLabel.Should().BeNull();
        simulator.Environment.LabeledBins[0].CurrentCount.Should().Be(1);
    }

    [Fact]
    public async Task WhenPlaceIncorrectly_ShouldNotIncrementSortCount()
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
            Items = ["X"],
        }];
        simulator.Environment.LabeledBins = [new() {
            ZPosition = 0.0f,
            Label = "Y",
            CurrentCount = 0,
            MaxCount = 2,
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

        simulator.Environment.Robot.CorrectSortCount.Should().Be(0);
        simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(0);
        simulator.Environment.Robot.FlagReason.Should().BeNull();
        await SendCommand(new MoveToZPositionCommand()
        {
            ZPosition = 0.0f,
        });
        await SendCommand(new FaceDirectionCommand()
        {
            FacingDirection = RobotShared.Model.FacingDirection.UnsortedBin,
        });
        await SendCommand(new PickItemCommand());
        await SendCommand(new FaceDirectionCommand()
        {
            FacingDirection = RobotShared.Model.FacingDirection.LabeledBin,
        });
        await SendCommand(new PlaceItemCommand());
        await simulator.WaitForCommandQueueEmpty();
        simulator.Environment.Robot.CorrectSortCount.Should().Be(0);
        simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(1);
        simulator.Environment.Robot.FlagReason.Should().Be(RobotShared.Model.RobotFlagReason.UnsafeEnvironment);
        simulator.Environment.Robot.HeldItemLabel.Should().BeNull();
        simulator.Environment.LabeledBins[0].CurrentCount.Should().Be(0);
    }

    [Fact]
    public async Task WhenPlaceToFullBin_ShouldNotIncrementSortCount()
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
            Items = ["X"],
        }];
        simulator.Environment.LabeledBins = [new() {
            ZPosition = 0.0f,
            Label = "X",
            CurrentCount = 2,
            MaxCount = 2,
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

        simulator.Environment.Robot.CorrectSortCount.Should().Be(0);
        simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(0);
        simulator.Environment.Robot.FlagReason.Should().BeNull();
        await SendCommand(new MoveToZPositionCommand()
        {
            ZPosition = 0.0f,
        });
        await SendCommand(new FaceDirectionCommand()
        {
            FacingDirection = RobotShared.Model.FacingDirection.UnsortedBin,
        });
        await SendCommand(new PickItemCommand());
        await SendCommand(new FaceDirectionCommand()
        {
            FacingDirection = RobotShared.Model.FacingDirection.LabeledBin,
        });
        await SendCommand(new PlaceItemCommand());
        await simulator.WaitForCommandQueueEmpty();
        simulator.Environment.Robot.CorrectSortCount.Should().Be(0);
        simulator.Environment.Robot.ServerCausedFlagCount.Should().Be(1);
        simulator.Environment.Robot.FlagReason.Should().Be(RobotShared.Model.RobotFlagReason.UnsafeEnvironment);
        simulator.Environment.Robot.HeldItemLabel.Should().BeNull();
        simulator.Environment.LabeledBins[0].CurrentCount.Should().Be(2);
    }

}
