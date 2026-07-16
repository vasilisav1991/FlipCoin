using Flipcoin.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flipcoin.Infrastructure.Persistence.Configurations;

public class GameRoundConfiguration : IEntityTypeConfiguration<GameRound>
{
    public void Configure(EntityTypeBuilder<GameRound> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Stake)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(g => g.Choice)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(g => g.Outcome)
            .HasConversion<string>()
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(g => g.Won).IsRequired();

        builder.Property(g => g.Payout)
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(g => g.PlayedAt).IsRequired();

        builder.HasIndex(g => g.UserId);

        // FK to the player. No navigation on User for rounds, so WithMany() is empty.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
