using Flipcoin.Application.Admin;
using Flipcoin.Domain.Enums;

namespace Flipcoin.Application.Abstractions.Persistence;

/// <summary>
/// Read-only queries for the admin audit views. These project straight to the
/// admin DTOs (a lightweight read side) rather than returning aggregates.
/// </summary>
public interface IAdminReadRepository
{
    Task<IReadOnlyList<AdminWalletDto>> GetWalletsAsync(CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AdminTransactionDto> Items, int TotalCount)> GetTransactionsAsync(
        TransactionType? type, int skip, int take, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<AdminGameRoundDto> Items, int TotalCount)> GetGameRoundsAsync(
        int skip, int take, CancellationToken cancellationToken = default);
}
