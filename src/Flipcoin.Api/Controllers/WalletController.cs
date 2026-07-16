using Flipcoin.Api.Auth;
using Flipcoin.Application.Common;
using Flipcoin.Application.Wallets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipcoin.Api.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private readonly GetMyWalletHandler _getWallet;
    private readonly GetMyTransactionsHandler _getTransactions;

    public WalletController(GetMyWalletHandler getWallet, GetMyTransactionsHandler getTransactions)
    {
        _getWallet = getWallet;
        _getTransactions = getTransactions;
    }

    [HttpGet]
    public async Task<ActionResult<WalletDto>> GetWallet(CancellationToken cancellationToken)
    {
        var wallet = await _getWallet.HandleAsync(User.GetUserId(), cancellationToken);
        return wallet is null ? NotFound() : Ok(wallet);
    }

    [HttpGet("transactions")]
    public async Task<ActionResult<PagedResult<TransactionDto>>> GetTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _getTransactions.HandleAsync(User.GetUserId(), page, pageSize, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
