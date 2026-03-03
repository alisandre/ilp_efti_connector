using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ilp_efti_connector.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "sources",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_key_hash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    config_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sources", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    username = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    full_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    keycloak_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    roles_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "DATETIME", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    customer_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    business_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    vat_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    eori_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contact_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    auto_created = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    source_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    updated_at = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_customers_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    entity_type = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    entity_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    action_type = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    performed_by_user_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    performed_by_source_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    old_value_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    new_value_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.id);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_performed_by_user_id",
                        column: x => x.performed_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customer_destinations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    customer_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    destination_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    address_line1 = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    city = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    postal_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    province = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_code = table.Column<string>(type: "CHAR(2)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    un_locode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_default = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    auto_created = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    updated_at = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_destinations", x => x.id);
                    table.ForeignKey(
                        name: "FK_customer_destinations_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transport_operations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    source_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    customer_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    destination_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    operation_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dataset_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hashcode = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    hashcode_algorithm = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    raw_payload_json = table.Column<string>(type: "JSON", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    updated_at = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "CHAR(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_operations", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_operations_customer_destinations_destination_id",
                        column: x => x.destination_id,
                        principalTable: "customer_destinations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_transport_operations_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transport_operations_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transport_operations_users_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_transport_operations_users_updated_by_user_id",
                        column: x => x.updated_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "efti_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    source_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    transport_operation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    correlation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    gateway_provider = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    direction = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dataset_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payload_json = table.Column<string>(type: "JSON", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    external_uuid = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    retry_count = table.Column<short>(type: "smallint", nullable: false, defaultValue: (short)0),
                    next_retry_at = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    sent_at = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    acknowledged_at = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_efti_messages", x => x.id);
                    table.ForeignKey(
                        name: "FK_efti_messages_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_efti_messages_transport_operations_transport_operation_id",
                        column: x => x.transport_operation_id,
                        principalTable: "transport_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transport_carriers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    transport_operation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    player_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    street_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    city_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_code = table.Column<string>(type: "CHAR(2)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tax_registration = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    eori_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tractor_plate = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    equipment_category = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_carriers", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_carriers_transport_operations_transport_operation_~",
                        column: x => x.transport_operation_id,
                        principalTable: "transport_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transport_consignees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    transport_operation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    player_type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    street_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    post_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    city_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_code = table.Column<string>(type: "CHAR(2)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    country_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tax_registration = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    eori_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_consignees", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_consignees_transport_operations_transport_operatio~",
                        column: x => x.transport_operation_id,
                        principalTable: "transport_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transport_consignment_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    transport_operation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    total_item_quantity = table.Column<int>(type: "int", nullable: false),
                    total_weight = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    total_volume = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_consignment_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_consignment_items_transport_operations_transport_o~",
                        column: x => x.transport_operation_id,
                        principalTable: "transport_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transport_details",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    transport_operation_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    cargo_type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    incoterms = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acceptance_street_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acceptance_post_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acceptance_city_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acceptance_country_code = table.Column<string>(type: "CHAR(2)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acceptance_country_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    acceptance_date = table.Column<DateTime>(type: "DATETIME", nullable: true),
                    receipt_street_name = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receipt_post_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receipt_city_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receipt_country_code = table.Column<string>(type: "CHAR(2)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    receipt_country_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_details", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_details_transport_operations_transport_operation_id",
                        column: x => x.transport_operation_id,
                        principalTable: "transport_operations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transport_packages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    consignment_item_id = table.Column<Guid>(type: "CHAR(36)", nullable: false, collation: "ascii_general_ci"),
                    sort_order = table.Column<int>(type: "int", nullable: false),
                    shipping_marks = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    item_quantity = table.Column<int>(type: "int", nullable: false),
                    type_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gross_weight = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: false),
                    gross_volume = table.Column<decimal>(type: "DECIMAL(10,3)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transport_packages", x => x.id);
                    table.ForeignKey(
                        name: "FK_transport_packages_transport_consignment_items_consignment_i~",
                        column: x => x.consignment_item_id,
                        principalTable: "transport_consignment_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_id",
                table: "audit_logs",
                column: "entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_type",
                table: "audit_logs",
                column: "entity_type");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_entity_type_entity_id_created_at",
                table: "audit_logs",
                columns: new[] { "entity_type", "entity_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_performed_by_user_id",
                table: "audit_logs",
                column: "performed_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_destinations_customer_id",
                table: "customer_destinations",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_customer_destinations_destination_code",
                table: "customer_destinations",
                column: "destination_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_customer_code",
                table: "customers",
                column: "customer_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_customers_source_id",
                table: "customers",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_correlation_id",
                table: "efti_messages",
                column: "correlation_id");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_created_at",
                table: "efti_messages",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_external_id",
                table: "efti_messages",
                column: "external_id");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_next_retry_at",
                table: "efti_messages",
                column: "next_retry_at");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_source_id",
                table: "efti_messages",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_status",
                table: "efti_messages",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_efti_messages_transport_operation_id",
                table: "efti_messages",
                column: "transport_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_sources_code",
                table: "sources",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_carriers_transport_operation_id",
                table: "transport_carriers",
                column: "transport_operation_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_consignees_transport_operation_id",
                table: "transport_consignees",
                column: "transport_operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_consignment_items_transport_operation_id",
                table: "transport_consignment_items",
                column: "transport_operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_details_transport_operation_id",
                table: "transport_details",
                column: "transport_operation_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transport_operations_created_by_user_id",
                table: "transport_operations",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_operations_customer_id",
                table: "transport_operations",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_operations_destination_id",
                table: "transport_operations",
                column: "destination_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_operations_operation_code",
                table: "transport_operations",
                column: "operation_code");

            migrationBuilder.CreateIndex(
                name: "IX_transport_operations_source_id",
                table: "transport_operations",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_operations_updated_by_user_id",
                table: "transport_operations",
                column: "updated_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_transport_packages_consignment_item_id",
                table: "transport_packages",
                column: "consignment_item_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_keycloak_id",
                table: "users",
                column: "keycloak_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "efti_messages");

            migrationBuilder.DropTable(
                name: "transport_carriers");

            migrationBuilder.DropTable(
                name: "transport_consignees");

            migrationBuilder.DropTable(
                name: "transport_details");

            migrationBuilder.DropTable(
                name: "transport_packages");

            migrationBuilder.DropTable(
                name: "transport_consignment_items");

            migrationBuilder.DropTable(
                name: "transport_operations");

            migrationBuilder.DropTable(
                name: "customer_destinations");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "sources");
        }
    }
}
