using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using AutoFixture;
using Server.Util;
using Xunit;
using FluentAssertions;
namespace IntegrationTest.Repository
{
    /// <summary>
    /// Task 2 : Repository Missing Test cases for RobotRepository
    /// </summary>
    public class RobotRepositoryTest
    {
        ///<summary>
        /// Exists -> When Robot Exists should return true
        /// </summary>
        [Fact]
        public async Task WhenRobotExists_ShouldReturnTrue_WithExistMethod()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var robotId = new Fixture().Create<string>();
            await pgConainer.GetRobotRepository().Create(robotId, robotId, robotId);

            var found = await pgConainer.GetRobotRepository().Exists(robotId);
            found.Should().BeTrue();
        }
        ///<summary>
        /// Exists ->  When Robot Not Exists should return false
        /// </summary>
        [Fact]
        public async Task WhenNoRobotExists_ShouldReturnFalse_WithExistMethod()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var robotId = new Fixture().Create<string>();

            var found = await pgConainer.GetRobotRepository().Exists(robotId);
            found.Should().BeFalse();
        }

        ///<summary>
        /// Fetch -> When Robot Exists should return Robot
        /// </summary>
        [Fact]
        public async Task WhenRobotExists_ShouldReturnRobot_WithFetchMethod()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var robotId = new Fixture().Create<string>();
            await pgConainer.GetRobotRepository().Create(robotId, robotId, robotId);

            var found = await pgConainer.GetRobotRepository().Fetch(robotId);
            found.Should().NotBeNull();

            found?.RobotId.Should().Be(robotId);
            found?.LastLogInAt.Should().BeNull();
        }
        ///<summary>
        /// Fetch ->  When Robot Not Exists should return Null
        /// </summary>
        [Fact]
        public async Task WhenNoRobotExists_ShouldReturnNULL_WithFetchMethod()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var robotId = new Fixture().Create<string>();
            var robo = await pgConainer.GetRobotRepository().Fetch(robotId);

            robo.Should().BeNull();

        }
        ///<summary>
        /// LastLoginAt ->  Set LogInAt and should Update LastLoginAt, check/test before and after valie of LastLoginAt.
        /// </summary>
        [Fact]
        public async Task SetLoginAt_ShouldUpdateLoginAt()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var currentTime = DateTimeUtil.UtcNowMs;
            var robotId = new Fixture().Create<string>();
            await pgConainer.GetRobotRepository().Create(robotId, robotId, robotId);

            var beforeLogin = await pgConainer.GetRobotRepository().Fetch(robotId);
            beforeLogin?.LastLogInAt.Should().BeNull();

            await pgConainer.GetRobotRepository().SetLastLogInAtNow(robotId);

            var afterLogin = await pgConainer.GetRobotRepository().Fetch(robotId);

            afterLogin?.LastLogInAt.Should().NotBeNull();
            afterLogin?.LastLogInAt.Should().BeOnOrAfter(currentTime);


        }
        ///<summary>
        /// List() ->  when Listis empty then should only return seed data.
        /// </summary>
        [Fact]
        public async Task WhenListEmpty_ShouldReturnOnlySeedData()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var outList = await pgConainer.GetRobotRepository().List();
            outList.Should().BeEmpty();
        }
        ///<summary>
        /// Fetch Auth Credentials ->  when no robot exists thenresponse should be null.
        /// </summary>
        [Fact]
        public async Task AuthCredFetchMethod_WhnNoRobotExist_ShouldBeNULLResponse()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();
            await pgConainer.ExecuteSqlDataDefinition();
            var robotId = new Fixture().Create<string>();
            var authCred = await pgConainer.GetRobotRepository().FetchAuthCredential(robotId);
            authCred.Should().BeNull();
        }

        ///<summary>
        /// Robots Created -> should return all robots created in DB.
        /// </summary>
        [Fact]
        public async Task ListWhenRobotsCreatedinDB_ShouldReturnAllRobots()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var robotId1 = new Fixture().Create<string>();
            var robotId2 = new Fixture().Create<string>();

            await pgConainer.GetRobotRepository().Create(robotId1, robotId1, robotId1);
            await pgConainer.GetRobotRepository().Create(robotId2, robotId2, robotId2);

            var outList = await pgConainer.GetRobotRepository().List();

            outList.Count.Should().Be(2);
            outList.Should().Contain(robot => robot.RobotId == robotId1);
            outList.Should().Contain(robot => robot.RobotId == robotId2);

        }
        ///<summary>
        /// When one RonotLoggedIn should sort with LoggedIn first in List() method.
        /// </summary>
        [Fact]
        public async Task OneWhenRobotLogggedIn_SortingwithLoggedInFirst()
        {
            await using var pgConainer = new PgContainer();
            await pgConainer.StartAsync();

            await pgConainer.ExecuteSqlDataDefinition();

            var robotId1 = new Fixture().Create<string>();
            var robotId2 = new Fixture().Create<string>();

            await pgConainer.GetRobotRepository().Create(robotId1, robotId1, robotId1);
            await pgConainer.GetRobotRepository().Create(robotId2, robotId2, robotId2);

            await pgConainer.GetRobotRepository().SetLastLogInAtNow(robotId1);

            var outList = await pgConainer.GetRobotRepository().List();

            outList.Count.Should().Be(2);
            
            var indexA = outList.FindIndex(robot => robot.RobotId == robotId1);
            var indexB = outList.FindIndex(robot => robot.RobotId == robotId2);

            indexA.Should().BeLessThan(indexB);

        }
    }
}
