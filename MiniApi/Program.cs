using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapHealthChecks("/health");
app.MapGet("/", () => "MiniApi funcionando!");

app.Run();
