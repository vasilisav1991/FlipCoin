using Flipcoin.Api.Contracts.Auth;
using Flipcoin.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipcoin.Api.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly RegisterUserHandler _registerHandler;
    private readonly LoginUserHandler _loginHandler;

    public AuthController(RegisterUserHandler registerHandler, LoginUserHandler loginHandler)
    {
        _registerHandler = registerHandler;
        _loginHandler = loginHandler;
    }

    [HttpPost("register")]
    public async Task<ActionResult<RegisterUserResult>> Register(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _registerHandler.HandleAsync(
            new RegisterUserCommand(request.Email, request.Password), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginUserResult>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _loginHandler.HandleAsync(
            new LoginUserCommand(request.Email, request.Password), cancellationToken);

        return Ok(result);
    }
}
