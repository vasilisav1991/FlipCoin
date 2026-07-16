using Flipcoin.Application.Abstractions.Game;
using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Game;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Enums;
using Flipcoin.Domain.Exceptions;
using Moq;

namespace Flipcoin.UnitTests.Game;

public class PlayGameHandlerTests
{
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<IGameRoundRepository> _gameRounds = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICoinFlipper> _coinFlipper = new();

    private PlayGameHandler CreateHandler() =>
        new(_wallets.Object, _gameRounds.Object, _unitOfWork.Object, _coinFlipper.Object);

    private Wallet SetupWallet(Guid userId, decimal balance)
    {
        var wallet = new Wallet(userId, "FLIP-player00", balance);
        _wallets.Setup(w => w.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        return wallet;
    }

    [Fact]
    public async Task Staked_win_pays_double_the_stake()
    {
        var userId = Guid.NewGuid();
        var wallet = SetupWallet(userId, 100m);
        _coinFlipper.Setup(c => c.Flip()).Returns(CoinSide.Heads);

        var result = await CreateHandler().HandleAsync(new PlayGameCommand(userId, CoinSide.Heads, 10m));

        Assert.True(result.Won);
        Assert.Equal(20m, result.Payout);
        // -10 stake +20 payout => 110
        Assert.Equal(110m, wallet.Balance);
        Assert.Equal(110m, result.NewBalance);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Staked_loss_forfeits_the_stake()
    {
        var userId = Guid.NewGuid();
        var wallet = SetupWallet(userId, 100m);
        _coinFlipper.Setup(c => c.Flip()).Returns(CoinSide.Tails);

        var result = await CreateHandler().HandleAsync(new PlayGameCommand(userId, CoinSide.Heads, 10m));

        Assert.False(result.Won);
        Assert.Equal(0m, result.Payout);
        Assert.Equal(90m, wallet.Balance);
    }

    [Fact]
    public async Task Practice_win_rewards_flat_five()
    {
        var userId = Guid.NewGuid();
        var wallet = SetupWallet(userId, 100m);
        _coinFlipper.Setup(c => c.Flip()).Returns(CoinSide.Heads);

        var result = await CreateHandler().HandleAsync(new PlayGameCommand(userId, CoinSide.Heads, null));

        Assert.True(result.Won);
        Assert.Equal(5m, result.Payout);
        Assert.Equal(105m, wallet.Balance);
    }

    [Fact]
    public async Task Practice_loss_pays_nothing()
    {
        var userId = Guid.NewGuid();
        var wallet = SetupWallet(userId, 100m);
        _coinFlipper.Setup(c => c.Flip()).Returns(CoinSide.Tails);

        var result = await CreateHandler().HandleAsync(new PlayGameCommand(userId, CoinSide.Heads, null));

        Assert.False(result.Won);
        Assert.Equal(0m, result.Payout);
        Assert.Equal(100m, wallet.Balance);
    }

    [Fact]
    public async Task Stake_greater_than_balance_is_rejected()
    {
        var userId = Guid.NewGuid();
        var wallet = SetupWallet(userId, 5m);
        _coinFlipper.Setup(c => c.Flip()).Returns(CoinSide.Heads);

        await Assert.ThrowsAsync<InsufficientBalanceException>(() =>
            CreateHandler().HandleAsync(new PlayGameCommand(userId, CoinSide.Heads, 10m)));

        Assert.Equal(5m, wallet.Balance);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Records_the_round_with_correct_balance_after()
    {
        var userId = Guid.NewGuid();
        var wallet = SetupWallet(userId, 100m);
        _coinFlipper.Setup(c => c.Flip()).Returns(CoinSide.Heads);
        GameRound? captured = null;
        _gameRounds.Setup(r => r.AddAsync(It.IsAny<GameRound>(), It.IsAny<CancellationToken>()))
            .Callback<GameRound, CancellationToken>((r, _) => captured = r);

        await CreateHandler().HandleAsync(new PlayGameCommand(userId, CoinSide.Heads, 10m));

        Assert.NotNull(captured);
        Assert.Equal(10m, captured!.Stake);
        Assert.Equal(20m, captured.Payout);
        Assert.True(captured.Won);
        // Two ledger entries were recorded on the wallet: Stake then Payout.
        Assert.Equal(2, wallet.Transactions.Count);
        Assert.Equal(110m, wallet.Transactions.Last().BalanceAfter);
    }
}
