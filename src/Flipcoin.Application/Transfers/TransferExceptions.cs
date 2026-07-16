namespace Flipcoin.Application.Transfers;

/// <summary>The destination address does not match any wallet. Mapped to HTTP 404.</summary>
public class RecipientNotFoundException : Exception
{
    public RecipientNotFoundException(string address)
        : base($"No wallet found for address '{address}'.")
    {
    }
}

/// <summary>A transfer to one's own wallet was attempted. Mapped to HTTP 400.</summary>
public class SelfTransferException : Exception
{
    public SelfTransferException()
        : base("You cannot transfer coins to your own wallet.")
    {
    }
}
