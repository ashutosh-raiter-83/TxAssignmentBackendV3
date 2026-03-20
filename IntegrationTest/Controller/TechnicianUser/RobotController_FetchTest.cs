using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Http;
using RobotShared.Model.Http.TechnicianUser;
using System.Net.Http.Json;
using Xunit;
using System.Net;
using Server.Util;

namespace IntegrationTest.Controller.TechnicianUser;

public class RobotController_FetchTest : WebAppFactoryFixture
{
    public RobotController_FetchTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenUnauthenticated_ShouldReceiveUnauthorized()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/Robot1"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUserTypeRobot_ShouldReceiveUnauthorized()
    {
        var client = Factory.CreateRobotClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/Robot1"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenRobotNotExists_ShouldReceiveNotFound()
    {
        var robotId = new Fixture().Create<string>();
        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}"
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

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<FetchRobotResponse>();
        responseBody.Should().NotBeNull();
        responseBody.RobotId.Should().Be(robotId);
        responseBody.LastLogInAt.Should().BeNull();
        responseBody.IsOnline.Should().BeFalse();
    }

    [Fact]
    public async Task WhenRobotOffline_ShouldSetIsOnlineFalse()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        Factory.SetRobotOffline(robotId);

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<FetchRobotResponse>();
        responseBody.Should().NotBeNull();
        responseBody.RobotId.Should().Be(robotId);
        responseBody.LastLogInAt.Should().BeNull();
        responseBody.IsOnline.Should().BeFalse();
    }

    [Fact]
    public async Task WhenRobotOnline_ShouldSetIsOnlineTrue()
    {
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        Factory.SetRobotOnline(robotId);

        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<FetchRobotResponse>();
        responseBody.Should().NotBeNull();
        responseBody.RobotId.Should().Be(robotId);
        responseBody.LastLogInAt.Should().BeNull();
        responseBody.IsOnline.Should().BeTrue();
    }

    /// <summary>
    /// Task 1: Bug Fixes
    /// Fix added in LogInController-> LogInAsRobot Action with to set the LoginAt timestamp to current time.
    /// Never updated the LoginAt timestamp here in DB.
    /// </summary>
    [Fact]
    public async Task WhenRobotLoggedIn_ShouldSetlastLogInAt()
    {
        var now = DateTimeUtil.UtcNowMs;
        var robotId = new Fixture().Create<string>();
        var username = new Fixture().Create<string>();
        var password = new Fixture().Create<string>();

        await Factory.GetRobotRepository().Create(robotId, username, password);

        Factory.SetRobotOnline(robotId);

        var client = Factory.CreateTechnicianClient();
        await client.PostAsync(
            "/log-in/robot",
            JsonContent.Create(new LogInAsRobotRequest()
            {
                Username = username,
                Password = password,
            })
        );

        var response = await client.GetAsync(
            $"/technician-user/robot/{robotId}"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<FetchRobotResponse>();
        responseBody.Should().NotBeNull();
        responseBody.RobotId.Should().Be(robotId);
        responseBody.LastLogInAt.Should().BeOnOrAfter(now);
        responseBody.IsOnline.Should().BeTrue();
    }

}
