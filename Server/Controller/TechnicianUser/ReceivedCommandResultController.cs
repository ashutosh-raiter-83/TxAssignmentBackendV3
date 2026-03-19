using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotShared.Model.CommandResult;
using RobotShared.Model.Http.TechnicianUser;
using Server.Auth;
using Server.Repository;

namespace Server.Controller.TechnicianUser;

[ApiController]
[Route("technician-user")]
[TechnicianUserAuthorization]
public class ReceivedCommandResultController : ControllerBase
{
    private readonly IRobotRepository _robotRepository;
    private readonly IReceivedCommandResultRepository _receivedCommandResultRepository;
    public ReceivedCommandResultController(
        IRobotRepository robotRepository,
        IReceivedCommandResultRepository receivedCommandResultRepository
    )
    {
        _robotRepository = robotRepository;
        _receivedCommandResultRepository = receivedCommandResultRepository;
    }

    /// <summary>
    /// Fetches a single CommandResult
    /// </summary>
    [HttpGet("robot/{robotId}/command/{commandId}/result/received", Name = "TechnicianUser-FetchReceivedCommandResult")]
    [ProducesResponseType(typeof(ReceivedCommandResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Fetch([FromRoute] string robotId, [FromRoute] string commandId)
    {
        var receivedCommandResult = await _receivedCommandResultRepository.Fetch(robotId, commandId);
        return receivedCommandResult == null ?
            NotFound() :
            Ok(receivedCommandResult);
    }

    /// <summary>
    /// Lists the 100 most recent command results
    /// </summary>
    [HttpGet("robot/{robotId}/command/result/received", Name = "TechnicianUser-ListReceivedCommandResult")]
    [ProducesResponseType(typeof(ListReceivedCommandResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> List([FromRoute] string robotId)
    {
        if (!await _robotRepository.Exists(robotId))
        {
            return NotFound();
        }

        return Ok(new ListReceivedCommandResultResponse()
        {
            ReceivedCommandResults = await _receivedCommandResultRepository.List(robotId),
        });
    }
}
