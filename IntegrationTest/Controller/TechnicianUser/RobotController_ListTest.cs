using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Http;
using RobotShared.Model.Http.TechnicianUser;
using System.Net.Http.Json;
using Xunit;
using System.Net;
using Server.Util;

namespace IntegrationTest.Controller.TechnicianUser;

public class RobotController_ListTest : WebAppFactoryFixture
{
    public RobotController_ListTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenUnauthenticated_ShouldReceiveUnauthorized()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync(
            $"/technician-user/robot"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUserTypeRobot_ShouldReceiveUnauthorized()
    {
        var client = Factory.CreateRobotClient();
        var response = await client.GetAsync(
            $"/technician-user/robot"
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUserTypeTechnician_ShouldSucceed()
    {
        var client = Factory.CreateTechnicianClient();
        var response = await client.GetAsync(
            $"/technician-user/robot"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListRobotResponse>();
        responseBody.Should().NotBeNull();
        responseBody.Robots.Should().NotBeEmpty();
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
            $"/technician-user/robot"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListRobotResponse>();
        responseBody.Should().NotBeNull();

        var robot = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotId)
            .Which;

        robot.LastLogInAt.Should().BeNull();
        robot.IsOnline.Should().BeFalse();
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
            $"/technician-user/robot"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListRobotResponse>();
        responseBody.Should().NotBeNull();

        var robot = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotId)
            .Which;

        robot.LastLogInAt.Should().BeNull();
        robot.IsOnline.Should().BeTrue();
    }

    /// <summary>
    /// Task 1: Bug Fixes
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
            $"/technician-user/robot"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListRobotResponse>();
        responseBody.Should().NotBeNull();

        var robot = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotId)
            .Which;

        robot.LastLogInAt.Should().BeOnOrAfter(now);
        robot.IsOnline.Should().BeTrue();
    }

    /// <summary>
    /// Task 1: Bug Fixes
    /// </summary>
    [Fact]
    public async Task WhenMultipleRobotLoggedIn_ShouldSortByMostRecentLoggedIn()
    {
        var robotIdA = new Fixture().Create<string>();
        var usernameA = new Fixture().Create<string>();
        var passwordA = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotIdA, usernameA, passwordA);

        var robotIdB = new Fixture().Create<string>();
        var usernameB = new Fixture().Create<string>();
        var passwordB = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotIdB, usernameB, passwordB);

        var client = Factory.CreateTechnicianClient();
        await client.PostAsync(
            "/log-in/robot",
            JsonContent.Create(new LogInAsRobotRequest()
            {
                Username = usernameA,
                Password = passwordA,
            })
        );
        await Task.Delay(1000);
        await client.PostAsync(
            "/log-in/robot",
            JsonContent.Create(new LogInAsRobotRequest()
            {
                Username = usernameB,
                Password = passwordB,
            })
        );

        var response = await client.GetAsync(
            $"/technician-user/robot"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListRobotResponse>();
        responseBody.Should().NotBeNull();

        var robotA = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotIdA)
            .Which;

        var robotB = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotIdB)
            .Which;

        robotA.LastLogInAt.Should().BeBefore(robotB.LastLogInAt.Value);
        responseBody.Robots.IndexOf(robotA)
            .Should().BeGreaterThan(responseBody.Robots.IndexOf(robotB));
    }

    /// <summary>
    /// Task 1: Bug Fixes
    /// </summary>
    [Fact]
    public async Task WhenOneRobotLoggedInAndOtherNot_ShouldSortLoggedInFirst()
    {
        var robotIdA = new Fixture().Create<string>();
        var usernameA = new Fixture().Create<string>();
        var passwordA = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotIdA, usernameA, passwordA);

        var robotIdB = new Fixture().Create<string>();
        var usernameB = new Fixture().Create<string>();
        var passwordB = new Fixture().Create<string>();
        await Factory.GetRobotRepository().Create(robotIdB, usernameB, passwordB);

        var client = Factory.CreateTechnicianClient();
        await client.PostAsync(
            "/log-in/robot",
            JsonContent.Create(new LogInAsRobotRequest()
            {
                Username = usernameA,
                Password = passwordA,
            })
        );

        var response = await client.GetAsync(
            $"/technician-user/robot"
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<ListRobotResponse>();
        responseBody.Should().NotBeNull();

        var robotA = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotIdA)
            .Which;

        var robotB = responseBody.Robots.Should()
            .Contain(robot => robot.RobotId == robotIdB)
            .Which;

        robotA.LastLogInAt.Should().NotBeNull();
        robotB.LastLogInAt.Should().BeNull();
        responseBody.Robots.IndexOf(robotA)
            .Should().BeLessThan(responseBody.Robots.IndexOf(robotB));
    }
}
