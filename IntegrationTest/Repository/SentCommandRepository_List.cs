using AutoFixture;
using FluentAssertions;
using RobotShared.Model.Command;
using Server.Util;
using Xunit;

namespace IntegrationTest.Repository;

public class SentCommandRepository_List
{
    /// <summary>
    /// Task 1: Bug Fixes
    /// </summary>
    [Fact]
    public async Task WhenMoreThan100_ShouldTruncate()
    {
        await using var pgContainer = new PgContainer();
        await pgContainer.StartAsync();
        await pgContainer.ExecuteSqlDataDefinition();

        var robotId = new Fixture().Create<string>();
        await pgContainer.GetRobotRepository().Create(robotId, robotId, robotId);

        var now = DateTimeUtil.UtcNowMs;
        for (var i=0; i<200; ++i)
        {
            await pgContainer.GetSentCommandRepository().Create(new RobotShared.Model.Command.SentCommand()
            {
                CommandId = new Fixture().Create<string>(),
                RobotId = robotId,
                Command = new Fixture().Create<MoveToZPositionCommand>(),
                SentAt = now.AddMilliseconds(i),
            });
        }

        var list = await pgContainer.GetSentCommandRepository().List(robotId);
        list.Count.Should().Be(100);
    }
}
