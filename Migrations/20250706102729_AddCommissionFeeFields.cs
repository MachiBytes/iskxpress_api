using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskxpress_api.Migrations
{
    /// <inheritdoc />
    public partial class AddCommissionFeeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalCommissionFee",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionFee",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalCommissionFee",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CommissionFee",
                table: "OrderItems");
        }
    }
}
