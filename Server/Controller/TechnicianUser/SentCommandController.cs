using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotShared.Model.Command;
using RobotShared.Model.Http.TechnicianUser;
using Server.Auth;
using Server.Repository;

namespace Server.Controller.TechnicianUser;

[ApiController]
[Route("technician-user")]
[TechnicianUserAuthorization]
public class SentCommandController : ControllerBase
{
    private readonly IRobotRepository _robotRepository;
    private readonly ISentCommandRepository _sentCommandRepository;
    public SentCommandController(
        IRobotRepository robotRepository,
        ISentCommandRepository sentCommandRepository
    )
    {
        _robotRepository = robotRepository;
        _sentCommandRepository = sentCommandRepository;
    }

    /// <summary>
    /// Fetches a single SentCommand
    /// </summary>
    [HttpGet("robot/{robotId}/command/{commandId}/sent", Name = "TechnicianUser-FetchSentCommand")]
    [ProducesResponseType(typeof(SentCommand), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fetch([FromRoute] string robotId, [FromRoute] string commandId)
    {
        var sentCommand = await _sentCommandRepository.Fetch(robotId, commandId);
        return sentCommand == null ? NotFound() : Ok(sentCommand);
    }

    /// <summary>
    /// Lists the 100 most recent sent commands
    /// </summary>
    [HttpGet("robot/{robotId}/command/sent", Name = "TechnicianUser-ListSentCommand")]
    [ProducesResponseType(typeof(ListSentCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List([FromRoute] string robotId)
    {
        if (!await _robotRepository.Exists(robotId))
        {
            return NotFound();
        }

        return Ok(new ListSentCommandResponse()
        {
            SentCommands = await _sentCommandRepository.List(robotId),
        });
    }
}
