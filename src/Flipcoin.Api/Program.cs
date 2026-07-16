using Flipcoin.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // One structured log line per HTTP request (method, path, status, elapsed).
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Starting Flipcoin API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Flipcoin API terminated unexpectedly");
}
finally
{
    // Flush any buffered log events before the process exits.
    Log.CloseAndFlush();
}
