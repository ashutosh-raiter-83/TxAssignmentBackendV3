using Testcontainers.PostgreSql;

namespace LocalDevDependencies;

internal class Program
{
    public static async Task Main(string[] args)
    {
        // Hack to get the project root
        var d = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (d != null && d.Name is not "LocalDevDependencies")
        {
            d = d.Parent;
        }
        if (d == null)
        {
            throw new Exception($"Could not find LocalDevDependencies of {Directory.GetCurrentDirectory()}");
        }
        Console.WriteLine($"d.FullName={d.FullName}");

        var postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            // Just hardcode 7777 since we're just using this for local development and testing
            .WithPortBinding(7777, 5432)
            // SQL scripts in LocalDevDependencies/Sql/DataDefinition
            // will be executed to create the schema and seed data
            .WithResourceMapping(
                new DirectoryInfo(Path.Combine(d.FullName, "Sql", "DataDefinition")),
                "/docker-entrypoint-initdb.d"
            )
            .WithResourceMapping(
                new DirectoryInfo(Path.Combine(d.FullName, "Sql", "SeedData")),
                "/docker-entrypoint-initdb.d"
            )
            .Build();
        await postgreSqlContainer.StartAsync();
        var pgConnectionString = postgreSqlContainer.GetConnectionString();
        Console.WriteLine($"pgConnectionString={pgConnectionString}");
        do
        {
            Console.WriteLine("Type 'exit' to exit");
        } while (Console.ReadLine() is not "exit");
    }
}
