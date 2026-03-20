using Dapper;
using Server.Repository.Model;
using System.Data;

namespace Server.Repository;

public interface IRobotRepository
{
    Task Create(string robotId, string username, string password);
    Task<bool> Exists(string robotId);
    Task<Robot?> Fetch(string robotId);
    Task<RobotAuthCredential?> FetchAuthCredential(string username);
    Task<List<Robot>> List();
    Task SetLastLogInAtNow(string robotId);
}

public class RobotRepository : IRobotRepository
{
    private readonly IDbConnection _dbConnection;

    public RobotRepository(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    public async Task Create(string robotId, string username, string password)
    {
        await _dbConnection.ExecuteAsync(
            @"INSERT INTO
                sortbot.robot (robot_id, username, password)
            VALUES
	            (@RobotId, @Username, @Password)",
            new
            {
                RobotId = robotId,
                Username = username,
                Password = password,
            }
        );
    }

    public async Task<bool> Exists(string robotId)
    {
        return await _dbConnection.QuerySingleAsync<bool>(
            @"SELECT EXISTS(
                SELECT
                    *
                FROM
                    sortbot.robot
                WHERE
                    robot_id = @RobotId
            )",
            new
            {
                RobotId = robotId,
            }
        );
    }

    public async Task<Robot?> Fetch(string robotId)
    {
        var robot = await _dbConnection.QuerySingleOrDefaultAsync(
            @"SELECT
                robot_id,
                last_log_in_at
            FROM
                sortbot.robot
            WHERE
                robot_id = @RobotId",
            new
            {
                RobotId = robotId,
            }
        );
        return robot == null ?
            null :
            new Robot()
            {
                RobotId = robot.robot_id,
                LastLogInAt = robot.last_log_in_at,
            };
    }

    public async Task<RobotAuthCredential?> FetchAuthCredential(string username)
    {
        var credential = await _dbConnection.QuerySingleOrDefaultAsync(
            @"SELECT
                robot_id,
                password
            FROM
                sortbot.robot
            WHERE
                username = @Username",
            new
            {
                username,
            }
        );
        return credential == null ?
            null :
            new RobotAuthCredential()
            {
                RobotId = credential.robot_id,
                Username = username,
                Password = credential.password,
            };
    }
    //<summary>
    /*
     * Task  1: Bug Fix
          have made changes instead of DESC in  list added NULLS LAST 
          Becaause of this robots never looogged in appear before robots that have.
            most recently logged in robots appear first, 
            and robots that have never logged in appear at the end of the list.Also added LIMIT 100 to cap results to 100 robots.
         * */
    //</summary>
    public async Task<List<Robot>> List()
    {
        var robots = await _dbConnection.QueryAsync(
            @"SELECT
                *
            FROM
                sortbot.robot
            ORDER BY
                last_log_in_at DESC NULLS LAST, 
                robot_id ASC
            LIMIT 100"
        );
        return robots
            .Select(row => new Robot()
            {
                RobotId = row.robot_id,
                LastLogInAt = row.last_log_in_at,
            })
            .ToList();
    }

    public async Task SetLastLogInAtNow(string robotId)
    {
        await _dbConnection.ExecuteAsync(
            @"UPDATE
                sortbot.robot
            SET
                last_log_in_at = @Now
            WHERE
                robot_id = @RobotId",
            new
            {
                Now = Util.DateTimeUtil.UtcNowMs,
                RobotId = robotId,
            }
        );
    }
}
