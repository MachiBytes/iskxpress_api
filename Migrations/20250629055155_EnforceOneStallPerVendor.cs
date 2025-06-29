using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iskxpress_api.Migrations
{
    /// <inheritdoc />
    public partial class EnforceOneStallPerVendor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add unique constraint using raw SQL to avoid index conflicts
            migrationBuilder.Sql("ALTER TABLE `Stalls` ADD CONSTRAINT `UC_Stalls_VendorId` UNIQUE (`VendorId`);");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the unique constraint
            migrationBuilder.Sql("ALTER TABLE `Stalls` DROP CONSTRAINT `UC_Stalls_VendorId`;");
        }
    }
}
