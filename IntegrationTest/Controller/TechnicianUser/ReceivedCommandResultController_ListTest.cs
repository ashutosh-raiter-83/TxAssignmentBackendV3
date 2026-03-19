using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using RobotShared.Model.Http.TechnicianUser;
using Server.Util;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTest.Controller.TechnicianUser;

public class ReceivedCommandResultController_ListTest : WebAppFactoryFixture
{
    public ReceivedCommandResultController_ListTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenUnauthenticated_ShouldReceiveUnauthorized()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/result/received"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUserTypeRobot_ShouldReceiveUnauthorized()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateRobotClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/result/received"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenRobotNotExists_ShouldReceiveNotFound()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/result/received"
        );
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenRobotExists_ShouldSucceed()
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

        var receivedCommandResult = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotId)
            .With(x => x.CommandId, sentCommand.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<MoveToZPositionCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResult);

        var sentCommandOther = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<MoveToZPositionCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommandOther);

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/result/received"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListReceivedCommandResultResponse>();
        responseBody.Should().NotBeNull();
        responseBody.ReceivedCommandResults.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(receivedCommandResult);
    }

    [Fact]
    public async Task WhenMultipleReceivedCommandResults_ShouldSortByMostRecentSent()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotId, username, password);

        var sentCommandA = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<MoveToZPositionCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs.AddMilliseconds(100))
            .Create();

        var sentCommandB = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotId)
            .With(x => x.Command, new Fixture().Create<ScanEnvironmentCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommandA);
        await Factory.GetSentCommandRepository().Create(sentCommandB);

        var receivedCommandResultA = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotId)
            .With(x => x.CommandId, sentCommandA.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<MoveToZPositionCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs)
            .Create();

        var receivedCommandResultB = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotId)
            .With(x => x.CommandId, sentCommandB.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<ScanEnvironmentCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs.AddMilliseconds(100))
            .Create();

        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResultA);
        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResultB);

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/result/received"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListReceivedCommandResultResponse>();
        responseBody.Should().NotBeNull();

        var listedA = responseBody.ReceivedCommandResults.Should()
            .Contain(item => item.CommandId == receivedCommandResultA.CommandId)
            .Which;

        var listedB = responseBody.ReceivedCommandResults.Should()
            .Contain(item => item.CommandId == receivedCommandResultB.CommandId)
            .Which;

        listedA.Should().BeEquivalentTo(receivedCommandResultA);
        listedB.Should().BeEquivalentTo(receivedCommandResultB);

        responseBody.ReceivedCommandResults.IndexOf(listedA)
            .Should().BeGreaterThan(responseBody.ReceivedCommandResults.IndexOf(listedB));
    }

    [Fact]
    public async Task WhenMultipleRobotAndReceivedCommandResults_ShouldReturnReceivedCommandResultsOfSpecifiedRobot()
    {
        var robotIdX = new Fixture().Create<string>();
        var usernameX = new Fixture().Create<string>();
        var passwordX = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotIdX, usernameX, passwordX);

        var robotIdY = new Fixture().Create<string>();
        var usernameY = new Fixture().Create<string>();
        var passwordY = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotIdY, usernameY, passwordY);

        var sentCommandXA = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotIdX)
            .With(x => x.Command, new Fixture().Create<MoveToZPositionCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        var sentCommandXB = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotIdX)
            .With(x => x.Command, new Fixture().Create<ScanEnvironmentCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs.AddMilliseconds(100))
            .Create();

        var sentCommandYA = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotIdY)
            .With(x => x.Command, new Fixture().Create<PickItemCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs)
            .Create();

        var sentCommandYB = new Fixture()
            .Build<SentCommand>()
            .With(x => x.RobotId, robotIdY)
            .With(x => x.Command, new Fixture().Create<PlaceItemCommand>())
            .With(x => x.SentAt, DateTimeUtil.UtcNowMs.AddMilliseconds(100))
            .Create();

        await Factory.GetSentCommandRepository().Create(sentCommandXA);
        await Factory.GetSentCommandRepository().Create(sentCommandXB);

        await Factory.GetSentCommandRepository().Create(sentCommandYA);
        await Factory.GetSentCommandRepository().Create(sentCommandYB);

        var receivedCommandResultXA = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotIdX)
            .With(x => x.CommandId, sentCommandXA.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<MoveToZPositionCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs)
            .Create();

        var receivedCommandResultXB = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotIdX)
            .With(x => x.CommandId, sentCommandXB.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<ScanEnvironmentCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs.AddMilliseconds(100))
            .Create();

        var receivedCommandResultYA = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotIdY)
            .With(x => x.CommandId, sentCommandYA.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<MoveToZPositionCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs)
            .Create();

        var receivedCommandResultYB = new Fixture()
            .Build<ReceivedCommandResult>()
            .With(x => x.RobotId, robotIdY)
            .With(x => x.CommandId, sentCommandYB.CommandId)
            .With(x => x.CommandResult, new Fixture().Create<ScanEnvironmentCommandResult>())
            .With(x => x.ReceivedAt, DateTimeUtil.UtcNowMs.AddMilliseconds(100))
            .Create();

        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResultXA);
        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResultXB);

        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResultYA);
        await Factory.GetReceivedCommandResultRepository().Create(receivedCommandResultYB);

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotIdX}/command/result/received"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListReceivedCommandResultResponse>();
        responseBody.Should().NotBeNull();

        responseBody.ReceivedCommandResults.Should()
            .Contain(item => item.CommandId == sentCommandXA.CommandId)
            .Which
            .Should().BeEquivalentTo(receivedCommandResultXA);

        responseBody.ReceivedCommandResults.Should()
            .Contain(item => item.CommandId == sentCommandXB.CommandId)
            .Which
            .Should().BeEquivalentTo(receivedCommandResultXB);

        responseBody.ReceivedCommandResults.Should()
            .NotContain(item => item.CommandId == sentCommandYA.CommandId);

        responseBody.ReceivedCommandResults.Should()
            .NotContain(item => item.CommandId == sentCommandYB.CommandId);
    }

}
