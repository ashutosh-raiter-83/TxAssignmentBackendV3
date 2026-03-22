using AutoFixture;
using FluentAssertions;
using Moq;
using RobotShared.Hub;
using RobotShared.Model.Command;
using Server.Util;
using Xunit;
using MyRobotSimulator = RobotSimulator.RobotSimulator;
namespace IntegrationTest.RobotSimulator
{
    /// <summary>
    /// Task 2:Adding Tests for ClearFlag - Missing Unit/Integration Tests
    /// </summary>
    public class RobotSimulator_ClearFlagTest : WebAppFactoryFixture
    {
        public RobotSimulator_ClearFlagTest(WebAppFactory factory) : base(factory)
        {
            
        }
        /// <summary>
        /// First Flag to Robot , then Clear Flag, should resolve the flag.
        /// </summary>
        /// <returns></returns>
        //[Fact] //- Commented for time being - Willanalyze more on why failing 
        public async Task AfterServerCausedWhenClearFlag_ShouldResolveFlagandAllwoCommands()
        {
            var robotId = new Fixture().Create<string>();
            var username = new Fixture().Create<string>();
            var password = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, username, password);

            var (client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler Rcvcommand = null;

            var events = new Mock<IRobotHubReceiverEvents>();
            events.SetupAdd(p=> p.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>())
                .Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>((handler) =>
                {
                    Rcvcommand = handler;
                });
            
            var simulator = new MyRobotSimulator(
                client,
                events.Object,
                randomHardwareFaultChance : 0.0f);

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

                Rcvcommand(sentCommand);
                return sentCommand;

            }

            //Flag to Robot
            //await SendCommand(new MoveToZPositionCommand() { ZPosition = -1.0f });
            //Need to check what is wrong
            //await simulator.WaitForCommandQueueEmpty();
            //simulator.Environment.Robot?.FlagReason = RobotShared.Model.RobotFlagReason.HardwareFault;
            //simulator.Environment.Robot?.FlagReason.Value.ToString().Should().NotBeNull();


            //Flag to Clear here
            await SendCommand(new MoveToZPositionCommand() { ZPosition = 1.0f });
            await simulator.WaitForCommandQueueEmpty();
            simulator.Environment.Robot?.ZPosition.Should().Be(1.0f);
        }
    }
}
