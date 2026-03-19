# **Coding Assignment: Telexistence Remote Monitoring and Control API**

## **Problem Statement:**

Telexistence provides remote operation services using robots.
In this assignment, the robot's purpose is to sort items into the correct bins.

1. Bins of unsorted items are placed on the robot's left side.
2. Labeled bins are placed on the robot's right side.
3. Each item in the unsorted bin is labeled.
   So, the robot knows where each item is supposed to go.
4. The robot must pick items from the unsorted bins, and place them in the correct labeled bins.
5. When an unsorted bin is emptied, the robot must flag it.
   It will then be removed; a new unsorted bin will replace it.
6. When a labeled bin is full, the robot must flag it.
   It will then be replaced by a new empty bin with a potentially different label.
7. When the robot encounters an error, the robot must flag itself.
   When a human fixes the problem (e.g. fixing hardware or environmental issues), the human will unflag the robot.
8. The system must report statistics regarding the number of items sorted, operations performed, error rate, etc.
9. When sent multiple commands, the robot will attempt to execute them in the order received.

The goal is to build an API that will allow users to interact with a robot for remote monitoring and control. This API will need to:

1. Accept commands from users
   - Examples of commands,
     - Move to unsorted bin X
     - Move to labeled bin Y
     - Scan item
     - Pick item
     - Place item
     - etc.
2. Expose the current state of robots
   - Examples of states,
     - Position
     - IsGrabbingItem
     - IsFlagged
     - etc.
3. Expose the current state of bins
   - Examples of states,
     - Position
     - Fullness (Empty/Partially Filled/Filled)
     - IsFlagged
     - etc.
4. Support user authentication and authorization.

Additionally, it should be set up for future extensibility to integrate with frontend teams for real-time communication and a logging dashboard.

> Note:
> We have the evaluation criteria that generally describes what we look for, but it is intentionally not too detailed so we can let you determine what is best (also a kind of design aspect to the challenge)

## **Provided Material:**

A .NET 10 solution should have been provided with this document.
It contains,

1. Boilerplate for some API endpoints.
2. Code for some features.
3. Bugs in some features.
4. Unit/integration tests. Some pass. Some fail.

## **Requirements:**

### **1. Bug Fixes**

The solution comes with some unit/integration tests.
Some of them are failing, and indicate bugs with existing features.

1. Fix as many as you reasonably can.
2. Write as many inline comments as you can when implementing your fix.
   This will help us understand your thought process.

Do not delete/modify existing test cases unless you absolutely have to.

### **2. Missing Unit/Integration Tests**

Some existing features have no unit/integration tests, or have insufficient test cases.
Missing tests/test cases may hide bugs with existing features.

1. Add as many new tests/test cases as you reasonably can, for the existing features.
2. If your new test/test cases reveal bugs, fix them.

Do not delete/modify existing test cases unless you absolutely have to.

### **3. Feature Development**

Implement an auto-pilot mode for the robot.
Features developed must have the appropriate unit/integration tests.

Functional requirements:
1. When auto-pilot mode is deactivated, the server must not send commands to the robot
   - Users can control the robot
   - Users can activate auto-pilot
2. When auto-pilot mode is activated and running, the server takes over and sends commands to the robot
   - Any commands to control the robot sent by users must be ignored while auto-pilot mode is activated and running
   - Users can only deactivate auto-pilot mode at this time
   - The server must ensure the robot continues to sort items indefinitely
3. When the robot is flagged, or any bin is flagged, auto-pilot mode must be paused and the server must not send commands to the robot
   - Users may now send commands to control the robot for troubleshooting purposes
   - Users may also deactivate auto-pilot mode
4. When the robot becomes unflagged, auto-pilot must resume running

API requirements:
1. Endpoints to activate/deactivate auto-pilot mode
2. Endpoints to read the auto-pilot state (activated/running/etc.)
3. (Optional) Endpoints to read auto-pilot statistics (items sorted/bins filled/bins emptied/errors encountered/etc.)

### **4. Documentation**

Documentation requirements:
1. Which endpoints did you implement, what do they do, and how are they meant to be used by frontend teams?
2. How does your implementation of the auto-pilot mode work?
   - What edge cases did you cover?
   - What edge cases did you not cover?
3. How would you improve the implementation in future, if given enough time?
4. How would you improve the overall codebase, if given enough time?
5. Other questions for this task may be found in code comments. Look for `Task 4` and answer them, too.

# **What to Return Back to Us**

1. **Source Code**: Host your code as git project and put it on GitHub.
   1. Invite `system-application@tx-inc.com` as a collaborator
2. **Deliverable Version**: Ensure that the application you've developed is functional.
3. **Run & Test Instructions**: Provide comprehensive documentation on how to run and test your application locally. This should ideally include any setup steps, dependencies, and configurations.
4. **README File**: Create a README file that encapsulates:
   - Answers to the questions provided in the challenge.
   - Technical decisions or requirements that you wish to highlight or explain.
   - A breakdown of the time spent on various tasks, as this helps us understand your workflow and approach.

For instance:

```
Read documents: 30mins
Make test: 2 hours
Implement: 4 hours
Refactor: 1 hour
...
```

Please note that reporting the time spent isn't a grading criterion. Instead, it assists us in gauging the challenge's difficulty level, especially when correlated with git commit timestamps.

---

## **Evaluation Criteria:**

- **Backend Skills**: Clean, modular, and well-structured code. Adherence to REST principles and security best practices (e.g., JWT for authentication).
- **Testing**: Coverage of key parts of the application with unit/integration tests.
- **Extensibility**: Code design that is scalable and easy to extend by other teams, including the frontend team.

This assignment would provide a solid foundation for evaluating backend skills in the context of a real-life Telexistence use case. You can easily extend this for other teams, such as frontend developers, by sharing the API endpoints and providing the necessary documentation.
Also your code should be original and not copied from another source, and you should only utilize necessary packages.
