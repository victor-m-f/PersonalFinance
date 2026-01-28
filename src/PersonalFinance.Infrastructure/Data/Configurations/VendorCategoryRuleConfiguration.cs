using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Constraints;

namespace PersonalFinance.Infrastructure.Data.Configurations;

public sealed class VendorCategoryRuleConfiguration : IEntityTypeConfiguration<VendorCategoryRule>
{
    public void Configure(EntityTypeBuilder<VendorCategoryRule> builder)
    {
        builder.ToTable(nameof(VendorCategoryRule));
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.KeywordNormalized);
        builder.HasIndex(x => x.CategoryId);

        builder.Property(x => x.Keyword)
            .HasMaxLength(EntityConstraints.VendorCategoryRule.KeywordMaxLength)
            .IsRequired();

        builder.Property(x => x.KeywordNormalized)
            .HasMaxLength(EntityConstraints.VendorCategoryRule.KeywordMaxLength)
            .IsRequired();

        builder.Property(x => x.CategoryId)
            .IsRequired();

        builder.Property(x => x.Confidence)
            .HasConversion(
                confidence => confidence.Value,
                value => ConfidenceScore.FromStorage(value))
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.LastUsedAt);
    }
}
