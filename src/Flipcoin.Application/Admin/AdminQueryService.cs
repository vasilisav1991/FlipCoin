using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Common;
using Flipcoin.Domain.Enums;

namespace Flipcoin.Application.Admin;

/// <summary>
/// Read-side service backing the admin audit endpoints. Applies paging bounds
/// and delegates the projections to the read repository.
/// </summary>
public class AdminQueryService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly IAdminReadRepository _repository;

    public AdminQueryService(IAdminReadRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<AdminWalletDto>> GetWalletsAsync(CancellationToken cancellationToken = default)
        => _repository.GetWalletsAsync(cancellationToken);

    public async Task<PagedResult<AdminTransactionDto>> GetTransactionsAsync(
        TransactionType? type, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var (items, total) = await _repository.GetTransactionsAsync(
            type, (page - 1) * pageSize, pageSize, cancellationToken);
        return new PagedResult<AdminTransactionDto>(items, page, pageSize, total);
    }

    public async Task<PagedResult<AdminGameRoundDto>> GetGameRoundsAsync(
        int page, int pageSize, CancellationToken cancellationToken = default)
    {
        (page, pageSize) = Normalize(page, pageSize);
        var (items, total) = await _repository.GetGameRoundsAsync(
            (page - 1) * pageSize, pageSize, cancellationToken);
        return new PagedResult<AdminGameRoundDto>(items, page, pageSize, total);
    }

    private static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? DefaultPageSize : Math.Min(pageSize, MaxPageSize);
        return (page, pageSize);
    }
}
