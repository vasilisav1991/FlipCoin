namespace Flipcoin.Client.Services;

/// <summary>An error surfaced from the API (already turned into a friendly message).</summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
