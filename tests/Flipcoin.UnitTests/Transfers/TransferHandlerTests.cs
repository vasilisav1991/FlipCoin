using Flipcoin.Application.Abstractions.Persistence;
using Flipcoin.Application.Abstractions.RealTime;
using Flipcoin.Application.Transfers;
using Flipcoin.Application.Wallets;
using Flipcoin.Domain.Entities;
using Flipcoin.Domain.Exceptions;
using Moq;

namespace Flipcoin.UnitTests.Transfers;

public class TransferHandlerTests
{
    private readonly Mock<IWalletRepository> _wallets = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IWalletNotifier> _notifier = new();

    private TransferHandler CreateHandler() => new(_wallets.Object, _unitOfWork.Object, _notifier.Object);

    private static Wallet WalletFor(Guid userId, string address, decimal balance)
        => new(userId, address, balance);

    [Fact]
    public async Task Moves_funds_and_commits_once()
    {
        var senderUserId = Guid.NewGuid();
        var sender = WalletFor(senderUserId, "FLIP-sender0", 100m);
        var recipient = WalletFor(Guid.NewGuid(), "FLIP-recip00", 50m);

        _wallets.Setup(w => w.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync(sender);
        _wallets.Setup(w => w.GetByAddressAsync("FLIP-recip00", It.IsAny<CancellationToken>())).ReturnsAsync(recipient);

        var result = await CreateHandler().HandleAsync(new TransferCommand(senderUserId, "FLIP-recip00", 30m));

        Assert.Equal(70m, sender.Balance);
        Assert.Equal(80m, recipient.Balance);
        Assert.Equal(70m, result.NewBalance);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Rejects_transfer_exceeding_balance_and_does_not_commit()
    {
        var senderUserId = Guid.NewGuid();
        var sender = WalletFor(senderUserId, "FLIP-sender0", 20m);
        var recipient = WalletFor(Guid.NewGuid(), "FLIP-recip00", 0m);

        _wallets.Setup(w => w.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync(sender);
        _wallets.Setup(w => w.GetByAddressAsync("FLIP-recip00", It.IsAny<CancellationToken>())).ReturnsAsync(recipient);

        await Assert.ThrowsAsync<InsufficientBalanceException>(() =>
            CreateHandler().HandleAsync(new TransferCommand(senderUserId, "FLIP-recip00", 50m)));

        Assert.Equal(20m, sender.Balance);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Rejects_unknown_recipient()
    {
        var senderUserId = Guid.NewGuid();
        _wallets.Setup(w => w.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(WalletFor(senderUserId, "FLIP-sender0", 100m));
        _wallets.Setup(w => w.GetByAddressAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Wallet?)null);

        await Assert.ThrowsAsync<RecipientNotFoundException>(() =>
            CreateHandler().HandleAsync(new TransferCommand(senderUserId, "FLIP-missing0", 10m)));
    }

    [Fact]
    public async Task Rejects_transfer_to_self()
    {
        var senderUserId = Guid.NewGuid();
        var wallet = WalletFor(senderUserId, "FLIP-self0000", 100m);
        _wallets.Setup(w => w.GetByUserIdAsync(senderUserId, It.IsAny<CancellationToken>())).ReturnsAsync(wallet);
        _wallets.Setup(w => w.GetByAddressAsync("FLIP-self0000", It.IsAny<CancellationToken>())).ReturnsAsync(wallet);

        await Assert.ThrowsAsync<SelfTransferException>(() =>
            CreateHandler().HandleAsync(new TransferCommand(senderUserId, "FLIP-self0000", 10m)));

        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Rejects_non_positive_amount()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateHandler().HandleAsync(new TransferCommand(Guid.NewGuid(), "FLIP-recip00", 0m)));
    }
}
