using Grpc.Core;
using MagicOnion;
using MagicOnion.Server.Hubs;
using Microsoft.AspNetCore.Authorization;
using RobotShared.Hub;
using RobotShared.Model;
using RobotShared.Model.CommandResult;
using Server.Repository;
using Server.Util;
using Server.AutoPilot;
namespace Server.Hub;

[Authorize]
public class RobotHub : StreamingHubBase<IRobotHub, IRobotHubReceiver>, IRobotHub
{
    private readonly IConnectedRobotCollection _connectedRobotCollection;
    private readonly ISentCommandRepository _sentCommandRepository;
    private readonly IReceivedCommandResultRepository _receivedCommandResultRepository;

    private string _robotId;
    private readonly IAutoPilotManager _autoPilotManager;
    public RobotHub(
        IConnectedRobotCollection connectedRobotCollection,
        ISentCommandRepository sentCommandRepository,
        IReceivedCommandResultRepository receivedCommandResultRepository,
        IAutoPilotManager autoPilotManager
    )
    {
        _connectedRobotCollection = connectedRobotCollection;
        _sentCommandRepository = sentCommandRepository;
        _receivedCommandResultRepository = receivedCommandResultRepository;
        _autoPilotManager = autoPilotManager;
    }

    private string EnsureRobotId()
    {
        var userId = Context.CallContext.GetHttpContext().User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            throw new Exception("No userId found");
        }
        return userId;
    }

    public Task<string> GetHelloWorld()
    {
        return Task.FromResult("Hello, world!");
    }

    public async Task ReportCommandResult(CommandResultBase commandResult)
    {
        Console.WriteLine($"ReportCommandResult, commandResult={commandResult}, robotId={_robotId}");
        if (!await _sentCommandRepository.Exists(_robotId, commandResult.CommandId))
        {
            throw new ReturnStatusException(StatusCode.NotFound, "Not found");
        }
        // Above lines of code have checked if exist but returnes Not found, but no
        // code has written to checkif result was already reported before creating new one
        // In receive command repository, There is Fetch method can be used to check an existing result
        // with that we can return Already Exists Status code.
        
        var existingResult = await _receivedCommandResultRepository.Fetch(_robotId, commandResult.CommandId);
        if(existingResult != null)
        {
            throw new ReturnStatusException(StatusCode.AlreadyExists, "Already exists");
        }

        await _receivedCommandResultRepository.Create(new ReceivedCommandResult()
        {
            CommandId = commandResult.CommandId,
            RobotId = _robotId,
            CommandResult = commandResult,
            ReceivedAt = DateTimeUtil.UtcNowMs,
        });

        // Task 3: Feature Development
        // TODO Auto-pilot should be notified of command result so it can determine what to do next
        // Here notify autopilot of command result so it can be dcide whatsnext?
        // //Code added for  Task 3: Feature Development :
        await _autoPilotManager.OnCOmmandResultRecieved(_robotId, commandResult);

        // if clear flag command success then resume autopilot
        if (commandResult is ClearFlagsCommandResult { Success: true })
            await _autoPilotManager.OnFlagsCleared(_robotId);
    }

    protected override async ValueTask OnConnected()
    {
        _robotId = EnsureRobotId();
        Console.WriteLine($"Connection received {ConnectionId}, robotId={_robotId}");
        await _connectedRobotCollection.OnConnected(_robotId, this.Client);
    }

    protected override async ValueTask OnDisconnected()
    {
        Console.WriteLine($"Disconnection received {ConnectionId}, robotId={_robotId}");
        await _connectedRobotCollection.OnDisconnected(_robotId);
    }

    public Task ReportFlag(Robot robot)
    {
        Console.WriteLine($"Flag received {ConnectionId}, robotId={_robotId}");

        // Task 3: Feature Development
        // TODO Auto-pilot should be aware of this and not send commands until flag is cleared
        //Code added for  Task 3: Feature Development : 
        // Pause the autopilot here when flag is reported
        _autoPilotManager.OnFlagReported(_robotId);
        return Task.CompletedTask;
    }
}
