using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using Server.Hub;
using Server.Repository;
using Server.Util;
namespace Server.AutoPilot
{
    public interface IAutoPilotManager
    {
        /// <summary>
        /// Activate AutoPilot for the robot. Return false if robot is not online.
        ///</summary>
        Task<bool> ActivateAutoPilot(string robotId);
        /// <summary>
        /// DeActivate AutoPilot for the robot.
        ///</summary>
        void DeActivateAutoPilot(string robotId);
        /// <summary>
        /// Get current State of AutoPilot for the robot.
        ///</summary>
        AutoPilotState GetState(string robotId);
        /// <summary>
        /// Get AutoPilot statistics for the robot.
        ///</summary>
        AutoPilotSession? GetSession(string robotId);
        /// <summary>
        /// Return true if AutoPilot is active for the robot, otherwise false.
        ///</summary>
        bool IsRunning(string robotId);
        /// <summary>
        /// Called whe co,,and resuld rcdv;d from robot
        /// If autopilot is running, determins and snds next command
        ///</summary>
        Task OnCOmmandResultRecieved(string robotId, CommandResultBase result);
        /// <summary>
        /// Called when a flag is reported by robot, Pauses autopilot if its running.
        ///</summary>
        void OnFlagReported(string robotId);
        /// <summary>
        /// Calledwhen flag are cleared by robot.
        ///</summary>
        Task OnFlagsCleared(string robotId);
    }

    public class AutoPilotManager : IAutoPilotManager
    {
        private readonly ConcurrentDictionary<string, AutoPilotSession> _autoPilotSessions = new();
        private readonly IConnectedRobotCollection _connectedRobotCollection;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public AutoPilotManager(IConnectedRobotCollection connectedRobotCollection, IServiceScopeFactory serviceScopeFactory)
        {
            _connectedRobotCollection = connectedRobotCollection;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<bool> ActivateAutoPilot(string robotId)
        {
            if (!_connectedRobotCollection.TryGetClient(robotId, out _)) 
                return false;

            var session = new AutoPilotSession();
            _autoPilotSessions[robotId] = session;

            await SendNextCommand(robotId,session);
            return true;
        }

        public void DeActivateAutoPilot(string robotId)
        {
            if(_autoPilotSessions.TryRemove(robotId,out var session))
                session.DeActivate();
        }

        public AutoPilotSession? GetSession(string robotId)
        {
            _autoPilotSessions.TryGetValue(robotId, out var session);

            return session;
        }

        public AutoPilotState GetState(string robotId)
        {
            if (_autoPilotSessions.TryGetValue(robotId, out var session))
                return session.State;

            return AutoPilotState.Deactivated;
            
        }

        public bool IsRunning(string robotId)
        {
            return _autoPilotSessions.TryGetValue(robotId,out var session) && session.State ==AutoPilotState.Running;
        }

        public async Task OnCOmmandResultRecieved(string robotId, CommandResultBase result)
        {
            if (!_autoPilotSessions.TryGetValue(robotId, out var session))
                return;

            if (session.State != AutoPilotState.Running)
                return;
            
            var toContinue = session.ProcessResult(result);
            if (toContinue)
                await SendNextCommand(robotId, session);
        }

        public void OnFlagReported(string robotId)
        {
            if (_autoPilotSessions.TryGetValue(robotId, out var session))
                session?.Pause();
        }

        public async Task OnFlagsCleared(string robotId)
        {
            if (_autoPilotSessions.TryGetValue(robotId, out var session))
            {
                session?.Resume();
                if(session?.State == AutoPilotState.Running)
                    await SendNextCommand(robotId, session);
            }
        }

        private async Task SendNextCommand(string robotId,AutoPilotSession session)
        {
            var command = session.GetNextCommand();
            if (command == null)
                return;

            if (!_connectedRobotCollection.TryGetClient(robotId, out var client))
                return;

            var sentCOmmand = new SentCommand
            {
                CommandId = Guid.NewGuid().ToString(),
                RobotId =robotId,
                Command =command,
                SentAt = DateTimeUtil.UtcNowMs
            };

            using var scope = _serviceScopeFactory.CreateScope();
            var sentCommandRepo = scope.ServiceProvider.GetRequiredService<ISentCommandRepository>();

            await sentCommandRepo.Create(sentCOmmand);
            await client.OnCommandReceived(sentCOmmand);
        }
    }
}
