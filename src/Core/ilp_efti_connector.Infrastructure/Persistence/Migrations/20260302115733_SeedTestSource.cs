using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ilp_efti_connector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "sources",
                columns: new[] { "id", "api_key_hash", "code", "config_json", "created_at", "is_active", "name", "type" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), null, "TMS_TEST", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Sorgente di Test", "TMS" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sources",
                keyColumn: "id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
