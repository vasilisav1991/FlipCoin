using Flipcoin.Api.Auth;
using Flipcoin.Api.Contracts.Game;
using Flipcoin.Application.Game;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipcoin.Api.Controllers;

[ApiController]
[Route("api/game")]
[Authorize]
public class GameController : ControllerBase
{
    private readonly PlayGameHandler _playGameHandler;

    public GameController(PlayGameHandler playGameHandler)
    {
        _playGameHandler = playGameHandler;
    }

    [HttpPost("play")]
    public async Task<ActionResult<PlayGameResult>> Play(
        PlayGameRequest request,
        CancellationToken cancellationToken)
    {
        var command = new PlayGameCommand(User.GetUserId(), request.Choice, request.Stake);
        var result = await _playGameHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }
}
