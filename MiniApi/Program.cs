
public partial class Program
{
    private static void Main(string[] args)
    {
var builder = WebApplication.CreateBuilder(args);

        // Swagger
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .WriteTo.Console());

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


app.Run();
    }
}