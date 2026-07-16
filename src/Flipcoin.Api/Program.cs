using Flipcoin.Api.HostedServices;
using Flipcoin.Application;
using Flipcoin.Infrastructure;
using Serilog;
using Serilog.Events;

// Bootstrap logger: active before the host is built, so any failure during
// startup (bad config, DB wiring, etc.) is still written to the console.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace the default logging providers with Serilog. This fuller
    // configuration takes over from the bootstrap logger once the host is built.
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Add services to the container.

    var connectionString = builder.Configuration.GetConnectionString("Postgres")
        ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");
    builder.Services.AddInfrastructure(connectionString);
    builder.Services.AddApplication();

    // Seeds demo accounts on host start (idempotent). See the hosted service for
    // why this is not done inline between Build() and Run().
    builder.Services.AddHostedService<DatabaseSeederHostedService>();

    builder.Services.AddControllers();

    // Liveness endpoint (see MapHealthChecks below).
    builder.Services.AddHealthChecks();

    // OpenAPI document + Swagger UI (browsable at /swagger in Development).
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // One structured log line per HTTP request (method, path, status, elapsed).
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Starting Flipcoin API");
    app.Run();
}
// HostAbortedException is thrown by EF Core's design-time tooling to stop the
// host after building it; it is expected, not a crash, so let it pass through.
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Flipcoin API terminated unexpectedly");
}
finally
{
    // Flush any buffered log events before the process exits.
    Log.CloseAndFlush();
}
