using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Abstractions.RealTime;
using Flipcoin.Application.Wallets;

namespace Flipcoin.Application.Transfers;

/// <summary>
/// Moves coins from the sender's wallet to a recipient wallet atomically. The
/// debit, the credit, and both ledger entries are applied to tracked entities
/// and committed with a single SaveChanges, so the whole transfer is one
/// all-or-nothing database transaction.
/// </summary>
public class TransferHandler
{
    private readonly IWalletRepository _wallets;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWalletNotifier _notifier;

    public TransferHandler(IWalletRepository wallets, IUnitOfWork unitOfWork, IWalletNotifier notifier)
    {
        _wallets = wallets;
        _unitOfWork = unitOfWork;
        _notifier = notifier;
    }

    public async Task<TransferResult> HandleAsync(TransferCommand command, CancellationToken cancellationToken = default)
    {
        if (command.Amount <= 0m)
        {
            throw new ArgumentException("Transfer amount must be positive.", nameof(command));
        }

        var sender = await _wallets.GetByUserIdAsync(command.SenderUserId, cancellationToken)
            ?? throw new WalletNotFoundException();

        var recipient = await _wallets.GetByAddressAsync(command.ToAddress, cancellationToken)
            ?? throw new RecipientNotFoundException(command.ToAddress);

        if (recipient.Id == sender.Id)
        {
            throw new SelfTransferException();
        }

        // Throws InsufficientBalanceException if the sender cannot cover it.
        sender.SendTransfer(command.Amount, recipient.Address);
        recipient.ReceiveTransfer(command.Amount, sender.Address);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Push the new balances to both parties in real time (best-effort).
        await _notifier.WalletChangedAsync(sender.UserId, sender.Balance, cancellationToken);
        await _notifier.WalletChangedAsync(recipient.UserId, recipient.Balance, cancellationToken);

        return new TransferResult(recipient.Address, command.Amount, sender.Balance);
    }
}
