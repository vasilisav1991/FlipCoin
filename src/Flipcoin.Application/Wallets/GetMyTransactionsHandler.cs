using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Common;

namespace Flipcoin.Application.Wallets;

/// <summary>
/// Returns a page of the caller's own transaction history (most recent first).
/// Like the wallet lookup, the user id comes from the authenticated principal.
/// Returns null when the user has no wallet.
/// </summary>
public class GetMyTransactionsHandler
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IWalletRepository _wallets;
    private readonly ITransactionRepository _transactions;

    public GetMyTransactionsHandler(IWalletRepository wallets, ITransactionRepository transactions)
    {
        _wallets = wallets;
        _transactions = transactions;
    }

    public async Task<PagedResult<TransactionDto>?> HandleAsync(
        Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var wallet = await _wallets.GetByUserIdAsync(userId, cancellationToken);
        if (wallet is null)
        {
            return null;
        }

        // Clamp paging inputs to sane bounds.
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);

        var totalCount = await _transactions.CountByWalletAsync(wallet.Id, cancellationToken);
        var entries = await _transactions.GetByWalletPagedAsync(
            wallet.Id, (page - 1) * pageSize, pageSize, cancellationToken);

        var items = entries
            .Select(t => new TransactionDto(
                t.Type.ToString(), t.Amount, t.CounterpartyAddress, t.Timestamp, t.BalanceAfter))
            .ToList();

        return new PagedResult<TransactionDto>(items, page, pageSize, totalCount);
    }
}
