using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Products.Endpoints;
using Products.Infrastructure;
using Products.Repositories;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
        .WriteTo.Console())
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le =>
        {
            if (le.Properties.TryGetValue("RequestPath", out var p))
            {
                var path = p.ToString().Trim('"');
                return !path.Contains("/health") && !path.Contains("/swagger");
            }
            return false;
        })
        .WriteTo.File(
            path: "logs/audit.log",
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {RequestMethod} | {RequestPath} | {StatusCode}{NewLine}",
            rollingInterval: RollingInterval.Day))
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Servicios
builder.Services.AddSingleton<DatabaseInitializer>();
builder.Services.AddSingleton<ProductRepository>();

builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>("sqlite-db", tags: ["database"]);

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("Products API", "/health");
}).AddInMemoryStorage();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<DatabaseInitializer>().Initialize();

// Middleware
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Health Checks
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

// Endpoints
app.MapProductEndpoints();

app.Run();