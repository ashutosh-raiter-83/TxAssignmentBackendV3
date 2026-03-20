using AutoFixture;
using FluentAssertions;
using Microsoft.OpenApi;
using RobotShared.Model.Http;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Xunit;

namespace IntegrationTest.Controller;

public class LogInController_LogInAsTechnicianTest : WebAppFactoryFixture
{
    public LogInController_LogInAsTechnicianTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task WhenTechnicianExists_ShouldSucceed()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = "Technician1",
                Password = "Technician1",
            })
        );
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<LogInAsTechnicianResponse>();
        responseBody.Should().NotBeNull();
        responseBody.Token.Should().NotBeNullOrEmpty();
        var token = Factory.TryParseToken(responseBody.Token);
        token.Should().NotBeNull();
        var user = new ClaimsPrincipal(new ClaimsIdentity(token.Claims));
        user.FindFirst("userType").Value.Should().Be(UserType.Technician.ToString());
        user.FindFirst("userId").Value.Should().Be("Technician1");
    }

    [Fact]
    public async Task WhenTechnicianNotExists_ShouldReceiveUnauthorized()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = new Fixture().Create<string>(),
                Password = new Fixture().Create<string>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task WhenUsernameTooShort_ShouldReceiveBadRequest()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = "",
                Password = new Fixture().Create<string>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenPasswordTooShort_ShouldReceiveBadRequest()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = new Fixture().Create<string>(),
                Password = "",
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenUsernameTooLong_ShouldReceiveBadRequest()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = new string('x', 257),
                Password = new Fixture().Create<string>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenPasswordTooLong_ShouldReceiveBadRequest()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = new Fixture().Create<string>(),
                Password = new string('x', 257),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenUsernameNotSet_ShouldReceiveBadRequest()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new
            {
                Password = new Fixture().Create<string>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task WhenPasswordNotSet_ShouldReceiveBadRequest()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new
            {
                Username = new Fixture().Create<string>(),
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
    //Task 2 : Missing Unit/Integration Tests
    //Added test case for for wrong password, which should return Unauthorized.
    [Fact]
    public async Task WhenPasswordWrong_ShouldReceiveUnauthorized()
    {
        var client = Factory.CreateClient();
        var response = await client.PostAsync(
            "/log-in/technician",
            JsonContent.Create(new LogInAsTechnicianRequest()
            {
                Username = "Technician1",
                Password = "WrongPassword",
            })
        );
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
    
    //Task 2 : Missing Unit/Integration Tests
    // Fetch Invalid Token and Should be UnAuthorized
    [Fact]
    public async Task TokenWithFetchWithInvalidToke_ShouldbeUnauthorized()
    {
        var client = Factory.CreateClient();

        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "Putting-Invalid-Token-Here");


        var response = await client.GetAsync($"/technician-user/robot/Robot1");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

    }
}
