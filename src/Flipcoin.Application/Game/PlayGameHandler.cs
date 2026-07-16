using Flipcoin.Application.Abstractions.Game;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Wallets;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Exceptions;

namespace Flipcoin.Application.Game;

/// <summary>
/// Plays one server-decided coin flip. The server generates the outcome, applies
/// the payout math, and records the round plus any ledger entries in a single
/// transaction. The client never influences the outcome.
///
/// Rules: a staked win pays 2x the stake (net +stake); a staked loss forfeits it.
/// A practice round (no stake) pays a flat +5 on a correct guess and nothing on a
/// wrong one. (Interpretation of the plan's "flat +5" reward — easily changed.)
/// </summary>
public class PlayGameHandler
{
    private const decimal PracticeReward = 5m;

    private readonly IWalletRepository _wallets;
    private readonly IGameRoundRepository _gameRounds;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICoinFlipper _coinFlipper;

    public PlayGameHandler(
        IWalletRepository wallets,
        IGameRoundRepository gameRounds,
        IUnitOfWork unitOfWork,
        ICoinFlipper coinFlipper)
    {
        _wallets = wallets;
        _gameRounds = gameRounds;
        _unitOfWork = unitOfWork;
        _coinFlipper = coinFlipper;
    }

    public async Task<PlayGameResult> HandleAsync(PlayGameCommand command, CancellationToken cancellationToken = default)
    {
        var stake = command.Stake ?? 0m;
        if (stake < 0m)
        {
            throw new ArgumentException("Stake cannot be negative.", nameof(command));
        }

        var wallet = await _wallets.GetByUserIdAsync(command.UserId, cancellationToken)
            ?? throw new WalletNotFoundException();

        if (stake > 0m && stake > wallet.Balance)
        {
            throw new InsufficientBalanceException(wallet.Balance, stake);
        }

        var outcome = _coinFlipper.Flip();
        var won = command.Choice == outcome;

        decimal payout;
        if (stake > 0m)
        {
            wallet.PlaceStake(stake);
            payout = won ? stake * 2m : 0m;
            if (payout > 0m)
            {
                wallet.ReceivePayout(payout);
            }
        }
        else
        {
            payout = won ? PracticeReward : 0m;
            if (payout > 0m)
            {
                wallet.ReceiveReward(payout);
            }
        }

        var round = new GameRound(command.UserId, stake, command.Choice, outcome, payout);
        await _gameRounds.AddAsync(round, cancellationToken);

        // Round and any ledger entries persist together in one transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new PlayGameResult(command.Choice, outcome, won, payout, wallet.Balance);
    }
}
