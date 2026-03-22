using Dapper;
using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Npgsql;
using RobotShared.Hub;
using RobotShared.Model.Http;
using Server;
using Server.Auth;
using Server.Hub;
using Server.Repository;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Testcontainers.PostgreSql;
using Xunit;
using Server.AutoPilot;
namespace IntegrationTest;

public class WebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private static readonly PgContainer PgContainer = new PgContainer();

    public readonly Mock<IConnectedRobotCollection> ConnectedRobotCollection = new();

    public WebAppFactory()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            {
                var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IDbConnection));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.Add(new ServiceDescriptor(
                    typeof(IDbConnection),
                    (serviceProvider) => new NpgsqlConnection(PgContainer.GetConnectionString()),
                    ServiceLifetime.Scoped
                ));
            }

            {
                var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(IConnectedRobotCollection));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.Add(new ServiceDescriptor(
                    typeof(IConnectedRobotCollection),
                    (serviceProvider) => ConnectedRobotCollection.Object,
                    ServiceLifetime.Singleton
                ));
            }
        });
    }

    private static readonly object PgLock = new object();
    private static Task? PgStartAsyncTask = null;

    private static async Task PgStartAsync()
    {
        await PgContainer.StartAsync();
        await PgContainer.ExecuteSqlDataDefinition();
        await PgContainer.ExecuteSqlSeedData();
    }

    public Task InitializeAsync()
    {
        lock (PgLock)
        {
            if (PgStartAsyncTask == null)
            {
                PgStartAsyncTask = PgStartAsync();
            }
        }
        return PgStartAsyncTask;
    }

    public new Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private string JwtSecret => Server.Services.GetRequiredService<AuthConfig>().JwtSecret;

    public string GenerateToken(UserType userType, string userId, DateTime? expires = null)
    {
        return GenerateToken(JwtSecret, userType, userId, expires);
    }

    public static string GenerateToken(
        string jwtSecret,
        UserType userType,
        string userId,
        DateTime? expires = null
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
            Expires = expires ?? DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public JwtSecurityToken? TryParseToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(JwtSecret);
        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            return jwtToken;
        }
        catch
        {
            return null;
        }
    }

    public HttpClient CreateTechnicianClient()
    {
        var token = GenerateToken(UserType.Technician, "Technician1");
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    public HttpClient CreateRobotClient()
    {
        var token = GenerateToken(UserType.Robot, "Robot1");
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new("Bearer", token);
        return client;
    }

    public GrpcChannel CreateGrpcChannel()
    {
        return GrpcChannel.ForAddress(Server.BaseAddress, new GrpcChannelOptions()
        {
            HttpHandler = Server.CreateHandler(),
        });
    }

    public async Task<(IRobotHub, Mock<IRobotHubReceiver>)> CreateMagicOnionClient (string robotId)
    {
        var token = GenerateToken(UserType.Robot, robotId);
        var receiver = new Mock<IRobotHubReceiver>();
        var client = await StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
            CreateGrpcChannel(),
            receiver.Object,
            option: new CallOptions(new Metadata()
            {
                { "authorization", $"Bearer {token}" },
            })
        );
        return (client, receiver);
    }

    public Mock<IRobotHubReceiver> SetRobotOnline(string robotId)
    {
        var clientMock = new Mock<IRobotHubReceiver>();
        ConnectedRobotCollection
            .Setup(x => x.TryGetClient(robotId, out It.Ref<IRobotHubReceiver?>.IsAny))
            .Callback((string robotId, out IRobotHubReceiver? client) =>
            {
                client = clientMock.Object;
            })
            .Returns(true);
        return clientMock;
    }

    public void SetRobotOffline(string robotId)
    {
        ConnectedRobotCollection
            .Setup(x => x.TryGetClient(robotId, out It.Ref<IRobotHubReceiver?>.IsAny))
            .Callback((string robotId, out IRobotHubReceiver? client) =>
            {
                client = null;
            })
            .Returns(false);
    }

    public IDbConnection GetDbConnection()
    {
        return new NpgsqlConnection(PgContainer.GetConnectionString());
    }

    public RobotRepository GetRobotRepository()
    {
        return new RobotRepository(GetDbConnection());
    }

    public SentCommandRepository GetSentCommandRepository()
    {
        return new SentCommandRepository(GetDbConnection());
    }

    public ReceivedCommandResultRepository GetReceivedCommandResultRepository()
    {
        return new ReceivedCommandResultRepository(GetDbConnection());
    }
    public IAutoPilotManager GetAutoPilotManager()
    {
        return Server.Services.GetRequiredService<IAutoPilotManager>();
    }
}
