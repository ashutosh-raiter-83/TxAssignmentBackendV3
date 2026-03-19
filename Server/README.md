### Requirements

1. IDE of your choice
1. .NET 10 SDK
1. Docker
1. Ports 7777, 7778, 7779 available

---

### Setup and First Run

1. Open the project in Visual Studio 2026 (or IDE of your choice)
1. Select the "Local Development" Launch profile
   - See: `TxAssignmentBackendV3.slnLaunch.user`
1. Click "Start"
1. 3 projects should run
   - LocalDevDependencies: Starts test PostgreSQL instance in a Docker container
     - Creates the necessary schema and seed data, too
   - Server: Runs the server and connects to the test PostgreSQL instance
   - RobotClient: Logs in to Robot1 and Robot3, and runs RobotSimulator on them
1. Navigate to http://localhost:7779/swagger/index.html
1. Log in as a technician
   - Username: Technician1
   - Password: Technician1
1. Copy the `token` from the response
1. Navigate to http://localhost:7779/swagger/index.html?urls.primaryName=Technician-User+v1.0.0
1. Use the `token` from earlier to Authorize
1. Send this command to `Robot1`: `{"command": {"$type":"QueryRobotStateCommand"}}`
   - `POST http://localhost:7779/technician-user/robot/Robot1/command`
1. Check the received command result
   - `GET http://localhost:7779/technician-user/robot/Robot1/command/{commandId}/result/received`

You should now be ready to work on the assignment.

Reminder that there are 4 tasks.

1. Bug Fixes
   - Find All -> `Task 1`
1. Missing Unit/Integration Tests
   - Find All -> `Task 2`
1. Feature Development
   - You should spend the most time on this
   - Find All -> `Task 3` for some hints
1. Documentation
   - Save this for the end
   - Find All -> `Task 4` for additional questions

Rules for projects,
- IntegrationTest
  - Do not delete/modify existing test cases unless you absolutely have to.
  - Do not delete/modify existing code unless you absolutely have to.
  - Please add more test cases, if necessary.
- LocalDevDependencies
  - Do not modify this except to add more `.sql` files
- RobotClient
  - Do not modify this; unless you find a bug and need to fix it to complete the assignment.
  - Do not add new code.
- RobotShared
  - Do not delete/modify existing code unless you absolutely have to.
  - You may add new DTOs for Task 3 (Feature Development).
- RobotSimulator
  - Do not modify this; unless you find a bug and need to fix it to complete the assignment.
  - Do not add new code.
- Server
  - Do not delete/modify existing code unless you absolutely have to.
  - You may add new code for Task 3 (Feature Development).
- UnitTest
  - Do not delete/modify existing test cases unless you absolutely have to.
  - Do not delete/modify existing code unless you absolutely have to.
  - Please add more test cases, if necessary.

---

### Sample Commands and Results

The robot simulator will attempt to resolve any robot/bin flags when this command is sent.
```
{
  "command": {
    "$type":"ClearFlagsCommand"
  }
}

{
  "commandId": "8b906a00-03a5-43fc-ab22-368e07b546bd",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "ClearFlagsCommandResult",
    "commandId": "8b906a00-03a5-43fc-ab22-368e07b546bd",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:01:15.627Z"
}
```

---

```
{
  "command": {
    "$type":"QueryRobotStateCommand"
  }
}

{
  "commandId": "514ea166-772c-4885-bf8b-e90d185e8d52",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "QueryRobotStateCommandResult",
    "robot": {
      "zPosition": 0,
      "facingDirection": 0,
      "isHoldingItem": false,
      "flagReason": null,
      "flaggedUnsortedBins": [],
      "flaggedLabeledBins": [],
      "serverCausedFlagCount": 0
    },
    "commandId": "514ea166-772c-4885-bf8b-e90d185e8d52",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:02:13.578Z"
}
```

---


```
{
  "command": {
    "$type":"ScanEnvironmentCommand"
  }
}

{
  "commandId": "d4c762ed-bd4d-4ce3-b5b9-433c59a12882",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "ScanEnvironmentCommandResult",
    "robot": {
      "zPosition": 0,
      "facingDirection": 0,
      "isHoldingItem": false,
      "flagReason": null,
      "flaggedUnsortedBins": [],
      "flaggedLabeledBins": [],
      "serverCausedFlagCount": 0
    },
    "unsortedBinZPositions": [
      0.9352784,
      1.2993279,
      2.2125165,
      3.5223236,
      4.87834,
      5.739028,
      6.1912932
    ],
    "labeledBinZPositions": [
      0.9435745,
      1.4249105,
      2.9697573,
      3.3362012,
      4.704886,
      5.3155494
    ],
    "commandId": "d4c762ed-bd4d-4ce3-b5b9-433c59a12882",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:07:06.831Z"
}
```

---

```
{
  "command": {
    "$type":"MoveToZPositionCommand",
    "zPosition": 0.9435745
  }
}

{
  "commandId": "fd43e029-8b05-4744-9a33-aae7655ce2d3",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "MoveToZPositionCommandResult",
    "commandId": "fd43e029-8b05-4744-9a33-aae7655ce2d3",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:07:33.549Z"
}
```

---

```
{
  "command": {
    "$type":"FaceDirectionCommand",
    "facingDirection": 2
  }
}

{
  "commandId": "c8018334-232b-488c-b4ff-c3ef866dcdca",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "FaceDirectionCommandResult",
    "commandId": "c8018334-232b-488c-b4ff-c3ef866dcdca",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:07:53.856Z"
}
```

---

```
{
  "command": {
    "$type":"ScanLabeledBinCommand"
  }
}

{
  "commandId": "3730c88f-6a85-4136-b67b-417d2b40b8ea",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "ScanLabeledBinCommandResult",
    "label": "Canned Good",
    "fullness": 0,
    "commandId": "3730c88f-6a85-4136-b67b-417d2b40b8ea",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:08:13.376Z"
}
```

---

```
{
  "command": {
    "$type":"ScanUnsortedBinCommand"
  }
}

{
  "commandId": "47cbb779-be19-430c-b9d5-e99a387bb660",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "ScanUnsortedBinCommandResult",
    "topmostItemLabel": "Dairy",
    "commandId": "47cbb779-be19-430c-b9d5-e99a387bb660",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:09:42.082Z"
}
```

---

```
{
  "command": {
    "$type":"PickItemCommand"
  }
}

{
  "commandId": "c9fecd0a-1bc6-4d63-8837-56a50f64ff2d",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "PickItemCommandResult",
    "commandId": "c9fecd0a-1bc6-4d63-8837-56a50f64ff2d",
    "success": true,
    "failureReason": null
  },
  "receivedAt": "2025-11-27T11:10:06.561Z"
}
```

---

```
{
  "command": {
    "$type":"PlaceItemCommand"
  }
}

{
  "commandId": "65e1c732-633a-482d-991d-c4d0bf3c5860",
  "robotId": "Robot1",
  "commandResult": {
    "$type": "PlaceItemCommandResult",
    "commandId": "65e1c732-633a-482d-991d-c4d0bf3c5860",
    "success": false,
    "failureReason": "Attempted to place to incorrect bin"
  },
  "receivedAt": "2025-11-27T11:12:17.572Z"
}
```
