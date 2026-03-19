namespace Server.Auth;

public class AuthConfig
{
    /// <summary>
    /// Expected to be base64 encoded
    /// </summary>
    public string JwtSecret { get; set; }
}
