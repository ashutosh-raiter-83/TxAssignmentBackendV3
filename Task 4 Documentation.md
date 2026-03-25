Documentation requirements:

1.1 Which endpoints did you implement? and what do they do?

New endpoints :
Activate : POST /technician-user/robot/Robot1/autopilot/activate
Request Body - None
Expected Response if Robot exsits and online
current state if already active e.g Paused
ex {"state": "Running"} after activation with statuscode 200 OK
Expected Response if Robot not exists
statuscode 404 Not Found
Expected Response if Robot exists but not online
status code 422 Unprocessable Entity 
ex 	"detail": "Robot is not online,try again"
	"title": "Unprocessable Entity",
   	 "status": 422	


Deactivate : POST /technician-user/robot/Robot1/autopilot/deactivate
request - None
Expected Response {"state": "Deactivated"} with statuscode 200 OK
Expected Response if Robot not exists
statuscode 404 Not Found
ex
{
    "state": "Deactivated"
}

State - GET /technician-user/robot/Robot1/autopilot/state
Request Body - Not required
Auth - Technician user token required
Expected Response 
{"state": "<Value>"} 
with statuscode 200 OK
Values can be 'Deactivate' OR 'Running' OR 'Paused'
ex 
{
    "state": "Deactivated"
}

Statistics - GET /technician-user/robot/{robotId}/autopilot/statistics
Request Body - Not required
Auth - Technician user token required
Expected Response  with statuscode 200 OK
{
    "state": "<state value>",
    "itemsSorted": 0,
    "commandsSent": 0,
    "errorsEncountered": 0
}
state value can be 'Deactivate' OR 'Running' OR 'Paused'
ex
{
    "state": "Deactivated",
    "itemsSorted": 0,
    "commandsSent": 0,
    "errorsEncountered": 0
}


-----------------------------------------------------------------------------------------------------------------------
1.2 How are they meant to be used by frontend teams?
For Front end team to use first 
1. Login Flow : 
	Call POST/log-in/technician
	Token would be generated, copy it and use subsequent request to be executed.
2. Call to GET /technician-user/robot  - This would list of robots -Response would tell about which robots are online.
3. With Manual operation
	Call GET /technician-user/robot/{robotId}/command with respective desired $type like ScanEnvironmentCommand, QueryRobotStateCommand, MoveToZPositionCommand, 		FaceDirectionCommand with faceDirection as UnsortedBin, ScanUnsortedBinCommand, ScanLabeledBinCommand, PickItemCommand, PlaceItemCommand, ClearFlagsCommand.
	
	Then checkout the received commands GET /technician-user/robot/{robotId}/command/result/received to see the results of commandsend execution
4. With AutoPilot operation
	Call POST /technician-user/robot/{robotId}/autopilot/activate 
	Then Check GET /technician-user/robot/{robotId}/autopilot/statistics for progress 
	keep checking state in case if it "Paused" then show warning, Need to execute command "ClearFlag" manually else wait for operator to takeaction.
	Call POST /technician-user/robot/{robotId}/autopilot/deactivate to deactivate/stop.
5. If auto-pilot mode is activated and running and front end sends manaul command, then server should response with HttpsStatus code as 409 'Conflict'.Based on this
	scenario front end should check autopilot state and show message that "Autopilot is in running state, deactivate before sending commands" which is added as 
logic in Send Action of CommandController.cs
-----------------------------------------------------------------------------------------------------------------------
2. How does your implementation of the auto-pilot mode work?
Front End Team  -------> AutoPilotController(activate/deactivate/state/statistics) ------> Singleton AutoPilotManager(ConcurrentDictionary<string, AutoPilotSession>)
---> AutoPilotSession (SortingSets, AutoPilotState, unsortedBinPosition,lebeledBinPosition, heldItemlebel, Dictionary<float, string> lebeledBinLebles, Dictionary<float, Fullness> lebeledBinFullnes,HashSet<float> scannedLebeledBin, CommandBase? GetNextCommand(),ProcessResult(CommandResultBase commandResult)) --->
gRPC using IConnectedRobotCollection -----------> Robot

Responsibilites:
AutoPilotController - With Scoped DI, Its per request - Exposes activate/deactivate/state/statistics endpoints to be operated by Front end team.

AutoPilotManager - With Singleton DI, Manages sessions per robot using ConcurrentDictionary<string, AutoPilotSession>.
		   Manages coordination between controller, gRPC hub and each session.
		   Making use of IServiceScopeFactory to reolve the DI scoped ISentCommandRepository while persisting commands.

AutoPilotSession - Works as State Machine, Tracks the 
			State : Deactivated or Running Or Paused
			SortingSets :  ScanEnvironment/MoveToUnsortedBin/FaceUnsortedBin/ScanUnsortedBin/PickItem/
				       MoveToLabeledBin/FaceLabeledBin/ScanLabeledBin/PlaceItem
			Statistics 
RobotHub  - 	Works Per connection basis
		On any of the event command result received + advance sorting - notify to AutoPilotManager, 
		Flage reported : pause,flag cleared, resume

