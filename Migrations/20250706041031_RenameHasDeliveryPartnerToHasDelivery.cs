using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskxpress_api.Migrations
{
    /// <inheritdoc />
    public partial class RenameHasDeliveryPartnerToHasDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceWithDelivery",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "HasDeliveryPartner",
                table: "Stalls",
                newName: "hasDelivery");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "hasDelivery",
                table: "Stalls",
                newName: "HasDeliveryPartner");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceWithDelivery",
                table: "Products",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
