CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` varchar(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` varchar(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

START TRANSACTION;
DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    ALTER DATABASE CHARACTER SET utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `sources` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `code` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `api_key_hash` varchar(64) CHARACTER SET utf8mb4 NULL,
        `is_active` tinyint(1) NOT NULL DEFAULT TRUE,
        `config_json` JSON NULL,
        `created_at` DATETIME NOT NULL,
        CONSTRAINT `PK_sources` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `users` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `username` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `email` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `full_name` varchar(200) CHARACTER SET utf8mb4 NULL,
        `is_active` tinyint(1) NOT NULL DEFAULT TRUE,
        `keycloak_id` varchar(100) CHARACTER SET utf8mb4 NULL,
        `roles_json` JSON NULL,
        `created_at` DATETIME NOT NULL,
        `last_login_at` DATETIME NULL,
        CONSTRAINT `PK_users` PRIMARY KEY (`id`)
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `customers` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `customer_code` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `business_name` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `vat_number` varchar(50) CHARACTER SET utf8mb4 NULL,
        `eori_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `contact_email` varchar(255) CHARACTER SET utf8mb4 NULL,
        `is_active` tinyint(1) NOT NULL DEFAULT TRUE,
        `auto_created` tinyint(1) NOT NULL DEFAULT FALSE,
        `source_id` CHAR(36) COLLATE ascii_general_ci NULL,
        `created_at` DATETIME NOT NULL,
        `updated_at` DATETIME NOT NULL,
        CONSTRAINT `PK_customers` PRIMARY KEY (`id`),
        CONSTRAINT `FK_customers_sources_source_id` FOREIGN KEY (`source_id`) REFERENCES `sources` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `audit_logs` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `entity_type` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `entity_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `action_type` longtext CHARACTER SET utf8mb4 NOT NULL,
        `performed_by_user_id` CHAR(36) COLLATE ascii_general_ci NULL,
        `performed_by_source_id` CHAR(36) COLLATE ascii_general_ci NULL,
        `description` varchar(500) CHARACTER SET utf8mb4 NOT NULL,
        `old_value_json` JSON NULL,
        `new_value_json` JSON NULL,
        `ip_address` varchar(45) CHARACTER SET utf8mb4 NULL,
        `user_agent` varchar(500) CHARACTER SET utf8mb4 NULL,
        `created_at` DATETIME NOT NULL,
        CONSTRAINT `PK_audit_logs` PRIMARY KEY (`id`),
        CONSTRAINT `FK_audit_logs_users_performed_by_user_id` FOREIGN KEY (`performed_by_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `customer_destinations` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `customer_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `destination_code` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `label` varchar(200) CHARACTER SET utf8mb4 NULL,
        `address_line1` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `city` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `postal_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `province` varchar(100) CHARACTER SET utf8mb4 NULL,
        `country_code` CHAR(2) CHARACTER SET utf8mb4 NOT NULL,
        `un_locode` varchar(10) CHARACTER SET utf8mb4 NULL,
        `is_default` tinyint(1) NOT NULL DEFAULT FALSE,
        `auto_created` tinyint(1) NOT NULL DEFAULT FALSE,
        `created_at` DATETIME NOT NULL,
        `updated_at` DATETIME NOT NULL,
        CONSTRAINT `PK_customer_destinations` PRIMARY KEY (`id`),
        CONSTRAINT `FK_customer_destinations_customers_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `transport_operations` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `source_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `customer_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `destination_id` CHAR(36) COLLATE ascii_general_ci NULL,
        `operation_code` varchar(100) CHARACTER SET utf8mb4 NOT NULL,
        `dataset_type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `status` longtext CHARACTER SET utf8mb4 NOT NULL,
        `hashcode` varchar(64) CHARACTER SET utf8mb4 NULL,
        `hashcode_algorithm` varchar(20) CHARACTER SET utf8mb4 NULL,
        `raw_payload_json` JSON NULL,
        `created_at` DATETIME NOT NULL,
        `updated_at` DATETIME NOT NULL,
        `created_by_user_id` CHAR(36) COLLATE ascii_general_ci NULL,
        `updated_by_user_id` CHAR(36) COLLATE ascii_general_ci NULL,
        CONSTRAINT `PK_transport_operations` PRIMARY KEY (`id`),
        CONSTRAINT `FK_transport_operations_customer_destinations_destination_id` FOREIGN KEY (`destination_id`) REFERENCES `customer_destinations` (`id`) ON DELETE SET NULL,
        CONSTRAINT `FK_transport_operations_customers_customer_id` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_transport_operations_sources_source_id` FOREIGN KEY (`source_id`) REFERENCES `sources` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_transport_operations_users_created_by_user_id` FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL,
        CONSTRAINT `FK_transport_operations_users_updated_by_user_id` FOREIGN KEY (`updated_by_user_id`) REFERENCES `users` (`id`) ON DELETE SET NULL
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `efti_messages` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `source_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `transport_operation_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `correlation_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `gateway_provider` longtext CHARACTER SET utf8mb4 NOT NULL,
        `direction` longtext CHARACTER SET utf8mb4 NOT NULL,
        `dataset_type` varchar(50) CHARACTER SET utf8mb4 NOT NULL,
        `status` varchar(255) CHARACTER SET utf8mb4 NOT NULL,
        `payload_json` JSON NOT NULL,
        `external_id` varchar(100) CHARACTER SET utf8mb4 NULL,
        `external_uuid` varchar(100) CHARACTER SET utf8mb4 NULL,
        `retry_count` smallint NOT NULL DEFAULT 0,
        `next_retry_at` DATETIME NULL,
        `sent_at` DATETIME NULL,
        `acknowledged_at` DATETIME NULL,
        `created_at` DATETIME NOT NULL,
        CONSTRAINT `PK_efti_messages` PRIMARY KEY (`id`),
        CONSTRAINT `FK_efti_messages_sources_source_id` FOREIGN KEY (`source_id`) REFERENCES `sources` (`id`) ON DELETE RESTRICT,
        CONSTRAINT `FK_efti_messages_transport_operations_transport_operation_id` FOREIGN KEY (`transport_operation_id`) REFERENCES `transport_operations` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `transport_carriers` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `transport_operation_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `sort_order` int NOT NULL,
        `name` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `player_type` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `street_name` varchar(300) CHARACTER SET utf8mb4 NULL,
        `post_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `city_name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `country_code` CHAR(2) CHARACTER SET utf8mb4 NOT NULL,
        `country_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        `tax_registration` varchar(100) CHARACTER SET utf8mb4 NULL,
        `eori_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `tractor_plate` varchar(20) CHARACTER SET utf8mb4 NOT NULL,
        `equipment_category` varchar(20) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_transport_carriers` PRIMARY KEY (`id`),
        CONSTRAINT `FK_transport_carriers_transport_operations_transport_operation_~` FOREIGN KEY (`transport_operation_id`) REFERENCES `transport_operations` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `transport_consignees` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `transport_operation_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `name` varchar(300) CHARACTER SET utf8mb4 NOT NULL,
        `player_type` varchar(30) CHARACTER SET utf8mb4 NOT NULL,
        `street_name` varchar(300) CHARACTER SET utf8mb4 NULL,
        `post_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `city_name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `country_code` CHAR(2) CHARACTER SET utf8mb4 NOT NULL,
        `country_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        `tax_registration` varchar(100) CHARACTER SET utf8mb4 NULL,
        `eori_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_transport_consignees` PRIMARY KEY (`id`),
        CONSTRAINT `FK_transport_consignees_transport_operations_transport_operatio~` FOREIGN KEY (`transport_operation_id`) REFERENCES `transport_operations` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `transport_consignment_items` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `transport_operation_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `total_item_quantity` int NOT NULL,
        `total_weight` DECIMAL(10,3) NOT NULL,
        `total_volume` DECIMAL(10,3) NULL,
        CONSTRAINT `PK_transport_consignment_items` PRIMARY KEY (`id`),
        CONSTRAINT `FK_transport_consignment_items_transport_operations_transport_o~` FOREIGN KEY (`transport_operation_id`) REFERENCES `transport_operations` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `transport_details` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `transport_operation_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `cargo_type` varchar(20) CHARACTER SET utf8mb4 NULL,
        `incoterms` varchar(5) CHARACTER SET utf8mb4 NULL,
        `acceptance_street_name` varchar(300) CHARACTER SET utf8mb4 NULL,
        `acceptance_post_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `acceptance_city_name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `acceptance_country_code` CHAR(2) CHARACTER SET utf8mb4 NOT NULL,
        `acceptance_country_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        `acceptance_date` DATETIME NULL,
        `receipt_street_name` varchar(300) CHARACTER SET utf8mb4 NULL,
        `receipt_post_code` varchar(20) CHARACTER SET utf8mb4 NULL,
        `receipt_city_name` varchar(200) CHARACTER SET utf8mb4 NOT NULL,
        `receipt_country_code` CHAR(2) CHARACTER SET utf8mb4 NOT NULL,
        `receipt_country_name` varchar(100) CHARACTER SET utf8mb4 NULL,
        CONSTRAINT `PK_transport_details` PRIMARY KEY (`id`),
        CONSTRAINT `FK_transport_details_transport_operations_transport_operation_id` FOREIGN KEY (`transport_operation_id`) REFERENCES `transport_operations` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE TABLE `transport_packages` (
        `id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `consignment_item_id` CHAR(36) COLLATE ascii_general_ci NOT NULL,
        `sort_order` int NOT NULL,
        `shipping_marks` varchar(100) CHARACTER SET utf8mb4 NULL,
        `item_quantity` int NOT NULL,
        `type_code` varchar(50) CHARACTER SET utf8mb4 NULL,
        `gross_weight` DECIMAL(10,3) NOT NULL,
        `gross_volume` DECIMAL(10,3) NULL,
        CONSTRAINT `PK_transport_packages` PRIMARY KEY (`id`),
        CONSTRAINT `FK_transport_packages_transport_consignment_items_consignment_i~` FOREIGN KEY (`consignment_item_id`) REFERENCES `transport_consignment_items` (`id`) ON DELETE CASCADE
    ) CHARACTER SET=utf8mb4;

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_audit_logs_created_at` ON `audit_logs` (`created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_audit_logs_entity_id` ON `audit_logs` (`entity_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_audit_logs_entity_type` ON `audit_logs` (`entity_type`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_audit_logs_entity_type_entity_id_created_at` ON `audit_logs` (`entity_type`, `entity_id`, `created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_audit_logs_performed_by_user_id` ON `audit_logs` (`performed_by_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_customer_destinations_customer_id` ON `customer_destinations` (`customer_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_customer_destinations_destination_code` ON `customer_destinations` (`destination_code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_customers_customer_code` ON `customers` (`customer_code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_customers_source_id` ON `customers` (`source_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_correlation_id` ON `efti_messages` (`correlation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_created_at` ON `efti_messages` (`created_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_external_id` ON `efti_messages` (`external_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_next_retry_at` ON `efti_messages` (`next_retry_at`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_source_id` ON `efti_messages` (`source_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_status` ON `efti_messages` (`status`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_efti_messages_transport_operation_id` ON `efti_messages` (`transport_operation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_sources_code` ON `sources` (`code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_carriers_transport_operation_id` ON `transport_carriers` (`transport_operation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_transport_consignees_transport_operation_id` ON `transport_consignees` (`transport_operation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_transport_consignment_items_transport_operation_id` ON `transport_consignment_items` (`transport_operation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_transport_details_transport_operation_id` ON `transport_details` (`transport_operation_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_operations_created_by_user_id` ON `transport_operations` (`created_by_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_operations_customer_id` ON `transport_operations` (`customer_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_operations_destination_id` ON `transport_operations` (`destination_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_operations_operation_code` ON `transport_operations` (`operation_code`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_operations_source_id` ON `transport_operations` (`source_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_operations_updated_by_user_id` ON `transport_operations` (`updated_by_user_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_transport_packages_consignment_item_id` ON `transport_packages` (`consignment_item_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_users_email` ON `users` (`email`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE INDEX `IX_users_keycloak_id` ON `users` (`keycloak_id`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    CREATE UNIQUE INDEX `IX_users_username` ON `users` (`username`);

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302114905_InitialCreate') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260302114905_InitialCreate', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302115733_SeedTestSource') THEN

    INSERT INTO `sources` (`id`, `api_key_hash`, `code`, `config_json`, `created_at`, `is_active`, `name`, `type`)
    VALUES ('11111111-1111-1111-1111-111111111111', NULL, 'TMS_TEST', NULL, TIMESTAMP '2026-01-01 00:00:00', TRUE, 'Sorgente di Test', 'TMS');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260302115733_SeedTestSource') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260302115733_SeedTestSource', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303122316_SeedFrontendSource') THEN

    INSERT IGNORE INTO sources (id, api_key_hash, code, config_json, created_at, is_active, name, type)
    VALUES ('22222222-2222-2222-2222-222222222222', NULL, 'TEST_FRONTEND', NULL, '2026-01-01 00:00:00', 1, 'Form Frontend Manuale', 'FORM');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

DROP PROCEDURE IF EXISTS MigrationsScript;
DELIMITER //
CREATE PROCEDURE MigrationsScript()
BEGIN
    IF NOT EXISTS(SELECT 1 FROM `__EFMigrationsHistory` WHERE `MigrationId` = '20260303122316_SeedFrontendSource') THEN

    INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
    VALUES ('20260303122316_SeedFrontendSource', '9.0.0');

    END IF;
END //
DELIMITER ;
CALL MigrationsScript();
DROP PROCEDURE MigrationsScript;

COMMIT;

