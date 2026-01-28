using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PersonalFinance.Domain.Entities;
using PersonalFinance.Domain.ValueObjects;
using PersonalFinance.Shared.Constraints;

namespace PersonalFinance.Infrastructure.Data.Configurations;

public sealed class ImportedDocumentConfiguration : IEntityTypeConfiguration<ImportedDocument>
{
    public void Configure(EntityTypeBuilder<ImportedDocument> builder)
    {
        builder.ToTable(nameof(ImportedDocument));
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Hash).IsUnique();

        builder.Property(x => x.OriginalFileName)
            .HasMaxLength(EntityConstraints.ImportedDocument.FileNameMaxLength)
            .IsRequired();

        builder.Property(x => x.StoredFileName)
            .HasMaxLength(EntityConstraints.ImportedDocument.FileNameMaxLength)
            .IsRequired();

        builder.Property(x => x.FileExtension)
            .HasMaxLength(EntityConstraints.ImportedDocument.FileExtensionMaxLength)
            .IsRequired();

        builder.Property(x => x.FileSize)
            .IsRequired();

        builder.Property(x => x.Hash)
            .HasConversion(
                hash => hash.Value,
                value => DocumentHash.FromStorage(value))
            .HasMaxLength(EntityConstraints.ImportedDocument.HashMaxLength)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(EntityConstraints.ImportedDocument.StatusMaxLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ProcessedAt);
        builder.Property(x => x.IsOcrUsed)
            .IsRequired();

        builder.Property(x => x.FailureReason)
            .HasMaxLength(EntityConstraints.ImportedDocument.FailureReasonMaxLength);
    }
}
