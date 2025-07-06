using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskxpress_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPremiumUserPriceToProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Verified",
                table: "Users",
                newName: "Premium");

            migrationBuilder.AddColumn<decimal>(
                name: "PremiumUserPrice",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PremiumUserPrice",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Premium",
                table: "Users",
                newName: "Verified");
        }
    }
}
