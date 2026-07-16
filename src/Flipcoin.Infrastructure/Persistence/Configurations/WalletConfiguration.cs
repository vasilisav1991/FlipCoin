using Flipcoin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flipcoin.Infrastructure.Persistence.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);
        builder.Property(w => w.Id).ValueGeneratedNever();

        builder.Property(w => w.Address)
            .IsRequired()
            .HasMaxLength(32);
        builder.HasIndex(w => w.Address).IsUnique();

        // Unique FK enforces the one-wallet-per-user (1:1) rule at the DB level.
        builder.HasIndex(w => w.UserId).IsUnique();

        builder.Property(w => w.Balance)
            .HasPrecision(18, 4)
            .IsRequired();

        // Wallet 1 -> * Transactions. The collection is exposed read-only over the
        // private _transactions field, so tell EF to read/write via that field.
        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Metadata
            .FindNavigation(nameof(Wallet.Transactions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