CommandController - With Scoped DI, checks if autopilot is running then returns 409 Conflict -> This prevents from manual commands interference.


Algorithm used for Sorting:
Implemented Pre scan strategy-
1. ScanEnvironment to discover all bin position (unsortedBinPosition = [] and lebeledBinPosition = [])
2. Pre scan lebeled bins before toucing any item, For each unsorted bin - move toit then face it -topmost item, cache lebel and fullness per Z position
3. Sort Loop for each unsorted Bin:
	- MovetoUnsortedBin then FaceLebeledBin then ScanUnsortedBin
	- If bin is empty then advance to next unsorted bin
	- Pick Item
	- Find Matching LebeledBin from cache - lebelmatches,not full
	- MoveToLebeledBin then FaceLebeledBin then PlaceItem
	- ItemSorted++; go back to 1st(MovetoUnsortedBin then FaceLebeledBin then ScanUnsortedBin) for next item in same bin
4.All unsorted bins empty then loop back to step1(ScanEnvironment to discover...... which is rescan environment) 

-----------------------------------------------------------------------------------------------------------------------
2.1 What edge cases did you cover?
A] When the robot goes offline and there is no active pilot : The system tries to send the next command.The function SendNextCommand calls TryGetClient() to check if the robot is still connected.If the robot is not connected, the command is just ignored (not sent).The session stays alive even though the robot is offline. 
This means the system keeps the session open, but it doesn’t collect statistics or make any progress.Later, when a Deactivate command is issued, the session is properly closed and removed in a clean way.If the robot disconnects, commands stop being sent but the session remains open until someone deactivates it, which then cleans everything up.
B] When a flag is reported during sorting : The autopilot immediately pauses.It sets a PausedFlag, which means no more commands are sent while the flag is active.
If the ClearFlagCommand succeeds, the autopilot resumes operation. When it resumes, it starts again from ScanEnvironment. At this point, all cached bin data is cleared, so the system doesn’t rely on old or possibly incorrect information. The algorithm then begins fresh, ensuring it doesn’t act on stale environment data.
A flag stops the autopilot right away. Once cleared, the system restarts cleanly, wiping old data to avoid mistakes.
C] When Command fails  due to a random hardware fault that happens about 5% of the time : When any command returns an unsuccessful result (called CommandResultBase), the system notes this as an error and increases the error counter. After this, the sorting set resets back to ScanEnvironment. This reset is a safe recovery strategy: instead of continuing with possibly wrong assumptions, the robot re‑discovers the environment from scratch. By doing this, the robot avoids acting on incorrect or outdated information and ensures it is working with the true current state.
 If a command fails, the robot doesn’t risk making mistakes. It resets, scans the environment again, and safely continues from a clean state.
D] When the robot scans an unsorted bin and finds it empty : meaning the ScanUnsortedBinCommand does not return any topmost item, The system does not try to pick up an item from that bin.Instead the session moves on to the next unsorted bin automatically.This prevents wasted effort and avoids errors from trying to grab something that isn’t there.If a bin is empty, the robot skips it and continues with the next one, keeping the process smooth and efficient.
E] All Labeled Bins Full or No Matching Label When the robot is holding an item, it looks for a labeled bin that matches the item’s label.If no cached labeled bin matches, the algorithm doesn’t give up immediately.Instead, it falls back to scanning any labeled bins that haven’t been scanned yet.If it scans all labeled bins and still finds no match, the system resets back to ScanEnvironment. At this point, the simulator will usually flag the situation anyway, since there’s nowhere suitable to place the item.
 If the robot can’t find a matching bin, it tries scanning all labeled bins. If still no match, it resets and lets the simulator highlight the problem.
Activate When Already Active (Idempotentency scenario):  If you call Activate while the session is already Running or Paused,- The system does not create a new session. It also does not send duplicate commands. Instead, it simply returns the current state of the session.
Calling Activate again when the session is already active is harmless. It just confirms the current status without restarting or duplicating anything.
F] Deactivate When Already Deactivated (Idempotency scenario) : If you call Deactivate while the session is already deactivated,The system does not throw an error.
It simply returns the state as Deactivated. No duplicate actions or side effects occur.
Calling Deactivate again when the session is already off is safe. It just confirms the session is deactivated, without causing problems.

G] Manual Command During Autopilot (Conflict Prevention): If a technician tries to send a manual command while the autopilot is running, The system checks the status using IsRunning. If it sees that autopilot is active, it does not allow the manual command. Instead, it returns a 409 Conflict response.
This prevents the technician from interfering with the ongoing automated sorting process.
When autopilot is running, manual commands are blocked with a conflict message, ensuring the robot stays on its automated path without disruption.

H] Concurrent Access (Safe Multi‑Robot Sessions):	The AutopilotManager uses a ConcurrentDictionary. This means multiple robots can each have their own independent sessions at the same time. Even if different events happen at once (for example, a flag is reported while a result is received), The dictionary stays safe and does not get corrupted. 	This design ensures that concurrent requests are handled smoothly without interfering with each other.
The system is built to handle multiple robots and overlapping events safely, thanks to the use of a concurrent data structure.

