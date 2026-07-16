using Flipcoin.Api.Auth;
using Flipcoin.Api.Contracts.Transfers;
using Flipcoin.Application.Transfers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipcoin.Api.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly TransferHandler _transferHandler;

    public TransfersController(TransferHandler transferHandler)
    {
        _transferHandler = transferHandler;
    }

    [HttpPost]
    public async Task<ActionResult<TransferResult>> Transfer(
        TransferRequest request,
        CancellationToken cancellationToken)
    {
        // Sender is always the authenticated user, never taken from the request.
        var command = new TransferCommand(User.GetUserId(), request.ToAddress, request.Amount);
        var result = await _transferHandler.HandleAsync(command, cancellationToken);
        return Ok(result);
    }
}
