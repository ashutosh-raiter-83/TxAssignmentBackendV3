using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RobotShared.Model.Http;
using Server.Auth;
using Server.Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Server.Controller;

[ApiController]
[Route("log-in")]
public class LogInController : ControllerBase
{
    private readonly AuthConfig _authConfig;
    private readonly IRobotRepository _robotRepository;
    private readonly ITechnicianRepository _technicianRepository;

    public LogInController(
        AuthConfig authConfig,
        IRobotRepository robotRepository,
        ITechnicianRepository technicianRepository
    )
    {
        _authConfig = authConfig;
        _robotRepository = robotRepository;
        _technicianRepository = technicianRepository;
    }

    private static string GenerateToken(
        string jwtSecret,
        UserType userType,
        string userId
    )
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(jwtSecret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] {
                    new Claim("userType", userType.ToString()),
                    new Claim("userId", userId),
                }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Logs in as a robot
    /// Task 1 : Bug Fix - Missing Password Verification
    /// Never updated the LoginAt timestamp here in DB, hence it was always remaining NULL
    /// Fix added to set the LoginAt timestamp to current time.
    /// Task 2: Missing Unit/Integration Tests
    /// Password verification looks missing here which would lead to secutiry Issue.
    /// Fix added to verify the password before authorizing the robot.
    /// </summary>
    [HttpPost("robot", Name = "LogInAsRobot")]
    [ProducesResponseType(typeof(LogInAsRobotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogInAsRobot([FromBody] LogInAsRobotRequest request)
    {
        var robot = await _robotRepository.FetchAuthCredential(request.Username);
        
        if (robot == null || robot.Password != request.Password)
        {
            return Unauthorized();
        }
        
        await _robotRepository.SetLastLogInAtNow(robot.RobotId);
        return Ok(new LogInAsRobotResponse
        {
            Token = GenerateToken(
                _authConfig.JwtSecret,
                UserType.Robot,
                robot.RobotId
            ),
        });
    }

    /// <summary>
    /// Logs in as a technician
    /// Task 2: Missing Unit/Integration Tests
    /// Password verification looks missing here which would lead to secutiry Issue.
    /// Fix added to verify the password before authorizing the robot.
    /// </summary>
    [HttpPost("technician", Name = "LogInAsTechnician")]
    [ProducesResponseType(typeof(LogInAsTechnicianResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogInAsTechnician([FromBody] LogInAsTechnicianRequest request)
    {
        var technician = await _technicianRepository.FetchAuthCredential(request.Username);
        if (technician == null || technician.Password != request.Password)
        {
            return Unauthorized();
        }

        return Ok(new LogInAsTechnicianResponse
        {
            Token = GenerateToken(
                _authConfig.JwtSecret,
                UserType.Technician,
                technician.TechnicianId
            ),
        });
    }
}
