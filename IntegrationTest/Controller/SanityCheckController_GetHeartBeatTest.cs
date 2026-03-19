using FluentAssertions;
using RobotShared.Model.Http;
using System.Net.Http.Json;
using Xunit;

namespace IntegrationTest.Controller;

public class SanityCheckController_GetHeartBeatTest : WebAppFactoryFixture
{
    public SanityCheckController_GetHeartBeatTest(WebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task ShouldSucceed()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadFromJsonAsync<GetHeartBeatResponse>();
        responseBody.Should().NotBeNull();
        responseBody.StillAlive.Should().BeTrue();
        responseBody.PgVersion.Should().NotBeNullOrEmpty();
    }

}
