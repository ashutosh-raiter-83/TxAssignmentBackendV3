using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Command;
using Server.Util;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTest.Controller.TechnicianUser;

public class SentCommandController_FetchTest : WebAppFactoryFixture
{
    public SentCommandController_FetchTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenUnauthenticated_ShouldReceiveUnauthorized()
    {
        var robotId = new Fixture().Create<string>();
        var commandId = new Fixture().Create<string>();
        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/{commandId}/sent"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUserTypeRobot_ShouldReceiveUnauthorized()
    {
        var robotId = new Fixture().Create<string>();
        var commandId = new Fixture().Create<string>();
        var client = Factory.CreateRobotClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/{commandId}/sent"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenRobotNotExists_ShouldReceiveNotFound()
    {
        var robotId = new Fixture().Create<string>();
        var commandId = new Fixture().Create<string>();
        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/{commandId}/sent"
        );
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WhenCommandNotExists_ShouldReceiveNotFound()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        var commandId = new Fixture().Create<string>();
        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/{commandId}/sent"
        );
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
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

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}/command/{sentCommand.CommandId}/sent"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<SentCommand>();
        responseBody.Should().NotBeNull();
        responseBody.Should().BeEquivalentTo(sentCommand);
    }
}
