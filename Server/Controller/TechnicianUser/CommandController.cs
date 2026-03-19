using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RobotShared.Model.Command;
using RobotShared.Model.Http.TechnicianUser;
using Server.Auth;
using Server.Hub;
using Server.Repository;
using Server.Util;
using System.Net;

namespace Server.Controller.TechnicianUser;

[ApiController]
[Route("technician-user")]
[TechnicianUserAuthorization]
public class CommandController : ControllerBase
{
    private readonly IConnectedRobotCollection _connectedRobotCollection;
    private readonly IRobotRepository _robotRepository;
    private readonly ISentCommandRepository _sentCommandRepository;
    public CommandController(
        IConnectedRobotCollection connectedRobotCollection,
        IRobotRepository robotRepository,
        ISentCommandRepository sentCommandRepository
    )
    {
        _connectedRobotCollection = connectedRobotCollection;
        _robotRepository = robotRepository;
        _sentCommandRepository = sentCommandRepository;
    }

    /// <summary>
    /// Sends a command to the robot, if it is connected
    /// </summary>
    [HttpPost("robot/{robotId}/command", Name = "TechnicianUser-SendCommand")]
    [ProducesResponseType(typeof(SendCommandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Send([FromRoute] string robotId, [FromBody] SendCommandRequest request)
    {
        if (_connectedRobotCollection.TryGetClient(robotId, out var client))
        {
            var sentCommand = new SentCommand()
            {
                CommandId = Guid.NewGuid().ToString(),
                RobotId = robotId,
                Command = request.Command,
                SentAt = DateTimeUtil.UtcNowMs,
            };
            await _sentCommandRepository.Create(sentCommand);
            // Task 4: Documentation
            //
            // What happens if command is persisted,
            // but sending the command fails?
            // (e.g. robot goes offline in between)
            //
            // What happens if command is sent first instead,
            // but persisting the command to DB fails?
            //
            // How would you improve the current design to handle partial success?
            await client.OnCommandReceived(sentCommand);
            return Ok(new SendCommandResponse()
            {
                SentCommand = sentCommand,
            });
        }

        if (!await _robotRepository.Exists(robotId))
        {
            return NotFound();
        }

        return Problem(
            detail: "Robot is not online, please try again later",
            statusCode: (int)HttpStatusCode.UnprocessableEntity
        );
    }
}
