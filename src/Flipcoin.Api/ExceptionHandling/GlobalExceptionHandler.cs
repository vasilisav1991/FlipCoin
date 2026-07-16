using Flipcoin.Application.Auth;
using Flipcoin.Application.Transfers;
using Flipcoin.Application.Wallets;
using Flipcoin.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Flipcoin.Api.ExceptionHandling;

/// <summary>
/// Central translation of exceptions into RFC 7807 ProblemDetails responses, so
/// controllers never map errors themselves. Known application exceptions map to
/// specific status codes; anything else is a 500 (logged, with no detail leaked).
/// New exception types are added to the switch as later phases introduce them.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation failed"),
            EmailAlreadyInUseException => (StatusCodes.Status409Conflict, "Email already in use"),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, "Invalid credentials"),
            InsufficientBalanceException => (StatusCodes.Status409Conflict, "Insufficient balance"),
            WalletNotFoundException => (StatusCodes.Status404NotFound, "Wallet not found"),
            RecipientNotFoundException => (StatusCodes.Status404NotFound, "Recipient not found"),
            SelfTransferException => (StatusCodes.Status400BadRequest, "Invalid transfer"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            // Unknown failure: log the detail server-side, reveal nothing to the client.
            _logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = status;

        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : exception.Message
        };

        if (exception is ValidationException validationException)
        {
            // Field -> messages, so the client sees exactly what failed.
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }
}
