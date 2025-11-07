using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Emata.Exercise.LoansManagement.Repayments.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Payments");

            migrationBuilder.CreateTable(
                name: "Repayments",
                schema: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LoanId = table.Column<Guid>(type: "uuid", nullable: false),
                    AmountToPrinciple = table.Column<decimal>(type: "numeric", nullable: false),
                    AmountToInterest = table.Column<decimal>(type: "numeric", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Repayments", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Repayments",
                schema: "Payments");
        }
    }
}
