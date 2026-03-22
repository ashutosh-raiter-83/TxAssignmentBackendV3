using AutoFixture;
using FluentAssertions;
using Grpc.Core;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using Server.Util;
using Xunit;

namespace IntegrationTest.Hub;

public class RobotHub_ReportCommandResultTest : WebAppFactoryFixture
{
    public RobotHub_ReportCommandResultTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenCommandNotExists_ShouldReceiveNotFound()
    {
        var (client, _) = await Factory.CreateMagicOnionClient("Robot1");

        var commandResult = new Fixture().Create<MoveToZPositionCommandResult>();

        var exception = await Assert.ThrowsAnyAsync<RpcException>(() =>
            client.ReportCommandResult(commandResult)
        );
        exception.StatusCode.Should().Be(StatusCode.NotFound);
    }

    [Fact]
    public async Task WhenCommandExists_ShouldSucceed()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        var sentCommand = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<MoveToZPositionCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommand);

        var (client, _) = await Factory.CreateMagicOnionClient(robotId);

        var commandResult = new Fixture()
            .Build<MoveToZPositionCommandResult>()
            .With(x => x.CommandId, sentCommand.CommandId)
            .Create();
        await client.ReportCommandResult(commandResult);

        var persisted = await Factory.GetReceivedCommandResultRepository()
            .Fetch(robotId, sentCommand.CommandId);
        persisted.CommandId.Should().Be(sentCommand.CommandId);
        persisted.RobotId.Should().Be(robotId);
        persisted.CommandResult.Should().BeEquivalentTo(commandResult);
        persisted.ReceivedAt.Should().BeOnOrAfter(sentCommand.SentAt);
    }

    /// <summary>
    /// Task 1: Bug Fixes
    /// Fix added in RobotHub.cs -> ReportCommandResult method to hanle case of Already exists.
    /// code has written to checkif result was already reported before creating new one
    ///In receive command repository, There is Fetch method can be used to check an existing result
    ///with that we can return Already Exists Status code.
    /// </summary>
    [Fact]
    public async Task WhenAlreadyReported_ShouldReceiveAlreadyExists()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        var sentCommand = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<MoveToZPositionCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommand);

        var (client, _) = await Factory.CreateMagicOnionClient(robotId);

        var commandResult = new Fixture()
            .Build<MoveToZPositionCommandResult>()
            .With(x => x.CommandId, sentCommand.CommandId)
            .Create();
        await client.ReportCommandResult(commandResult);

        var exception = await Assert.ThrowsAnyAsync<RpcException>(() =>
            client.ReportCommandResult(commandResult)
        );
        exception.StatusCode.Should().Be(StatusCode.AlreadyExists);
    }
    ///<summary>
    /// //Task 2 : Missing Unit/Integration Tests
    /// Adding Test casefpr Report COmmand Result Types
    /// </summary>
    [Fact]
    public async Task WhenReportePlacedItemCommandResult_ShouldBeSuccess()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        var sentCommand = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<PlaceItemCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommand);

        var (client, _) = await Factory.CreateMagicOnionClient(robotId);

        var commandResult = new Fixture()
            .Build<PlaceItemCommandResult>()
            .With(x => x.CommandId, sentCommand.CommandId)
            .Create();
        await client.ReportCommandResult(commandResult);

        var dataPersisted = await Factory.GetReceivedCommandResultRepository().Fetch(robotId, sentCommand.CommandId);

        dataPersisted.Should().NotBeNull();
        dataPersisted?.CommandId.Should().Be(sentCommand.CommandId);
        dataPersisted?.RobotId.Should().Be(robotId);
    }
    ///<summary>
    /// //Task 2 : Missing Unit/Integration Tests
    /// Adding Test casefpr Report COmmand Result Types
    /// </summary>
    [Fact]
    public async Task WhenReporteScanEnvironmentCommandResult_ShouldBeSuccess()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        var sentCommand = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<ScanEnvironmentCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommand);

        var (client, _) = await Factory.CreateMagicOnionClient(robotId);

        var commandResult = new ScanEnvironmentCommandResult()
        {
            CommandId=sentCommand.CommandId,
            Success =true,
            FailureReason = null,
            Robot = new RobotShared.Model.Robot()
            {
                ZPosition =0,
                FacingDirection =RobotShared.Model.FacingDirection.Neutral,
                IsHoldingItem =false,
                FlagReason=null,
                FlaggedLabeledBins = [],
                FlaggedUnsortedBins = [],
                ServerCausedFlagCount= 0,
                CorrectSortCount =0
            },
            UnsortedBinZPositions = [0.5f, 1.5f],
            LabeledBinZPositions = [0.5f, 1.5f]
        };
        await client.ReportCommandResult(commandResult);

        var dataPersisted = await Factory.GetReceivedCommandResultRepository().Fetch(robotId, sentCommand.CommandId);

        dataPersisted.Should().NotBeNull();
        dataPersisted?.CommandId.Should().Be(sentCommand.CommandId);
        dataPersisted?.RobotId.Should().Be(robotId);
    }
}
