using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotShared.Hub;
using RobotShared.Model.Http.TechnicianUser;
using Server.Auth;
using Server.Hub;
using Server.Repository;
using System.Data;

namespace Server.Controller.TechnicianUser;

[ApiController]
[Route("technician-user")]
[TechnicianUserAuthorization]
public class RobotController : ControllerBase
{
    private readonly IConnectedRobotCollection _connectedRobotCollection;
    private readonly IRobotRepository _robotRepository;
    public RobotController(
        IConnectedRobotCollection connectedRobotCollection,
        IRobotRepository robotRepository
    )
    {
        _connectedRobotCollection = connectedRobotCollection;
        _robotRepository = robotRepository;
    }

    /// <summary>
    /// Fetches a single Robot
    /// </summary>
    [HttpGet("robot/{robotId}", Name = "TechnicianUser-FetchRobot")]
    [ProducesResponseType(typeof(FetchRobotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fetch([FromRoute] string robotId)
    {
        var robot = await _robotRepository.Fetch(robotId);
        if (robot == null)
        {
            return NotFound();
        }

        return Ok(new FetchRobotResponse()
        {
            RobotId = robot.RobotId,
            LastLogInAt = robot.LastLogInAt,
            IsOnline = _connectedRobotCollection.TryGetClient(robotId, out _),
        });
    }

    /// <summary>
    /// Lists the 100 most recent logged in robots
    /// </summary>
    [HttpGet("robot", Name = "TechnicianUser-ListRobot")]
    [ProducesResponseType(typeof(ListRobotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List()
    {
        var robots = await _robotRepository.List();

        return Ok(new ListRobotResponse()
        {
            Robots = robots
                .Select(row => new FetchRobotResponse()
                {
                    RobotId = row.RobotId,
                    LastLogInAt = row.LastLogInAt,
                    IsOnline = _connectedRobotCollection.TryGetClient(row.RobotId, out IRobotHubReceiver? _),
                })
                .ToList(),
        });
    }
}
