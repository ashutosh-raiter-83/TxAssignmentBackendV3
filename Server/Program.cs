using MagicOnion.Server;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Npgsql;
using RobotShared.Model.Command;
using RobotShared.Model.CommandResult;
using Server.Auth;
using Server.Hub;
using Server.Repository;
using System.Data;
using System.Reflection;
using Server.AutoPilot;
namespace Server;

public class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseKestrel();

        var authConfig = builder.Configuration.GetSection("AuthConfig").Get<AuthConfig>();

        builder.Services.AddSingleton<AuthConfig>(authConfig);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(authConfig.JwtSecret)),
                    RequireExpirationTime = true,
                    RequireSignedTokens = true,
                    ClockSkew = TimeSpan.FromSeconds(10),

                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
                options.RequireHttpsMetadata = false;
            });

        var pgConnectionString = builder.Configuration["PgConnectionString"];
        builder.Services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(pgConnectionString));
        builder.Services.AddSingleton<IConnectedRobotCollection, ConnectedRobotCollection>();

        builder.Services.AddScoped<IRobotRepository, RobotRepository>();
        builder.Services.AddScoped<ITechnicianRepository, TechnicianRepository>();
        builder.Services.AddScoped<ISentCommandRepository, SentCommandRepository>();
        builder.Services.AddScoped<IReceivedCommandResultRepository, ReceivedCommandResultRepository>();
        builder.Services.AddSingleton<IAutoPilotManager,AutoPilotManager>();
        builder.Services.AddMagicOnion(options =>
        {
            options.GlobalFilters.Add<RobotUserAuthorizationMagicOnionFilter>();
            //options.GlobalStreamingHubFilters.Add<RobotUserAuthorizationMagicOnionFilter>();
        });

        builder.Services.AddMvc(options =>
        {
            options.Conventions.Add(new Swagger.ApiExplorerGroupConvention());
        });

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            //options.CustomSchemaIds(x => x
            //    .ToString()
            //    .Replace(x.Namespace + ".", "")
            //    .Replace("`1[", "-")
            //    .Replace("]", "")
            //    .Replace("+", ".")
            //);

            options.UseOneOfForPolymorphism();
            options.SelectSubTypesUsing(baseType =>
            {
                if (baseType == typeof(CommandBase) || baseType == typeof(CommandResultBase))
                {
                    return baseType.Assembly.GetTypes()
                        .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract);
                }
                return [];
            });

            options.SwaggerDoc("top-level", new OpenApiInfo()
            {
                Title = "Top-Level - v1.0.0",
                Version = "v1.0.0",
                Description = "Endpoints not scoped to any particular user type",
            });
            options.SwaggerDoc("robot-user", new OpenApiInfo()
            {
                Title = "Robot-User - v1.0.0",
                Version = "v1.0.0",
                Description = "Endpoints for robot users",
            });
            options.SwaggerDoc("technician-user", new OpenApiInfo()
            {
                Title = "Technician-User - v1.0.0",
                Version = "v1.0.0",
                Description = "Endpoints for technician users",
            });

            options.SupportNonNullableReferenceTypes();

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Description = "Used for robot/technician users",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement()
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/top-level/swagger.json", "Top-Level v1.0.0");
            options.SwaggerEndpoint("/swagger/robot-user/swagger.json", "Robot-User v1.0.0");
            options.SwaggerEndpoint("/swagger/technician-user/swagger.json", "Technician-User v1.0.0");
        });

        app.MapMagicOnionService();
        app.MapControllers();
        app.Run();
    }
}
