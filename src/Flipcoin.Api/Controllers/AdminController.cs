using Flipcoin.Api.Auth;
using Flipcoin.Application.Admin;
using Flipcoin.Application.Common;
using Flipcoin.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flipcoin.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminController : ControllerBase
{
    private readonly AdminQueryService _adminQueries;

    public AdminController(AdminQueryService adminQueries)
    {
        _adminQueries = adminQueries;
    }

    [HttpGet("wallets")]
    public async Task<ActionResult<IReadOnlyList<AdminWalletDto>>> GetWallets(CancellationToken cancellationToken)
        => Ok(await _adminQueries.GetWalletsAsync(cancellationToken));

    [HttpGet("transactions")]
    public async Task<ActionResult<PagedResult<AdminTransactionDto>>> GetTransactions(
        [FromQuery] TransactionType? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _adminQueries.GetTransactionsAsync(type, page, pageSize, cancellationToken));

    [HttpGet("game-rounds")]
    public async Task<ActionResult<PagedResult<AdminGameRoundDto>>> GetGameRounds(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _adminQueries.GetGameRoundsAsync(page, pageSize, cancellationToken));
}
