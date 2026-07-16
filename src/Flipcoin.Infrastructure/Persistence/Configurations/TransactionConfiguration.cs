using Flipcoin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flipcoin.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);
        // Ids are generated in the domain constructors, never by the database.
        // This also ensures new transactions reached via the wallet's collection
        // are tracked as inserts, not updates.
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.Type)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Amount)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(t => t.CounterpartyAddress)
            .HasMaxLength(32);

        builder.Property(t => t.Timestamp).IsRequired();

        builder.Property(t => t.BalanceAfter)
            .HasPrecision(18, 4)
            .IsRequired();

        // Non-unique index to make a wallet's history query efficient.
        builder.HasIndex(t => t.WalletId);
    }
}
