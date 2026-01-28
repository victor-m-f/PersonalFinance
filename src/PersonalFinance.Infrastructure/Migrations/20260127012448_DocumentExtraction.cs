using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Infrastructure.Migrations;

/// <inheritdoc />
public partial class DocumentExtraction : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ImportedDocument",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                OriginalFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                StoredFileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                FileExtension = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                Hash = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                Status = table.Column<string>(type: "TEXT", maxLength: 24, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                ProcessedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                IsOcrUsed = table.Column<bool>(type: "INTEGER", nullable: false),
                FailureReason = table.Column<string>(type: "TEXT", maxLength: 400, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ImportedDocument", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "VendorCategoryRule",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Keyword = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                KeywordNormalized = table.Column<string>(type: "TEXT", maxLength: 160, nullable: false),
                CategoryId = table.Column<Guid>(type: "TEXT", nullable: false),
                Confidence = table.Column<double>(type: "REAL", nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                LastUsedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_VendorCategoryRule", x => x.Id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Category_Name",
            table: "Category",
            column: "Name");

        migrationBuilder.CreateIndex(
            name: "IX_ImportedDocument_CreatedAt",
            table: "ImportedDocument",
            column: "CreatedAt");

        migrationBuilder.CreateIndex(
            name: "IX_ImportedDocument_Hash",
            table: "ImportedDocument",
            column: "Hash",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ImportedDocument_Status",
            table: "ImportedDocument",
            column: "Status");

        migrationBuilder.CreateIndex(
            name: "IX_VendorCategoryRule_CategoryId",
            table: "VendorCategoryRule",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_VendorCategoryRule_KeywordNormalized",
            table: "VendorCategoryRule",
            column: "KeywordNormalized");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ImportedDocument");

        migrationBuilder.DropTable(
            name: "VendorCategoryRule");

        migrationBuilder.DropIndex(
            name: "IX_Category_Name",
            table: "Category");
    }
}
