using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskxpress_api.Migrations
{
    /// <inheritdoc />
    public partial class AddStallDeliveryColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeliveryAvailable",
                table: "Stalls",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasDeliveryPartner",
                table: "Stalls",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DeliveryRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    AssignedDeliveryPartnerId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeliveryRequests_Users_AssignedDeliveryPartnerId",
                        column: x => x.AssignedDeliveryPartnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "OrderConfirmations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ConfirmationDeadline = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsAutoConfirmed = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AutoConfirmedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderConfirmations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderConfirmations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_AssignedDeliveryPartnerId",
                table: "DeliveryRequests",
                column: "AssignedDeliveryPartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryRequests_OrderId",
                table: "DeliveryRequests",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderConfirmations_OrderId",
                table: "OrderConfirmations",
                column: "OrderId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryRequests");

            migrationBuilder.DropTable(
                name: "OrderConfirmations");

            migrationBuilder.DropColumn(
                name: "DeliveryAvailable",
                table: "Stalls");

            migrationBuilder.DropColumn(
                name: "HasDeliveryPartner",
                table: "Stalls");
        }
    }
}
