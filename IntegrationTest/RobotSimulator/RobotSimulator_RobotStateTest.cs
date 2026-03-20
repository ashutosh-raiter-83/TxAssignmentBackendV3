using Xunit;
using RobotShared.Model.Command;
using Server.Util;
using RobotShared.Hub;
using AutoFixture;
using Moq;
using MyRoboSimulator = RobotSimulator.RobotSimulator;
namespace IntegrationTest.RobotSimulator
{
    /// <summary>
    /// Task 2: MIssing Unit/Integration Tests
    /// </summary>
    public class RobotSimulator_RobotStateTest : WebAppFactoryFixture
    {
        public RobotSimulator_RobotStateTest(WebAppFactory factory): base(factory)
        {
                
        }
        [Fact]
        public async Task WhenQueryWithRobotState_ShouldReturnCurrentState()
        {
            var robotId = new Fixture().Create<string>();
            var usrName = new Fixture().Create<string>();
            var passwoed = new Fixture().Create<string>();

            await Factory.GetRobotRepository().Create(robotId, usrName, passwoed);

            var(client, _) = await Factory.CreateMagicOnionClient(robotId);

            IRobotHubReceiverEvents.OnCommandReceivedHandler sendCommand = null;

            var events = new Mock<IRobotHubReceiverEvents>();

            events.SetupAdd(e=> e.OnCommandReceivedEvent += It.IsAny<IRobotHubReceiverEvents.OnCommandReceivedHandler>()).
                Callback<IRobotHubReceiverEvents.OnCommandReceivedHandler>(handler =>
            {
                sendCommand = handler;
            });

            var simulator = new MyRoboSimulator(
                client,events.Object, randomHardwareFaultChance: 0.0f
                );

            simulator.Environment.UnsortedBins = [new() {
                ZPosition = 0.0f,Items =["X"],
            }];

            simulator.Environment.LabeledBins = [new() {
                ZPosition = 0.0f,Label ="X", CurrentCount=0,MaxCount=5,
            }];
            

            async Task<SentCommand> SendCommand(CommandBase command)
            {
                var sentCommand = new SentCommand()
                {
                    CommandId = new Fixture().Create<string>(),
                    Command = command,
                    RobotId = robotId,
                    SentAt =DateTimeUtil.UtcNowMs,

                };
                await Factory.GetSentCommandRepository().Create(sentCommand);
                sendCommand(sentCommand);
                return sentCommand;
            }
        }
    }
}
