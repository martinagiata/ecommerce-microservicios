using Dapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using MiniApi.Extensions;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Serilog
        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration)
            .WriteTo.Console());

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Health Checks
        builder.Services.AddHealthChecks()
            .AddCheck("sqlite-db", () =>
            {
                try
                {
                    using var conn = new SqliteConnection("Data Source=app.db");
                    conn.Open();
                    conn.ExecuteScalar<int>("SELECT 1");

                    return HealthCheckResult.Healthy("SQLite OK");
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy("SQLite error", ex);
                }
            }, tags: ["database"]);

        // HealthChecks UI
        builder.Services.AddHealthChecksUI(setup =>
        {
            setup.SetEvaluationTimeInSeconds(600);
            setup.AddHealthCheckEndpoint("MiniApi", "/health");
        }).AddInMemoryStorage();

        var app = builder.Build();

        // Swagger UI
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseSerilogRequestLogging();
        app.UseHttpsRedirection();

        // Health endpoints
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecksUI(setup =>
        {
            setup.UIPath = "/health-ui";
        });

        // ENDPOINTS CRUD
        app.MapItemEndpoints();

        // Endpoint base
        app.MapGet("/", () => "MiniApi funcionando!");

        app.Run();
    }
}