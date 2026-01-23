using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Search;
using PersonalFinance.Shared.Constraints;

namespace PersonalFinance.Infrastructure.Data.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable(nameof(Expense));
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.Date);
        builder.HasIndex(x => x.CategoryId);
        builder.HasIndex(x => x.DescriptionSearch);

        builder.Property(x => x.Date)
            .IsRequired();

        builder.Property(x => x.Amount)
            .HasPrecision(
            ValueObjectConstraints.Money.Precision,
            ValueObjectConstraints.Money.Scale)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(EntityConstraints.Expense.DescriptionMaxLength);

        builder.Property(x => x.DescriptionSearch)
            .HasMaxLength(EntityConstraints.Expense.DescriptionSearchMaxLength);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }

    public static void ConfigureSaveChanges(ChangeTracker changeTracker)
    {
        var entries = changeTracker.Entries<Expense>();

        foreach (var entry in entries)
        {
            if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
            {
                continue;
            }

            if (entry.State == EntityState.Modified && !entry.Property(expense => expense.Description).IsModified)
            {
                continue;
            }

            var normalized = TextSearchNormalizer.Normalize(entry.Entity.Description);
            var current = entry.Property(expense => expense.DescriptionSearch).CurrentValue as string;

            if (string.Equals(current, normalized, StringComparison.Ordinal))
            {
                continue;
            }

            entry.Property(expense => expense.DescriptionSearch).CurrentValue = normalized;
        }
    }
}
