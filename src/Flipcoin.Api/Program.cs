using Flipcoin.Api.Extensions;
using Flipcoin.Api.ExceptionHandling;
using Flipcoin.Api.HostedServices;
using Flipcoin.Api.Validation;
using Flipcoin.Application;
using Flipcoin.Infrastructure;
using FluentValidation;
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
    builder.Services.AddJwtAuthentication(builder.Configuration);

    // Seeds demo accounts on host start (idempotent). See the hosted service for
    // why this is not done inline between Build() and Run().
    builder.Services.AddHostedService<DatabaseSeederHostedService>();

    builder.Services.AddValidatorsFromAssemblyContaining<Program>();

    // Allow the Blazor WASM client (a different origin) to call the API.
    const string clientCorsPolicy = "ClientApp";
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? Array.Empty<string>();
    builder.Services.AddCors(options =>
        options.AddPolicy(clientCorsPolicy, policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()));

    builder.Services
        .AddControllers(options => options.Filters.Add<ValidationFilter>())
        .AddJsonOptions(options =>
            // Bind and serialize enums by name (e.g. "Heads", "TransferOut")
            // rather than by their numeric value.
            options.JsonSerializerOptions.Converters.Add(
                new System.Text.Json.Serialization.JsonStringEnumConverter()));

    // Central exception -> ProblemDetails translation (see GlobalExceptionHandler).
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Liveness endpoint (see MapHealthChecks below).
    builder.Services.AddHealthChecks();

    // OpenAPI document + Swagger UI (browsable at /swagger in Development).
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Translate unhandled exceptions into ProblemDetails responses.
    app.UseExceptionHandler();

    // One structured log line per HTTP request (method, path, status, elapsed).
    app.UseSerilogRequestLogging();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Not in Development: the WASM client is served over http and calls the API
    // over http, and redirecting a cross-origin request to https breaks the
    // browser fetch/CORS. In production TLS is terminated up front (reverse proxy).
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors(clientCorsPolicy);

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    Log.Information("Starting Flipcoin API");
    app.Run();
}
// Let host-stopping signals pass through rather than reporting them as crashes:
// HostAbortedException (EF Core design-time tooling) and StopTheHostException
// (WebApplicationFactory's test host, thrown after the host is built).
catch (Exception ex) when (ex is not HostAbortedException
    && ex.GetType().Name != "StopTheHostException")
{
    Log.Fatal(ex, "Flipcoin API terminated unexpectedly");
}
finally
{
    // Flush any buffered log events before the process exits.
    Log.CloseAndFlush();
}

// Exposes the implicit Program class so the integration tests can host the API
// with WebApplicationFactory<Program>.
public partial class Program;