I] Resume After Flag Cleared :  When the autopilot is resumed after a flag has been cleared, The system resets all cached data.This includes:
	- Cached labeled bin labels
	- Cached fullness data (which bins were full or not)
	- Cached pre‑scan state
	- By clearing this information, the robot avoids acting on stale or outdated data.
	- This is important because a human operator may have rearranged bins while the robot was paused.
	- Starting fresh ensures the robot works with the current, correct environment.
In short: After a pause, the robot wipes its memory of bin states and starts scanning again, so it doesn’t make mistakes based on old information.

-----------------------------------------------------------------------------------------------------------------------
2.2 What edge cases did you not cover?
A] If a bin fills up after the initial scan, the robot doesn’t notice until it tries to place an item there. The simulator blocks the action, forcing a full re‑scan, which is safe but inefficient.
B] The session survives a disconnect, but commands don’t resume automatically. A technician has to restart the session by deactivating and reactivating.
C] Multiple Items With Same Label robot picks items one bin at a time and always uses the first matching labeled bin, without worrying about efficiency or balance.The algorithm works on one unsorted bin at a time.The robot’s control system is safe and predictable, but not optimized for efficiency. 
D] The robot only knows about bins that were present during the last ScanEnvironment phase. If new bins are added while sorting is already in progress, the robot does not detect them immediately.Once the environment is scanned again, the new bins are discovered and included in the sorting process.
E] Sessions exist only in memory. A server restart wipes them out completely since there’s no database to preserve them. If the server restarts, all active sessions are lost immediately. This means there is no database backend to save or persist sessions.
F] If a robot gets stuck, autopilot never times out or recovers on its own. It waits endlessly, so human intervention is required.if a robot stops responding, autopilot cannot detect or recover it requires manual action. When a new command is issued, it is first saved into the database. After saving, the command is then sent to the robot via gRPC.If the gRPC send fails (for example, the robot just disconnected),The database will still contain a record of the command, even though the robot never actually received it.There is no rollback mechanism to remove or undo that record.

-----------------------------------------------------------------------------------------------------------------------
3. How would you improve the implementation in future, if given enough time?
A] Command Timeout : - Right now, if a robot stops responding, the autopilot waits forever for a result. To fix this, a timer session can be added.If no result arrives within N seconds, the system can: a) Resend the last command, or b) Reset to ScanEnvironment to start fresh. c) This prevents the autopilot from being blocked indefinitely when a robot gets stuck or fails to reply.
B] Persistent Sessions : By saving sessions in a database or cache, the system can survive server restarts and continue where it left off. If the server restarts, all active sessions are lost because there is no persistence layer.  To fix this, sessions can be stored in a database or in a distributed cache (like Redis).On server restart, the system could then reload the saved sessions.
C] Reconnection :  When a robot disconnects, its autopilot session stays alive in memory.If the robot later reconnects, the system can detect this through RobotHub.OnConnectedAsync. If the reconnecting robot already has an active session, the system should automatically resume sending commands. This avoids the need for a technician to manually deactivate and re‑activate the session.
D] Retry Logic required: For temporary gRPC failures, the system retries a few times with increasing wait times before giving up, making recovery smoother without flooding the robot. Sometimes sending a command via gRPC fails due to a temporary issue (like a brief network glitch). Instead of giving up immediately, the system can retry sending the command.

-----------------------------------------------------------------------------------------------------------------------
4. How would you improve the overall codebase, if given enough time?
A] Input request validation : Right now, there is no validation on incoming request bodies (for example, SendCommandRequest or LogOnAsRobotRequest). By adding FluentValidation or DataAnnotations, the system could automatically check requests before they reach business logic. This would ensure only well‑formed, valid requests are processed.
B] Consistent Error Handling Middleware : Right now, each controller in the system manually returns error responses like 'Problem', 'NotFound', etc. A better approach is to use a global exception filter or middleware. Instead of each controller handling errors separately, a global middleware can catch them all and return consistent, clean error responses automatically.
C] Structured Logging : By adding ILogger<T> throughout these components, the system can log key events in a structured way.Using this logs we can capture events such as CommandSent, ResultReceveied, State transition or if any error encountered. Using a structured logging framework like Serilog makes logs machine‑readable and easy to query.- This would improve observability.
D]API versioning : can add [ApiVersion] attributes for fowward comapatibility when API's evolves.
E] JWT Token refresh : currently JWTtoken expires in 60 mins with no refresh mechanism. Add a refresh token flow for long running frontend sesions.
F] Docker: A docker-compose.yml with Postfgrsql + the server would besimpler for freont end developers.
G] Pagination: List endpoints are hardcoded with 100 LIMIT results, Adding cursor based pagination and increase in limit would add better performance.
H] Reposirory : repositories use IDbCOnnectionwith Daper.With Unit of work pattern would allow transactional consistency across mutliple repository calls.
I] HealthCheck : Can use APS.Net Core built in health check framework like AddHealthCheck, MapHealthCheck 
-----------------------------------------------------------------------------------------------------------------------