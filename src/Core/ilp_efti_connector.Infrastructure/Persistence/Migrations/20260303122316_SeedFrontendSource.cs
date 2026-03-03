using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ilp_efti_connector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedFrontendSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT IGNORE INTO sources (id, api_key_hash, code, config_json, created_at, is_active, name, type)
                VALUES ('22222222-2222-2222-2222-222222222222', NULL, 'TEST_FRONTEND', NULL, '2026-01-01 00:00:00', 1, 'Form Frontend Manuale', 'FORM');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sources",
                keyColumn: "id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));
        }
    }
}
