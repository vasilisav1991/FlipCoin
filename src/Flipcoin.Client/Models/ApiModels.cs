namespace Flipcoin.Client.Models;

// Client-side shapes of the API's JSON responses. System.Text.Json matches these
// PascalCase names to the API's camelCase output case-insensitively.

public record WalletDto(string Address, decimal Balance);

public record TransactionDto(
    string Type, decimal Amount, string? CounterpartyAddress, DateTime Timestamp, decimal BalanceAfter);

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount);

public record RegisterResult(Guid UserId, string WalletAddress);

public record PlayResult(string Choice, string Outcome, bool Won, decimal Payout, decimal NewBalance);

public record TransferResult(string ToAddress, decimal Amount, decimal NewBalance);

public record AdminWallet(string Email, string Role, string? Address, decimal? Balance);

public record AdminTransaction(
    string WalletAddress, string Type, decimal Amount, string? CounterpartyAddress,
    DateTime Timestamp, decimal BalanceAfter);

public record AdminGameRound(
    Guid UserId, decimal Stake, string Choice, string Outcome, bool Won, decimal Payout, DateTime PlayedAt);

// Shape of the API's ProblemDetails error responses.
public record ApiProblemDetails(string? Title, string? Detail, int? Status, Dictionary<string, string[]>? Errors);
