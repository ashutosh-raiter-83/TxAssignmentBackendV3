using Dapper;
using RobotShared.Model.CommandResult;
using System.Data;
using System.Text.Json;

namespace Server.Repository;

public interface IReceivedCommandResultRepository
{
    Task Create(ReceivedCommandResult receivedCommandResult);
    Task<ReceivedCommandResult?> Fetch(string robotId, string commandId);
    Task<List<ReceivedCommandResult>> List(string robotId);
}

public class ReceivedCommandResultRepository : IReceivedCommandResultRepository
{
    private readonly IDbConnection _dbConnection;

    public ReceivedCommandResultRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task Create(ReceivedCommandResult receivedCommandResult)
    {
        await _dbConnection.ExecuteAsync(
            @"INSERT INTO
                sortbot.received_command_result (command_id, robot_id, command_result, received_at)
            VALUES
                (@CommandId, @RobotId, @CommandResult::json, @ReceivedAt)",
            new
            {
                receivedCommandResult.CommandId,
                receivedCommandResult.RobotId,
                CommandResult = JsonSerializer.Serialize(receivedCommandResult.CommandResult),
                receivedCommandResult.ReceivedAt,
            }
        );
    }

    public async Task<ReceivedCommandResult?> Fetch(string robotId, string commandId)
    {
        var receivedCommandResult = await _dbConnection.QuerySingleOrDefaultAsync(
            @"SELECT
                *
            FROM
                sortbot.received_command_result
            WHERE
                command_id = @CommandId AND
                robot_id = @RobotId",
            new
            {
                CommandId = commandId,
                RobotId = robotId,
            }
        );
        return receivedCommandResult == null ?
            null :
            new ReceivedCommandResult()
            {
                CommandId = receivedCommandResult.command_id,
                RobotId = receivedCommandResult.robot_id,
                CommandResult = JsonSerializer.Deserialize<CommandResultBase>(receivedCommandResult.command_result),
                ReceivedAt = receivedCommandResult.received_at,
            };
    }

    public async Task<List<ReceivedCommandResult>> List(string robotId)
    {
        var receivedCommandResults = await _dbConnection.QueryAsync(
            @"SELECT
                *
            FROM
                sortbot.received_command_result
            WHERE
                robot_id = @RobotId
            ORDER BY
                received_at DESC
            LIMIT 100",
            new
            {
                RobotId = robotId,
            }
        );

        return receivedCommandResults
            .Select(row => new ReceivedCommandResult()
            {
                CommandId = row.command_id,
                RobotId = row.robot_id,
                CommandResult = JsonSerializer.Deserialize<CommandResultBase>(row.command_result),
                ReceivedAt = row.received_at,
            })
            .ToList();
    }

}
