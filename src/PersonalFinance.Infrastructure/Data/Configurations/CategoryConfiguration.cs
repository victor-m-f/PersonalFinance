using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Infrastructure.Data.Converters;
using PersonalFinance.Shared.Constraints;

namespace PersonalFinance.Infrastructure.Data.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable(nameof(Category));

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.ParentId);
        builder.HasIndex(x => x.Name);

        builder.Property(x => x.Name)
            .HasMaxLength(EntityConstraints.Category.NameMaxLength)
            .IsRequired();

        builder.Property(x => x.Color)
            .HasConversion(new CategoryColorConverter())
            .HasColumnName("ColorHex")
            .HasMaxLength(EntityConstraints.Category.ColorHexLength)
            .IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt);
    }
}
