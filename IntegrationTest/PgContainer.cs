using Npgsql;
using Server.Repository;
using System.Data;
using Testcontainers.PostgreSql;

namespace IntegrationTest;

public class PgContainer : IAsyncDisposable
{
    private readonly PostgreSqlContainer _pgContainer = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase("postgres")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task StartAsync()
    {
        await _pgContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await _pgContainer.DisposeAsync();
    }

    public string GetConnectionString()
    {
        return _pgContainer.GetConnectionString();
    }

    public async Task ExecuteSqlDataDefinition()
    {

        foreach (var sqlFilePath in GetSqlDataDefinitionFilePaths())
        {
            Console.WriteLine($"Execute {sqlFilePath}");
            var sqlString = File.ReadAllText(sqlFilePath, System.Text.Encoding.UTF8);
            var execResult = await _pgContainer.ExecScriptAsync(sqlString);
            Console.WriteLine($"ExitCode: {execResult.ExitCode}");
            Console.WriteLine($"Stdout: {execResult.Stdout}");
            Console.WriteLine($"Stderr: {execResult.Stderr}");
            Console.WriteLine($"Execute {sqlFilePath} Done");
        }
    }

    public async Task ExecuteSqlSeedData()
    {

        foreach (var sqlFilePath in GetSqlSeedDataFilePaths())
        {
            Console.WriteLine($"Execute {sqlFilePath}");
            var sqlString = File.ReadAllText(sqlFilePath, System.Text.Encoding.UTF8);
            var execResult = await _pgContainer.ExecScriptAsync(sqlString);
            Console.WriteLine($"ExitCode: {execResult.ExitCode}");
            Console.WriteLine($"Stdout: {execResult.Stdout}");
            Console.WriteLine($"Stderr: {execResult.Stderr}");
            Console.WriteLine($"Execute {sqlFilePath} Done");
        }
    }

    public static string GetSqlDataDefinitionDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var index = currentDirectory.LastIndexOf("IntegrationTest");
        var testRoot = currentDirectory.Substring(0, index + "IntegrationTest".Length);
        var result = $"{testRoot}/../LocalDevDependencies/Sql/DataDefinition";

        if (Directory.Exists(result))
        {
            return result;
        }

        throw new NotImplementedException($"Getting Sql Data Definition directory not implemented for current directory: {currentDirectory}");
    }

    public static string GetSqlSeedDataDirectory()
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var index = currentDirectory.LastIndexOf("IntegrationTest");
        var testRoot = currentDirectory.Substring(0, index + "IntegrationTest".Length);
        var result = $"{testRoot}/../LocalDevDependencies/Sql/SeedData";

        if (Directory.Exists(result))
        {
            return result;
        }

        throw new NotImplementedException($"Getting Sql Seed Data directory not implemented for current directory: {currentDirectory}");
    }

    public static string[] GetSqlDataDefinitionFilePaths()
    {
        return Directory.GetFiles(GetSqlDataDefinitionDirectory(), "*.sql")
            .OrderBy(path => path)
            .ToArray();
    }

    public static string[] GetSqlSeedDataFilePaths()
    {
        return Directory.GetFiles(GetSqlSeedDataDirectory(), "*.sql")
            .OrderBy(path => path)
            .ToArray();
    }

    public IDbConnection GetDbConnection()
    {
        return new NpgsqlConnection(GetConnectionString());
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
}
