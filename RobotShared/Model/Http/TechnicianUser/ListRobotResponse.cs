using RobotShared.Model.Command;

namespace RobotShared.Model.Http.TechnicianUser;

public class ListRobotResponse
{
    public List<FetchRobotResponse> Robots { get; set; }
}
