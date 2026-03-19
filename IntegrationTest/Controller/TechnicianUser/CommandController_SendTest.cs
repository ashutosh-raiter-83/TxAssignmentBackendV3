using AutoFixture;
using FluentAssertions;
using Moq;
using RobotShared.Model.Command;
using RobotShared.Model.Http.TechnicianUser;
using Server.Util;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTest.Controller.TechnicianUser;

public class CommandController_SendTest : WebAppFactoryFixture
{
    public CommandController_SendTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenUnauthenticated_ShouldReceiveUnauthorized()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            $"/technician-user/robot/{robotId}/command",
            JsonContent.Create(new SendCommandRequest()
            {
                Command = new Fixture().Create<MoveToZPositionCommand>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUserTypeRobot_ShouldReceiveUnauthorized()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateRobotClient();
        var response = await client.PostAsync(
            $"/technician-user/robot/{robotId}/command",
            JsonContent.Create(new SendCommandRequest()
            {
                Command = new Fixture().Create<MoveToZPositionCommand>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenRobotNotExists_ShouldReceiveNotFound()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateTechnicianClient();
        var response = await client.PostAsync(
            $"/technician-user/robot/{robotId}/command",
            JsonContent.Create(new SendCommandRequest()
            {
                Command = new Fixture().Create<MoveToZPositionCommand>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenRobotOnline_ShouldSucceed()
    {
        var now = DateTimeUtil.UtcNowMs;

        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        var command = new Fixture().Create<MoveToZPositionCommand>();

        var robotClient = Factory.SetRobotOnline(robotId);

        var client = Factory.CreateTechnicianClient();
        var response = await client.PostAsync(
            $"/technician-user/robot/{robotId}/command",
            JsonContent.Create(new SendCommandRequest()
            {
                Command = command,
            })
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<SendCommandResponse>();
        responseBody.Should().NotBeNull();
        responseBody.SentCommand.CommandId.Should().NotBeNullOrEmpty();
        responseBody.SentCommand.RobotId.Should().Be(robotId);
        responseBody.SentCommand.SentAt.Should().BeOnOrAfter(now);
        responseBody.SentCommand.Command.Should().BeEquivalentTo(command);

        robotClient.Verify(
            x => x.OnCommandReceived(It.Is<SentCommand>(sentCommand =>
                FluentVerifier.VerifyFluentAssertion(() =>
                    sentCommand.Should().BeEquivalentTo(responseBody.SentCommand, "")
                )
            )),
            Times.Once
        );
        
        var persisted = await Factory.GetSentCommandRepository()
            .Fetch(robotId, responseBody.SentCommand.CommandId);
        persisted.Should().BeEquivalentTo(responseBody.SentCommand);
    }

    [Fact]
    public async Task WhenRobotOffline_ShouldReceiveUnprocessableEntity()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        Factory.SetRobotOffline(robotId);

        var client = Factory.CreateTechnicianClient();
        var response = await client.PostAsync(
            $"/technician-user/robot/{robotId}/command",
            JsonContent.Create(new SendCommandRequest()
            {
                Command = new Fixture().Create<MoveToZPositionCommand>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
    }
}
