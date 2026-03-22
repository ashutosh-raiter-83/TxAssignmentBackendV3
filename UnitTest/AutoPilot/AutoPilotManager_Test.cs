using Xunit;
using Moq;
using RobotShared.Model;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using Server.AutoPilot;
using FluentAssertions;
using Server.Hub;
using Server.Repository;
using Microsoft.Extensions.DependencyInjection;
using RobotShared.Hub;
namespace UnitTest.AutoPilot
{
    public class AutoPilotManager_Test
    {
        /// <summary>
        /// Task 3 :Feature Development - Adding newTest cases for Auto Pilot Manager Testing
        /// </summary>
        public static (AutoPilotManager autopilotMgr, Mock<IConnectedRobotCollection> connectedRobots,
            Mock<ISentCommandRepository> commandRepo) CreateManager()
        {
            var connectedRobots = new Mock<IConnectedRobotCollection>();
            var commandRepo = new Mock<ISentCommandRepository>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ISentCommandRepository>(_ => commandRepo.Object);
            var svcProvider = serviceCollection.BuildServiceProvider();
            var scopeFactory = svcProvider.GetRequiredService<IServiceScopeFactory>();

            var manager = new AutoPilotManager(connectedRobots.Object, scopeFactory);

            return (manager, connectedRobots, commandRepo);
        }

        [Fact]
        public void WhenNoSessoin_StateShouldBeDeActive()
        {
            var (manager, _, _) = CreateManager();
            manager.GetState("robot1").Should().Be(AutoPilotState.Deactivated);
        }

        [Fact]
        public void WhenNoSessoin_IsRunningShouldReturnFalse()
        {
            var (manager, _, _) = CreateManager();
            manager.IsRunning("robot1").Should().BeFalse();
        }

        [Fact]
        public void WhenNoSessoin_GetSessionShouldReturnNull()
        {
            var (manager, _, _) = CreateManager();
            manager.GetSession("robot1").Should().BeNull();
        }
        [Fact]
        public async Task WhenActivateOfflineRobot_ShouldRespnseFalsse()
        {
            var (manager, connectedRobots, _) = CreateManager();
            IRobotHubReceiver? clnt = null;

            connectedRobots.Setup(a => a.TryGetClient("robot1", out clnt)).Returns(false);

            var resp = await manager.ActivateAutoPilot("robot1");
            resp.Should().BeFalse();

            manager.GetState("robot1").Should().Be(AutoPilotState.Deactivated);

        }

        [Fact]
        public async Task WhenActivateOnlineRobot_ShouldBeRunning()
        {
            var (manager, connectedRobots, commandRepo) = CreateManager();
            var clntMock = new Mock<IRobotHubReceiver>();
            IRobotHubReceiver? outClnt = clntMock.Object;
            
            connectedRobots
                .Setup(a => a.TryGetClient("robot1", out outClnt))
                .Returns(true);

            var resp = await manager.ActivateAutoPilot("robot1");
            resp.Should().BeTrue();

            manager.GetState("robot1").Should().Be(AutoPilotState.Running);
            manager.IsRunning("robot1").Should().BeTrue();
        }

        [Fact]
        public async Task WhenActivate_ShouldSendCommandFirstly()
        {
            var (manager, connectedRobots, commandRepo) = CreateManager();
            var clntMock = new Mock<IRobotHubReceiver>();
            IRobotHubReceiver? outClnt = clntMock.Object;
            connectedRobots
                .Setup(a => a.TryGetClient("robot1", out outClnt))
                .Returns(true);

            await manager.ActivateAutoPilot("robot1");

            commandRepo.Verify(a => a.Create(It.IsAny<RobotShared.Model.Command.SentCommand>()), Times.Once);
            clntMock.Verify(o => o.OnCommandReceived(It.IsAny<RobotShared.Model.Command.SentCommand>()), Times.Once);
        }

