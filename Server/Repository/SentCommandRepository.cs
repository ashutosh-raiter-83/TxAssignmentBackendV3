using Dapper;
using RobotShared.Model.Command;
using System.Data;
using System.Text.Json;

namespace Server.Repository;

public interface ISentCommandRepository
{
    Task Create(SentCommand sentCommand);
    Task<bool> Exists(string robotId, string commandId);
    Task<SentCommand?> Fetch(string robotId, string commandId);
    Task<List<SentCommand>> List(string robotId);
}

public class SentCommandRepository : ISentCommandRepository
{
    private readonly IDbConnection _dbConnection;

    public SentCommandRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task Create(SentCommand sentCommand)
    {
        await _dbConnection.ExecuteAsync(
            @"INSERT INTO
                sortbot.sent_command (command_id, robot_id, command, sent_at)
            VALUES
                (@CommandId, @RobotId, @Command::json, @SentAt)",
            new
            {
                sentCommand.CommandId,
                sentCommand.RobotId,
                Command = JsonSerializer.Serialize(sentCommand.Command),
                sentCommand.SentAt,
            }
        );
    }

    public async Task<bool> Exists(string robotId, string commandId)
    {
        return await _dbConnection.QuerySingleAsync<bool>(
            @"SELECT EXISTS(
                SELECT
                    *
                FROM
                    sortbot.sent_command
                WHERE
                    command_id = @CommandId AND
                    robot_id = @RobotId
            )",
            new
            {
                CommandId = commandId,
                RobotId = robotId,
            }
        );
    }

    /// <summary>
    /// Task 2: Missing Unit/Integration Tests
    /// </summary>
    public async Task<SentCommand?> Fetch(string robotId, string commandId)
    {
        var sentCommand = await _dbConnection.QuerySingleOrDefaultAsync(
            @"SELECT
                *
            FROM
                sortbot.sent_command
            WHERE
                command_id = @CommandId",
            new
            {
                CommandId = commandId,
                RobotId = robotId,
            }
        );
        return sentCommand == null ?
            null :
            new SentCommand()
            {
                CommandId = sentCommand.command_id,
                RobotId = sentCommand.robot_id,
                Command = JsonSerializer.Deserialize<CommandBase>(sentCommand.command),
                SentAt = sentCommand.sent_at,
            };
    }

    public async Task<List<SentCommand>> List(string robotId)
    {
        var sentCommands = await _dbConnection.QueryAsync(
            @"SELECT
                *
            FROM
                sortbot.sent_command
            WHERE
                robot_id = @RobotId
            ORDER BY
                sent_at DESC",
            new
            {
                RobotId = robotId,
            }
        );
        return sentCommands
            .Select(row => new SentCommand()
            {
                CommandId = row.command_id,
                RobotId = row.robot_id,
                Command = JsonSerializer.Deserialize<CommandBase>(row.command),
                SentAt = row.sent_at,
            })
            .ToList();
    }

}
