namespace Server.AutoPilot
{
    /// <summary>
    /// This enums would represent Current State of AutoPilot for the robot. 
    /// </summary>
    public enum AutoPilotState
    {
        /// <summary>
        /// When AutoPilot is not active. User can control robot. 
        /// </summary>
        Deactivated,

        /// <summary>
        /// AutoPilot is active and sends commands to robot, User commands would berejected with this state.
        /// </summary>
        Running,

        /// <summary>
        /// AutoPilot is active but paused since robot or bin is flagged.With this User can send commands for troubleshoot 
        /// Autopilot would resume when robt becomes unflagged.
        /// </summary>
        Paused,
    }

}