        [Fact]
        public async Task WhenDeactivated_ShouldBeDeactivated()
        {
            var (manager, connectedRobots, _) = CreateManager();
            var clntMock = new Mock<IRobotHubReceiver>();
            IRobotHubReceiver? outClnt = clntMock.Object;
            connectedRobots
                .Setup(a => a.TryGetClient("robot1", out outClnt))
                .Returns(true);

            await manager.ActivateAutoPilot("robot1");
            manager.DeActivateAutoPilot("robot1");

            manager.GetState("robot1").Should().Be(AutoPilotState.Deactivated);
            manager.IsRunning("robot1").Should().BeFalse();
        }
        /// <summary>
        /// Scenario with MultipleRobots SesionShould be Isolate
        /// </summary>
        [Fact]
        public async Task WhenMultiRobots_SessionShouldSetIsolate()
        {
            var (manager, connectedRobots, _) = CreateManager();
            
            var clntMockA = new Mock<IRobotHubReceiver>();
            var clntMockB = new Mock<IRobotHubReceiver>();
            IRobotHubReceiver? outClntA = clntMockA.Object;
            IRobotHubReceiver? outClntB = clntMockB.Object;

            connectedRobots
                .Setup(a => a.TryGetClient("robot1", out outClntA))
                .Returns(true);
            connectedRobots
                .Setup(a => a.TryGetClient("robot2", out outClntB))
                .Returns(true);

            await manager.ActivateAutoPilot("robot1");
            await manager.ActivateAutoPilot("robot2");

            manager.OnFlagReported("robot1");

            manager.GetState("robot1").Should().Be(AutoPilotState.Paused);
            manager.GetState("robot2").Should().Be(AutoPilotState.Running);
        }

        [Fact]
        public async Task WhenFlagReported_ShouldPauseRunningSession()
        {
            var (manager, connectedRobots, _) = CreateManager();
            var clntMock = new Mock<IRobotHubReceiver>();
            IRobotHubReceiver? outClnt = clntMock.Object;
            
            connectedRobots
                .Setup(a => a.TryGetClient("robot1", out outClnt))
                .Returns(true);

            await manager.ActivateAutoPilot("robot1");
            manager.OnFlagReported("robot1");

            manager.GetState("robot1").Should().Be(AutoPilotState.Paused);
            manager.IsRunning("robot1").Should().BeFalse();
        }
        [Fact]
        public async Task WhenDeactivateNonExistsOne_ShouldNotThrow()
        {
            var (manager, _, _) = CreateManager();
            var deact = () => manager.DeActivateAutoPilot("robot1");
            deact.Should().NotThrow();
        }
        [Fact]
        public async Task WhenFlagReportedWitoutSeession_ShouldNotThrow()
        {
            var (manager, _, _) = CreateManager();
            var deact = () => manager.OnFlagReported("robot1");
            deact.Should().NotThrow();
        }

        [Fact]
        public async Task WhenFlagClearAfterPause_ShouldResumeRunning()
        {
            var (manager, connectedRobots, _) = CreateManager();
            var clntMock = new Mock<IRobotHubReceiver>();
            IRobotHubReceiver? outClnt = clntMock.Object;
            connectedRobots
                .Setup(a => a.TryGetClient("robot1", out outClnt))
                .Returns(true);

            await manager.ActivateAutoPilot("robot1");
            manager.OnFlagReported("robot1");
            await manager.OnFlagsCleared("robot1");

            manager.GetState("robot1").Should().Be(AutoPilotState.Running);
        }
        [Fact]
        public async Task WhenCommandResultRecdWithoutSession_ShouldNotThrow()
        {
            var (manager, _, _) = CreateManager();
            var ct = async () => await manager.OnCOmmandResultRecieved(
                "robot1",new ScanEnvironmentCommandResult
                {
                    CommandId="cmdA",Success = true, FailureReason =null, Robot = new RobotShared.Model.Robot(),
                    UnsortedBinZPositions = [],
                    LabeledBinZPositions = [],
                }
                );
            await ct.Should().NotThrowAsync();
        }
    }
}
