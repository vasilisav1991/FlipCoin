using Flipcoin.Application.Abstractions.Game;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Abstractions.RealTime;
using Flipcoin.Application.Wallets;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Exceptions;

namespace Flipcoin.Application.Game;

/// <summary>
/// Plays one server-decided coin flip. The server generates the outcome, applies
/// the payout math, and records the round plus any ledger entries in a single
/// transaction. The client never influences the outcome.
///
/// Rules: a win pays 2x the stake (net +stake); a loss forfeits it.
/// </summary>
public class PlayGameHandler
{
    private readonly IWalletRepository _wallets;
    private readonly IGameRoundRepository _gameRounds;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoinFlipper _coinFlipper;
    private readonly IWalletNotifier _notifier;

    public PlayGameHandler(
        IWalletRepository wallets,
        IGameRoundRepository gameRounds,
        IUnitOfWork unitOfWork,
        ICoinFlipper coinFlipper,
        IWalletNotifier notifier)
    {
        _wallets = wallets;
        _gameRounds = gameRounds;
        _unitOfWork = unitOfWork;
        _coinFlipper = coinFlipper;
        _notifier = notifier;
    }

    public async Task<PlayGameResult> HandleAsync(PlayGameCommand command, CancellationToken cancellationToken = default)
    {
        var stake = command.Stake;
        if (stake <= 0m)
        {
            throw new ArgumentException("Stake must be positive.", nameof(command));
        }

        var wallet = await _wallets.GetByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new WalletNotFoundException();

        if (stake > wallet.Balance)
        {
            throw new InsufficientBalanceException(wallet.Balance, stake);
        }

        var outcome = _coinFlipper.Flip();
        var won = command.Choice == outcome;

        wallet.PlaceStake(stake);
        var payout = won ? stake * 2m : 0m;
        if (payout > 0m)
        {
            wallet.ReceivePayout(payout);
        }

        var round = new GameRound(command.UserId, stake, command.Choice, outcome, payout);
        await _gameRounds.AddAsync(round, cancellationToken);

        // Round and any ledger entries persist together in one transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Push the new balance to the player in real time (best-effort).
        await _notifier.WalletChangedAsync(command.UserId, wallet.Balance, cancellationToken);

        return new PlayGameResult(command.Choice, outcome, won, payout, wallet.Balance);
    }
}
