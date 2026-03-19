namespace RobotShared.Model.Http;

public class GetHeartBeatResponse
{
    public bool StillAlive { get; set; }

    /// <summary>
    /// The version of PostgreSQL running on the server.
    /// Don't ask why it is exposed publicly.
    /// </summary>
    public string PgVersion { get; set; }
}
