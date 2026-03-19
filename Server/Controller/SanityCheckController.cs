using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotShared.Model.Http;
using System.Data;

namespace Server.Controller;

[ApiController]
[Route("")]
public class SanityCheckController : ControllerBase
{
    private readonly IDbConnection _dbConnection;

    public SanityCheckController(IDbConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }

    /// <summary>
    /// Checks if the server is running and ready to process requests
    /// </summary>
    [HttpGet("", Name = "GetHeartBeat")]
    [ProducesResponseType(typeof(GetHeartBeatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetHeartBeat()
    {
        try
        {
            var version = await _dbConnection.QuerySingleAsync<string>("SELECT version()");
            return Ok(new GetHeartBeatResponse()
            {
                StillAlive = true,
                PgVersion = version,
            });
        }
        catch
        {
            return Problem();
        }
    }
}
