using Grpc.Core;
using Grpc.Net.Client;
using MagicOnion.Client;
using RobotClient.Hub;
using RobotShared.Hub;
using RobotShared.Model.Http;
using System.Net.Http.Json;

namespace RobotClient;

internal class Program
{
    static async Task RunSimulator(HttpClient httpClient, string username, string password)
    {
        var logInResult = await httpClient.PostAsJsonAsync("/log-in/robot", new
        {
            Username = username,
            Password = password,
        });
        logInResult.EnsureSuccessStatusCode();
        var logInResponse = await logInResult.Content.ReadFromJsonAsync<LogInAsRobotResponse>();
        Console.WriteLine($"Logged in {username}");

        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        var channel = GrpcChannel.ForAddress("http://127.0.0.1:7778", new GrpcChannelOptions()
        {
            Credentials = ChannelCredentials.Insecure,
        });
        var receiver = new RobotHubReceiver();
        var client = await StreamingHubClient.ConnectAsync<IRobotHub, IRobotHubReceiver>(
            channel,
            receiver,
            option: new CallOptions(new Metadata()
            {
                { "authorization", $"Bearer {logInResponse.Token}" },
            })
        );

        // 5% chance of a random hardware failure
        new RobotSimulator.RobotSimulator(client, receiver, randomHardwareFaultChance: 0.05f);
        var helloWorldResult = await client.GetHelloWorld();
        Console.WriteLine($"{username} says: {helloWorldResult}");
    }

    static async Task Main(string[] args)
    {
        using var httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://127.0.0.1:7779"),
        };
        while (true)
        {
            try
            {
                await Task.Delay(1000);
                var heartbeatResult = await httpClient.GetAsync("/");
                heartbeatResult.EnsureSuccessStatusCode();
                break;
            }
            catch
            {
                Console.WriteLine("Server not ready");
            }
        }
        Console.WriteLine("Server ready, logging in");
        await RunSimulator(httpClient, "Robot1", "Robot1");
        await RunSimulator(httpClient, "Robot3", "Robot3");
        do
        {
            Console.WriteLine("Type 'exit' to exit");
        } while (Console.ReadLine() is not "exit");
    }
}
