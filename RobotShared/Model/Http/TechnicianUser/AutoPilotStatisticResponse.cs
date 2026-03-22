namespace RobotShared.Model.Http.TechnicianUser
{
    public class AutoPilotStatisticResponse
    {
        /// <summary>   
        /// Current State of AutoPilot for the robot.
        ///</summary>
        public string? State { get; set; }
        /// <summary>   
        /// Total number of items sorted by AutoPilot successfully.
        ///</summary>
        public int ItemsSorted { get; set; }
        /// <summary>   
        /// Total number of commands sent by AutoPilot.
        ///</summary>
        public int CommandsSent { get; set; }
        /// <summary>   
        /// TotalNumber of errors occurred/encountered during AutoPilot.
        ///</summary>
        public int ErrorsEncountered { get; set; }

    }
}
