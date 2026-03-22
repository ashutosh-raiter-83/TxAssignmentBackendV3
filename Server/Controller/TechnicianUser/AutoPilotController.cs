using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotShared.Model.Command;
using RobotShared.Model.Http.TechnicianUser;
using Server.Auth;
using Server.AutoPilot;
using Server.Hub;
using Server.Repository;
using Server.Util;
using System.Net;

namespace Server.Controller.TechnicianUser;

[ApiController]
[Route("technician-user")]
[TechnicianUserAuthorization]
public class AutoPilotController : ControllerBase
{
    private readonly IAutoPilotManager _autoPilotManager;
    private readonly IRobotRepository _robotRepository;

    public AutoPilotController(
        IAutoPilotManager autoPilotManager,
        IRobotRepository robotRepository
    )
    {
        _autoPilotManager = autoPilotManager;
        _robotRepository = robotRepository;
    }

    /// <summary>
    /// Activates auto pilot mode for specified root,Robot must be online
    /// </summary>
    [HttpPost("robot/{robotId}/autopilot/activate", Name = "TechnicianUser-ActivateAutoPilot")]
    [ProducesResponseType(typeof(AutoPilotStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Activate([FromRoute] string robotId)
    {
        if (!await _robotRepository.Exists(robotId))
            return NotFound();

        if(_autoPilotManager.GetState(robotId) != AutoPilotState.Deactivated)
        {
            return Ok(new AutoPilotStateResponse
            {
                State = _autoPilotManager.GetState(robotId).ToString(),
            });
        }
        var activated=await _autoPilotManager.ActivateAutoPilot(robotId);
        if (!activated)
        {
            return Problem(
                detail:"Robot is not online,try again",
                statusCode:(int)HttpStatusCode.UnprocessableEntity
                );
        }

        return Ok(new AutoPilotStateResponse
        {
            State = _autoPilotManager.GetState(robotId).ToString(),
        });
    }
    /// <summary>
    /// DeActivates auto pilot mode for specified root.
    /// </summary>
    [HttpPost("robot/{robotId}/autopilot/deactivate", Name = "TechnicianUser-DeActivateAutoPilot")]
    [ProducesResponseType(typeof(AutoPilotStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate([FromRoute] string robotId)
    {
        if (!await _robotRepository.Exists(robotId))
            return NotFound();

        _autoPilotManager.DeActivateAutoPilot(robotId);
        return Ok(new AutoPilotStateResponse
        {
            State = AutoPilotState.Deactivated.ToString(),
        });
    }
    /// <summary>
    /// Get current autopilot state for given robot
    /// </summary>
    [HttpGet("robot/{robotId}/autopilot/state", Name = "TechnicianUser-GetAutoPilotState")]
    [ProducesResponseType(typeof(AutoPilotStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetState([FromRoute] string robotId)
    {
        if (!await _robotRepository.Exists(robotId))
            return NotFound();

        return Ok(new AutoPilotStateResponse
        {
            State = _autoPilotManager.GetState(robotId).ToString(),
        });
    }
    /// <summary>
    /// Get autopilot statistics for given robot
    /// </summary>
    [HttpGet("robot/{robotId}/autopilot/statistics", Name = "TechnicianUser-GetAutoPilotStatistics")]
    [ProducesResponseType(typeof(AutoPilotStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatistics([FromRoute] string robotId)
    {
        if (!await _robotRepository.Exists(robotId))
            return NotFound();
        
        var session = _autoPilotManager.GetSession(robotId);
        return Ok(new AutoPilotStatisticResponse
        {
            State = _autoPilotManager.GetState(robotId).ToString(),
            ItemsSorted = session?.ItemsSorted ?? 0,
            CommandsSent = session?.CommandSent ?? 0,
            ErrorsEncountered = session?.ErrorEncountered ?? 0
        });
    }
}
