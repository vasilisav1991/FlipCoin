namespace Flipcoin.Application.Common;

/// <summary>A single page of results plus the metadata needed to page through them.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
