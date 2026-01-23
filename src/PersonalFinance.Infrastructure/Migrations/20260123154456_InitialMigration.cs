using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalFinance.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialMigration : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Category",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false),
                ColorHex = table.Column<string>(type: "TEXT", maxLength: 9, nullable: false),
                ParentId = table.Column<Guid>(type: "TEXT", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Category", x => x.Id);
                table.ForeignKey(
                    name: "FK_Category_Category_ParentId",
                    column: x => x.ParentId,
                    principalTable: "Category",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Expense",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "TEXT", nullable: false),
                Date = table.Column<DateTime>(type: "TEXT", nullable: false),
                Amount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                DescriptionSearch = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Expense", x => x.Id);
                table.ForeignKey(
                    name: "FK_Expense_Category_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Category",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(
            name: "IX_Category_ParentId",
            table: "Category",
            column: "ParentId");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_CategoryId",
            table: "Expense",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_Date",
            table: "Expense",
            column: "Date");

        migrationBuilder.CreateIndex(
            name: "IX_Expense_DescriptionSearch",
            table: "Expense",
            column: "DescriptionSearch");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Expense");

        migrationBuilder.DropTable(
            name: "Category");
    }
}
