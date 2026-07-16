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

    builder.Services.AddScoped<DatabaseSeeder>();

    builder.Services.AddControllers();

    // Liveness endpoint (see MapHealthChecks below).
    builder.Services.AddHealthChecks();

    // OpenAPI document + Swagger UI (browsable at /swagger in Development).
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Seed demo accounts on startup. Idempotent, so it is safe to run every time.
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

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
catch (Exception ex)
{
    Log.Fatal(ex, "Flipcoin API terminated unexpectedly");
}
finally
{
    // Flush any buffered log events before the process exits.
    Log.CloseAndFlush();
}
