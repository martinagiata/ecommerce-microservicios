using Dapper;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Notifications.Extensiones;
using Notifications.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<NotificationRepository>();

builder.Services.AddHealthChecks()
    .AddCheck("sqlite-db", () =>
    {
        try
        {
            using var conn = new SqliteConnection("Data Source=notifications.db");
            conn.Open();
            conn.ExecuteScalar<int>("SELECT 1");
            return HealthCheckResult.Healthy("SQLite OK");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("SQLite error", ex);
        }
    }, tags: ["database"]);

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Notifications", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

app.Services.GetRequiredService<NotificationRepository>().Initialize();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

app.MapNotificationEndpoints();

app.MapGet("/", () => "Notifications Service funcionando!");

app.Run();
