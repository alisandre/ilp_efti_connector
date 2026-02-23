# EFTI Connector Platform — Documentazione Architetturale

> **Versione:** 2.0 · **Data:** Febbraio 2026  
> Documento di riferimento architetturale per il progetto **EFTI Connector Hub**.

---

## Indice

1. [Introduzione e Contesto](#1-introduzione-e-contesto)
2. [Strategia di Integrazione a Due Fasi](#2-strategia-di-integrazione-a-due-fasi)
3. [Stack Tecnologico](#3-stack-tecnologico)
4. [Diagramma del Database](#4-diagramma-del-database)
   - 4.1 `customers` · 4.2 `customer_destinations` · 4.3 `sources` · 4.4 `transport_operations`
   - 4.5 `efti_messages` · 4.6 `transport_consignees` · 4.7 `transport_carriers`
   - 4.8 `transport_details` · 4.9 `transport_consignment_items` · 4.10 `transport_packages`
   - 4.11 `users` · 4.12 `audit_logs` · 4.13 Relazioni ER
5. [Architettura a Microservizi](#5-architettura-a-microservizi)
6. [Fase 1 — Integrazione con MILOS TFP](#6-fase-1--integrazione-con-milos-tfp)
7. [Fase 2 — Integrazione Diretta con EFTI](#7-fase-2--integrazione-diretta-con-efti)
8. [Frontend React](#8-frontend-react)
9. [Sicurezza e Conformità Normativa](#9-sicurezza-e-conformità-normativa)
10. [Deployment e Scalabilità](#10-deployment-e-scalabilità)

---

## 1. Introduzione e Contesto

**EFTI** (Electronic Freight Transport Information) è il framework normativo europeo (Regolamento EU 2020/1056) che impone la digitalizzazione dei documenti di trasporto merci. La piattaforma EFTI consente alle autorità di controllo di accedere elettronicamente alle informazioni di trasporto (eCMR, eAWB, eBL, eRSD, eDAD) in tempo reale.

L'applicazione descritta in questo documento è un **EFTI Connector Hub**: un componente middleware multi-sorgente che funge da gateway centralizzato verso la piattaforma EFTI europea, permettendo a diversi moduli aziendali (TMS, WMS, ERP, dogana, vettori) di comunicare in modo uniforme con EFTI.

### 1.1 Obiettivi Principali

- Centralizzare la comunicazione verso EFTI evitando N integrazioni punto-a-punto
- Supportare più sorgenti dati (moduli interni, sistemi terzi, operatori umani via form)
- Mantenere un'anagrafica interna di clienti (mittenti) e relative destinazioni, aggiornata automaticamente tramite il codice cliente ricevuto con ogni ordine
- Garantire conformità ai dataset EFTI (eFTI dataset secondo EN 17532)
- Offrire audit completo, retry automatico e monitoraggio dei messaggi
- Fornire un'interfaccia React per inserimento manuale e consultazione

---

## 2. Strategia di Integrazione a Due Fasi

L'integrazione verso EFTI viene realizzata in **due fasi successive** per ridurre il rischio tecnico e accelerare il time-to-market.

### 2.1 Fase 1 — Integrazione via MILOS TFP (MVP)

**MILOS TFP** (Circle SpA) è una piattaforma certificata che funge da intermediario verso l'EFTI Gate nazionale. Il Connector invia i dati a MILOS tramite API REST, e MILOS gestisce autonomamente tutta la comunicazione con l'infrastruttura EFTI (dataset EN 17532, eFTI Gate, QR Code per controlli su strada).

**Vantaggi:**
- Nessun onere di certificazione X.509/AS4 nella fase iniziale
- API REST semplice e documentata (Circle SpA - ICD v1.0)
- Time-to-market ridotto: il Connector non gestisce i protocolli EFTI nativi
- Ambiente di test MILOS disponibile per validazione end-to-end

### 2.2 Fase 2 — Integrazione Diretta con EFTI Gate (Evolutiva)

Il Connector acquisisce la capacità di comunicare **direttamente con l'EFTI Gate nazionale** senza dipendere da MILOS. Il layer di astrazione introdotto nella Fase 1 consente di aggiungere questo canale senza impattare il resto del sistema.

**Caratteristiche:**
- Indipendenza da intermediari terzi e relativi costi
- Controllo diretto su SLA e resilienza EFTI
- Supporto protocolli nativi: REST HTTPS, eDelivery AS4, OAuth2 + X.509
- MILOS rimane disponibile come canale di fallback opzionale

### 2.3 Layer di Astrazione (`IEftiGateway`)

La chiave dell'architettura bifase è l'interfaccia `IEftiGateway` nel Gateway Service, che nasconde quale provider sia attivo:

```csharp
// Interfaccia comune — invariata tra Fase 1 e Fase 2
public interface IEftiGateway
{
    Task<EftiSendResult> SendEcmrAsync(EcmrPayload payload, CancellationToken ct);
    Task<EftiSendResult> UpdateEcmrAsync(string ecmrId, EcmrPayload payload, CancellationToken ct);
    Task<EftiSendResult> DeleteEcmrAsync(string ecmrId, CancellationToken ct);
    Task<EcmrPayload>    GetEcmrAsync(string ecmrId, CancellationToken ct);
}

// Fase 1: implementazione via MILOS REST API
public class MilosTfpGateway : IEftiGateway { ... }

// Fase 2: implementazione diretta verso EFTI Gate
public class EftiNativeGateway : IEftiGateway { ... }
```

La selezione avviene tramite feature flag in `appsettings.json`, senza deploy:

```json
{
  "EftiGateway": {
    "Provider": "Milos",
    "Milos": {
      "BaseUrl": "https://<server>/api/ecmr-service/",
      "ApiKey": "<from-vault>"
    },
    "EftiNative": {
      "BaseUrl": "https://efti-gate.example.eu/",
      "ClientId": "<oauth2-client-id>",
      "CertificatePath": "<from-vault>"
    }
  }
}
```

---

## 3. Stack Tecnologico

### 3.1 Backend — C# / .NET

| Componente | Tecnologia | Ruolo |
|---|---|---|
| Runtime | .NET 9 | Piattaforma di esecuzione principale |
| Framework API | ASP.NET Core 9 Web API | Esposizione endpoint REST |
| Messaggistica | MassTransit + RabbitMQ | Bus eventi interno tra microservizi |
| ORM | Entity Framework Core 9 + Pomelo.EntityFrameworkCore.MySql | Accesso dati su MariaDB |
| Auth | Keycloak + JWT/OAuth2 | Identity provider, RBAC |
| HTTP Client | Refit + Polly | Client tipizzato verso MILOS (F1) e EFTI Gate (F2) |
| Validazione | FluentValidation | Validazione payload MILOS e EN 17532 |
| Background Jobs | Hangfire | Retry asincrono, scheduling |
| Logging | Serilog + Seq | Logging strutturato centralizzato |
| Testing | xUnit + Moq + Testcontainers | Unit, integration e contract test |

### 3.2 Frontend — React

| Componente | Tecnologia | Ruolo |
|---|---|---|
| Framework | React 19 + TypeScript | SPA principale |
| Build Tool | Vite | Bundler e dev server |
| UI Library | Ant Design 5 / MUI | Componenti form, tabelle, layout |
| State Management | Zustand + React Query | State globale + caching API |
| Form Handling | React Hook Form + Zod | Validazione schema-first |
| HTTP | Axios + OpenAPI codegen | Client auto-generato da spec OpenAPI |
| Auth | OIDC Client (Keycloak) | SSO con il backend Keycloak |
| Tabelle/Grid | TanStack Table v8 | Griglie dati con filtri e paginazione |

### 3.3 Database e Storage

| Sistema | Tecnologia | Utilizzo |
|---|---|---|
| DB Principale | MariaDB 11.4 LTS | Dati transazionali, messaggi, audit log |
| Cache | Redis 7 | Cache token, sessioni, rate limiting |
| Message Broker | RabbitMQ 3.13 | Code messaggi inter-servizio |
| Object Storage | MinIO / Azure Blob | Allegati documenti di trasporto |

---

## 4. Diagramma del Database

Il database **MariaDB 11.4** è strutturato in **12 tabelle**. Lo schema è **invariato tra Fase 1 e Fase 2**: cambia solo l'implementazione del gateway esterno, non il modello dati interno.

---

### 4.1 Tabella: `customers`

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID interno |
| `customer_code` | `VARCHAR(100)` | UNIQUE NOT NULL | Codice cliente dal sorgente — chiave di lookup |
| `business_name` | `VARCHAR(300)` | NOT NULL | Ragione sociale del mittente |
| `vat_number` | `VARCHAR(50)` | | Partita IVA / VAT number |
| `eori_code` | `VARCHAR(20)` | | Codice EORI (obbligatorio EFTI cross-border) |
| `contact_email` | `VARCHAR(255)` | | Email operativa |
| `is_active` | `BOOLEAN` | DEFAULT true | Cliente abilitato |
| `auto_created` | `BOOLEAN` | DEFAULT false | `true` = creato automaticamente da ordine |
| `source_id` | `CHAR(36)` | FK `sources` | Sorgente che ha generato la creazione automatica |
| `created_at` | `DATETIME` | NOT NULL | Prima creazione |
| `updated_at` | `DATETIME` | NOT NULL | Ultimo aggiornamento |

---

### 4.2 Tabella: `customer_destinations`

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID interno |
| `customer_id` | `CHAR(36)` | FK `customers` NOT NULL | Cliente proprietario |
| `destination_code` | `VARCHAR(100)` | UNIQUE NOT NULL | Codice destinazione dal sorgente |
| `label` | `VARCHAR(200)` | | Etichetta leggibile |
| `address_line1` | `VARCHAR(300)` | NOT NULL | Via e numero civico |
| `city` | `VARCHAR(200)` | NOT NULL | Città |
| `postal_code` | `VARCHAR(20)` | | CAP |
| `province` | `VARCHAR(100)` | | Provincia / Regione |
| `country_code` | `CHAR(2)` | NOT NULL | ISO 3166-1 alpha-2 |
| `un_locode` | `VARCHAR(10)` | | Codice UN/LOCODE |
| `is_default` | `BOOLEAN` | DEFAULT false | Destinazione predefinita |
| `auto_created` | `BOOLEAN` | DEFAULT false | `true` = creata automaticamente |
| `created_at` | `DATETIME` | NOT NULL | Data creazione |
| `updated_at` | `DATETIME` | NOT NULL | Ultimo aggiornamento |

**Logica di Upsert Cliente/Destinazione** (identica in Fase 1 e Fase 2):

```
1. LOOKUP CLIENTE
   Cerca customers WHERE customer_code = :code
   → Trovato   : UPDATE business_name, vat_number, eori_code se cambiati
   → Non trovato: INSERT con auto_created = true

2. LOOKUP DESTINAZIONE
   Cerca customer_destinations WHERE destination_code = :dest_code
   → Trovata   : UPDATE address se cambiato
   → Non trovata: INSERT con auto_created = true

3. POPOLAMENTO PAYLOAD
   FASE 1 → consignorSender (ECMRRequest MILOS) popolato da customers + customer_destinations
   FASE 2 → Consignor (dataset EN 17532)       popolato dagli stessi dati
```

---

### 4.3 Tabella: `sources`

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | Identificatore univoco sorgente |
| `code` | `VARCHAR(50)` | UNIQUE NOT NULL | Codice breve (es. `TMS_ACME`) |
| `name` | `VARCHAR(200)` | NOT NULL | Nome descrittivo |
| `type` | `VARCHAR(20)` | NOT NULL | `TMS` \| `WMS` \| `ERP` \| `CUSTOMS` \| `MANUAL` |
| `api_key_hash` | `VARCHAR(64)` | | Hash SHA-256 API key |
| `is_active` | `BOOLEAN` | DEFAULT true | Abilitazione sorgente |
| `config_json` | `JSON` | | Configurazione specifica (mapping, webhook, ecc.) |
| `created_at` | `DATETIME` | NOT NULL | Data creazione |

---

### 4.4 Tabella: `transport_operations`

Nucleo del sistema. Ogni operazione di trasporto è collegata a un cliente, una destinazione e una sorgente. I dati del payload sono normalizzati nelle tabelle figlio (4.6–4.10); la colonna `raw_payload_json` conserva lo snapshot per debug.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID operazione |
| `source_id` | `CHAR(36)` | FK `sources` NOT NULL | Sorgente che ha creato l'operazione |
| `customer_id` | `CHAR(36)` | FK `customers` NOT NULL | Cliente/mittente |
| `destination_id` | `CHAR(36)` | FK `customer_destinations` | Destinazione di consegna |
| `operation_code` | `VARCHAR(100)` | NOT NULL INDEXED | Codice CMR/DDT — corrisponde a `eCMRID` MILOS |
| `dataset_type` | `VARCHAR(50)` | NOT NULL | `ECMR` \| `EDDT` |
| `status` | `VARCHAR(30)` | NOT NULL | Vedi enum `TransportOperationStatus` |
| `hashcode` | `VARCHAR(64)` | | Hash SHA-256 del payload (hashcodeDetails MILOS) |
| `hashcode_algorithm` | `VARCHAR(20)` | | Es. `SHA-256` |
| `raw_payload_json` | `JSON` | | Snapshot del payload al momento dell'invio — solo debug |
| `created_at` | `DATETIME` | NOT NULL | Data creazione |
| `updated_at` | `DATETIME` | NOT NULL | Ultimo aggiornamento |
| `created_by_user_id` | `CHAR(36)` | FK `users` | Utente che ha creato l'operazione (form manuale) |
| `updated_by_user_id` | `CHAR(36)` | FK `users` | Utente che ha effettuato l'ultimo aggiornamento |

**Enum `TransportOperationStatus`:** `DRAFT`, `PENDING_VALIDATION`, `VALIDATED`, `SENDING`, `SENT`, `ACKNOWLEDGED`, `ERROR`, `CANCELLED`

---

### 4.5 Tabella: `efti_messages`

Generica e invariata tra Fase 1 e Fase 2. La colonna `gateway_provider` traccia quale provider ha gestito il messaggio.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | ID messaggio interno |
| `source_id` | `CHAR(36)` | FK `sources` NOT NULL | Sorgente originaria |
| `transport_operation_id` | `CHAR(36)` | FK `transport_operations` NOT NULL | Operazione collegata |
| `correlation_id` | `CHAR(36)` | NOT NULL INDEXED | ID correlazione end-to-end |
| `gateway_provider` | `VARCHAR(20)` | NOT NULL | `MILOS` \| `EFTI_NATIVE` |
| `direction` | `VARCHAR(10)` | NOT NULL | `INBOUND` \| `OUTBOUND` |
| `dataset_type` | `VARCHAR(50)` | NOT NULL | `eCMR`, `eDDT`, `eAWB`, `eRSD`, `eBL`, `eDAD` |
| `status` | `VARCHAR(20)` | NOT NULL INDEXED | `PENDING`, `SENT`, `ACKNOWLEDGED`, `ERROR`, `RETRY`, `DEAD` |
| `payload_json` | `JSON` | NOT NULL | Payload effettivamente trasmesso al gateway |
| `external_id` | `VARCHAR(100)` | INDEXED | F1: `eCMRID` MILOS — F2: `messageId` EFTI |
| `external_uuid` | `VARCHAR(100)` | | F1: `uuid` dalla ECMRResponse MILOS |
| `retry_count` | `SMALLINT` | DEFAULT 0 | Numero tentativi effettuati |
| `next_retry_at` | `DATETIME` | INDEXED | Prossimo tentativo (backoff esponenziale) |
| `sent_at` | `DATETIME` | | Timestamp invio al gateway |
| `acknowledged_at` | `DATETIME` | | Timestamp ACK ricevuto |
| `created_at` | `DATETIME` | NOT NULL INDEXED | Timestamp creazione |

---

### 4.6 Tabella: `transport_consignees`

Destinatario dell'operazione di trasporto (relazione **1:1** con `transport_operations`). Corrisponde al campo `consignee` di `ECMRRequest` MILOS.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID |
| `transport_operation_id` | `CHAR(36)` | FK `transport_operations` UNIQUE NOT NULL | Operazione proprietaria |
| `name` | `VARCHAR(300)` | NOT NULL | Ragione sociale del destinatario |
| `player_type` | `VARCHAR(30)` | NOT NULL | Tipicamente `CONSIGNEE` |
| `street_name` | `VARCHAR(300)` | | Via e numero civico |
| `post_code` | `VARCHAR(20)` | | Codice postale |
| `city_name` | `VARCHAR(200)` | NOT NULL | Città |
| `country_code` | `CHAR(2)` | NOT NULL | ISO 3166-1 alpha-2 |
| `country_name` | `VARCHAR(100)` | | Nome paese per esteso |
| `tax_registration` | `VARCHAR(100)` | | Partita IVA / numero fiscale |
| `eori_code` | `VARCHAR(20)` | | Codice EORI |

**Enum `PlayerType`:** `CONSIGNOR_SENDER`, `SELLER`, `CONSIGNEE`, `CARRIER`, `FREIGHT_FORWARDER`

---

### 4.7 Tabella: `transport_carriers`

Vettori/trasportatori dell'operazione (relazione **1:N** con `transport_operations`). Corrisponde all'array `carriers[]` di `ECMRRequest` MILOS. L'ordine è preservato da `sort_order`.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID |
| `transport_operation_id` | `CHAR(36)` | FK `transport_operations` NOT NULL | Operazione proprietaria |
| `sort_order` | `INT` | NOT NULL | Ordine nell'array dei vettori (1-based) |
| `name` | `VARCHAR(300)` | NOT NULL | Ragione sociale del vettore |
| `player_type` | `VARCHAR(30)` | NOT NULL | Tipicamente `CARRIER` |
| `street_name` | `VARCHAR(300)` | | Via e numero civico |
| `post_code` | `VARCHAR(20)` | | Codice postale |
| `city_name` | `VARCHAR(200)` | NOT NULL | Città |
| `country_code` | `CHAR(2)` | NOT NULL | ISO 3166-1 alpha-2 |
| `country_name` | `VARCHAR(100)` | | Nome paese per esteso |
| `tax_registration` | `VARCHAR(100)` | | Partita IVA / numero fiscale |
| `eori_code` | `VARCHAR(20)` | | Codice EORI |
| `tractor_plate` | `VARCHAR(20)` | NOT NULL | Targa del trattore/mezzo (`tractorPlate` MILOS) |
| `equipment_category` | `VARCHAR(20)` | | Categoria mezzo (vedi enum `EcmrEquipmentCategory`) |

**Enum `EcmrEquipmentCategory`:** `CONTAINER`, `SEMITRAILER`, `TRAILER`, `SWAP_BODY`, `TANK`, `FLAT_RACK`, `VAN`, `REEFER`, `OPEN_TOP`, `BULK`, `SILO`, `VEHICLE_CARRIER`, `OTHER`

---

### 4.8 Tabella: `transport_details`

Dettagli di trasporto: incoterms, cargo type e luoghi contrattuali (relazione **1:1** con `transport_operations`). Corrisponde a `transportDetails`, `contractualCarrierAcceptanceLocation` e `contractualConsigneeReceiptLocation` di `ECMRRequest` MILOS.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID |
| `transport_operation_id` | `CHAR(36)` | FK `transport_operations` UNIQUE NOT NULL | Operazione proprietaria |
| `cargo_type` | `VARCHAR(20)` | | `FTL` \| `LTL` \| `GROUPAGE` |
| `incoterms` | `VARCHAR(5)` | | `EXW`, `FCA`, `CPT`, `CIP`, `DAT`, `DAP`, `DDP`, `FAS`, `FOB`, `CFR`, `CIF` |
| `acceptance_street_name` | `VARCHAR(300)` | | Via — luogo presa in carico vettore |
| `acceptance_post_code` | `VARCHAR(20)` | | CAP — luogo presa in carico |
| `acceptance_city_name` | `VARCHAR(200)` | | Città — luogo presa in carico |
| `acceptance_country_code` | `CHAR(2)` | | Paese — luogo presa in carico |
| `acceptance_country_name` | `VARCHAR(100)` | | Nome paese — luogo presa in carico |
| `acceptance_date` | `DATETIME` | | Data/ora contrattuale presa in carico |
| `receipt_street_name` | `VARCHAR(300)` | | Via — luogo consegna destinatario |
| `receipt_post_code` | `VARCHAR(20)` | | CAP — luogo consegna |
| `receipt_city_name` | `VARCHAR(200)` | | Città — luogo consegna |
| `receipt_country_code` | `CHAR(2)` | | Paese — luogo consegna |
| `receipt_country_name` | `VARCHAR(100)` | | Nome paese — luogo consegna |

---

### 4.9 Tabella: `transport_consignment_items`

Totali della spedizione (relazione **1:1** con `transport_operations`). Corrisponde all'oggetto `includedConsignmentItems` di `ECMRRequest` MILOS.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID |
| `transport_operation_id` | `CHAR(36)` | FK `transport_operations` UNIQUE NOT NULL | Operazione proprietaria |
| `total_item_quantity` | `INT` | NOT NULL | Numero totale colli (`totalItemQuantity` MILOS) |
| `total_weight` | `DECIMAL(10,3)` | NOT NULL | Peso lordo totale in kg (`totalWeight` MILOS) |
| `total_volume` | `DECIMAL(10,3)` | | Volume totale in m³ (`totalVolume` MILOS) |

---

### 4.10 Tabella: `transport_packages`

Singoli colli della spedizione (relazione **1:N** con `transport_consignment_items`). Corrisponde all'array `transportPackages[]` all'interno di `includedConsignmentItems` MILOS.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID |
| `consignment_item_id` | `CHAR(36)` | FK `transport_consignment_items` NOT NULL | Spedizione proprietaria |
| `sort_order` | `INT` | NOT NULL | Ordine nell'array dei colli (1-based) |
| `shipping_marks` | `VARCHAR(100)` | | Marcatura/codice collo (`shippingMarks` MILOS) |
| `item_quantity` | `INT` | NOT NULL | Numero unità nel collo (`itemQuantity` MILOS) |
| `type_code` | `VARCHAR(50)` | | Tipo imballaggio (es. `PALLET`, `BOX`) |
| `gross_weight` | `DECIMAL(10,3)` | NOT NULL | Peso lordo in kg (`grossWeight` MILOS) |
| `gross_volume` | `DECIMAL(10,3)` | | Volume lordo in m³ (`grossVolume` MILOS) |

---

### 4.11 Tabella: `users`

Utenti del sistema, sincronizzati con Keycloak. Tracciati come autori di operazioni manuali e nei log di audit.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID interno |
| `username` | `VARCHAR(100)` | UNIQUE NOT NULL | Username univoco |
| `email` | `VARCHAR(255)` | UNIQUE NOT NULL | Email dell'utente |
| `full_name` | `VARCHAR(200)` | | Nome completo |
| `is_active` | `BOOLEAN` | DEFAULT true | Utente abilitato |
| `keycloak_id` | `VARCHAR(100)` | INDEXED | Subject ID Keycloak (OIDC `sub`) |
| `roles_json` | `JSON` | | Ruoli RBAC in formato JSON |
| `created_at` | `DATETIME` | NOT NULL | Data creazione |
| `last_login_at` | `DATETIME` | | Data dell'ultimo accesso |

---

### 4.12 Tabella: `audit_logs`

Log di audit **immutabile** per conformità GDPR (Art. 5 Reg. EU 2020/1056). Traccia ogni azione su ogni entità del sistema.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID |
| `entity_type` | `VARCHAR(50)` | NOT NULL INDEXED | Entità soggetta all'azione (vedi enum) |
| `entity_id` | `CHAR(36)` | NOT NULL INDEXED | ID dell'entità specifica |
| `action_type` | `VARCHAR(20)` | NOT NULL | `Create`, `Read`, `Update`, `Delete`, `Send`, `Receive`, `Query`, `Export` |
| `performed_by_user_id` | `CHAR(36)` | FK `users` INDEXED | Utente che ha eseguito l'azione |
| `performed_by_source_id` | `CHAR(36)` | | Sorgente automatica che ha eseguito l'azione |
| `description` | `VARCHAR(500)` | NOT NULL | Descrizione testuale dell'azione |
| `old_value_json` | `JSON` | | Stato dell'entità prima della modifica |
| `new_value_json` | `JSON` | | Stato dell'entità dopo la modifica |
| `ip_address` | `VARCHAR(45)` | | Indirizzo IP (IPv4/IPv6) del client |
| `user_agent` | `VARCHAR(500)` | | User Agent del client |
| `created_at` | `DATETIME` | NOT NULL INDEXED | Timestamp azione |

**Indice composito:** `(entity_type, entity_id, created_at)` — ottimizza le ricerche per entità nel tempo.

**Enum `AuditEntityType`:** `Customer`, `CustomerDestination`, `Source`, `TransportOperation`, `EftiMessage`, `User`

---

### 4.13 Relazioni ER

```
sources (1) ──────── (N) customers                [FK: source_id, ON DELETE SET NULL]
sources (1) ──────── (N) transport_operations     [FK: source_id, ON DELETE RESTRICT]
sources (1) ──────── (N) efti_messages            [FK: source_id, ON DELETE RESTRICT]

customers (1) ─────── (N) customer_destinations   [FK: customer_id, ON DELETE CASCADE]
customers (1) ─────── (N) transport_operations    [FK: customer_id, ON DELETE RESTRICT]

customer_destinations (1) ── (N) transport_operations  [FK: destination_id, ON DELETE SET NULL]

users (1) ────────── (N) transport_operations     [FK: created_by_user_id, ON DELETE SET NULL]
users (1) ────────── (N) transport_operations     [FK: updated_by_user_id, ON DELETE SET NULL]
users (1) ────────── (N) audit_logs               [FK: performed_by_user_id, ON DELETE SET NULL]

transport_operations (1) ─── (N) efti_messages         [FK: transport_operation_id, ON DELETE CASCADE]
transport_operations (1) ─── (1) transport_consignees  [FK: transport_operation_id, ON DELETE CASCADE]
transport_operations (1) ─── (N) transport_carriers    [FK: transport_operation_id, ON DELETE CASCADE]
transport_operations (1) ─── (1) transport_details     [FK: transport_operation_id, ON DELETE CASCADE]
transport_operations (1) ─── (1) transport_consignment_items [FK: transport_operation_id, ON DELETE CASCADE]

transport_consignment_items (1) ── (N) transport_packages [FK: consignment_item_id, ON DELETE CASCADE]
```

---

## 5. Architettura a Microservizi

L'architettura è **identica nelle due fasi**. L'unico componente che cambia internamente è l'**EFTI Gateway Service**.

### 5.1 Microservizi di Input

**API Gateway / Source Ingestion Service**: punto di ingresso unico, autenticazione API Key/JWT, pubblica `TransportSubmitted` su RabbitMQ, rate limiting Redis per `source_id`.

**Validation Service**: valida il payload ricevuto. In Fase 1 valida la struttura ECMRRequest MILOS (campi obbligatori, codici paese ISO); in Fase 2 valida il dataset EN 17532 completo (EORI, UN LOCODE, semantica eFTI). Pubblica `TransportValidated` o `ValidationFailed`.

**Normalization / Mapping Service**: esegue l'upsert cliente/destinazione (stesso codice in entrambe le fasi), poi mappa verso il formato del gateway attivo — `ECMRRequest` MILOS in Fase 1, dataset EN 17532 in Fase 2. Crea i record `efti_messages` e `transport_operations`.

**Form Input Service (UI Backend)**: CRUD transport\_operations, autosave bozze, selezione cliente per codice, JWT Keycloak + RBAC.

### 5.2 Microservizi di Output

**EFTI Gateway Service** *(componente chiave della bifase)*: seleziona l'implementazione attiva tramite `IEftiGateway`. In Fase 1 chiama `POST/PUT/DELETE <server>/api/ecmr-service/ecmr` verso MILOS; in Fase 2 chiama l'EFTI Gate con OAuth2 + X.509. Resilienza Polly: retry esponenziale (3 tentativi), circuit breaker, timeout 30s. Salva `external_id` e `external_uuid` dalla risposta.

**Response Handler Service**: elabora le risposte del gateway. In Fase 1 elabora `ECMRResponse` (eCMRID + uuid) da MILOS; in Fase 2 elabora ACK/NACK dall'EFTI Gate. Pubblica `SourceNotificationRequired`.

**Notification / Webhook Service**: notifica i moduli sorgente via webhook HTTP, SSE per UI React, Hangfire retry max 24h. Invariato nelle due fasi.

**Query Proxy Service**: in Fase 1 non applicabile (MILOS gestisce le query delle autorità e i QR Code autonomamente). In Fase 2 gestisce le query delle autorità verso EFTI con cache Redis e audit GDPR obbligatorio.

**Retry & Dead Letter Service**: backoff esponenziale 1m → 5m → 15m → 1h → 6h → 24h, max 6 tentativi, poi DLQ con alert e dashboard. Invariato nelle due fasi.

---

## 6. Fase 1 — Integrazione con MILOS TFP

### 6.1 Overview MILOS TFP (Circle SpA)

MILOS TFP integra due servizi:

- **e-CMR Service**: crea e gestisce la lettera di vettura elettronica (e-CMR/e-DDT), gestisce il ciclo di vita e coordina le firme digitali (integrazione con Accudire per validità legale)
- **eFTI Platform**: riceve i dati dall'e-CMR Service, genera il dataset eFTI, comunica con l'eFTI Gate nazionale e genera il **QR Code univoco** per controlli su strada

Il Connector interagisce **esclusivamente con l'e-CMR Service** tramite REST. La propagazione verso l'eFTI Gate è automatica lato MILOS.

### 6.2 API MILOS — E-CMR Service

**Base URL**: `<server>/api/ecmr-service/`

| Funzione | Metodo | Endpoint | Request | Response |
|---|---|---|---|---|
| Creazione e-CMR/e-DDT | `POST` | `ecmr` | `ECMRRequest` | `ECMRResponse` |
| Modifica e-CMR/e-DDT | `PUT` | `ecmr/{id}` | `ECMRRequest` | `OK/KO` |
| Cancellazione e-CMR/e-DDT | `DELETE` | `ecmr/{id}` | Id e-CMR | `OK/KO` |
| Get e-CMR/e-DDT | `GET` | `ecmr/get/{id}` | Id e-CMR | `ECMRRequest` |

### 6.3 Data Model MILOS

**ECMRRequest** — payload principale inviato al Connector via MILOS:

| Campo | Tipo | Mapping anagrafica interna |
|---|---|---|
| `shipping.eCMRID` | `String` | `transport_operations.operation_code` |
| `shipping.datasetType` | `ECMR` \| `EDDT` | `transport_operations.dataset_type` |
| `consignorSender.name` | `String` | `customers.business_name` |
| `consignorSender.postalAddress` | `PostalAddress` | `customer_destinations.*` |
| `consignorSender.taxRegistration` | `String` | `customers.vat_number` |
| `consignorSender.EORICode` | `String` | `customers.eori_code` |
| `consignee.name` | `String` | `transport_consignees.name` |
| `consignee.type` | `PlayerType` | `transport_consignees.player_type` |
| `consignee.postalAddress` | `PostalAddress` | `transport_consignees.city_name`, `country_code`, … |
| `consignee.taxRegistration` | `String` | `transport_consignees.tax_registration` |
| `consignee.EORICode` | `String` | `transport_consignees.eori_code` |
| `carriers[].name` | `String` | `transport_carriers.name` (ordinati per `sort_order`) |
| `carriers[].type` | `PlayerType` | `transport_carriers.player_type` |
| `carriers[].postalAddress` | `PostalAddress` | `transport_carriers.city_name`, `country_code`, … |
| `carriers[].taxRegistration` | `String` | `transport_carriers.tax_registration` |
| `carriers[].tractorPlate` | `String` | `transport_carriers.tractor_plate` |
| `carriers[].equipmentCategory` | `EcmrEquipmentCategory` | `transport_carriers.equipment_category` |
| `contractualCarrierAcceptanceLocation.postalAddress` | `PostalAddress` | `transport_details.acceptance_city_name`, `acceptance_country_code`, … |
| `contractualCarrierAcceptanceLocation.date` | `String` | `transport_details.acceptance_date` |
| `contractualConsigneeReceiptLocation.postalAddress` | `PostalAddress` | `transport_details.receipt_city_name`, `receipt_country_code`, … |
| `includedConsignmentItems.totalItemQuantity` | `Integer` | `transport_consignment_items.total_item_quantity` |
| `includedConsignmentItems.totalWeight` | `Decimal` | `transport_consignment_items.total_weight` |
| `includedConsignmentItems.totalVolume` | `Decimal` | `transport_consignment_items.total_volume` |
| `includedConsignmentItems.transportPackages[].shippingMarks` | `String` | `transport_packages.shipping_marks` |
| `includedConsignmentItems.transportPackages[].itemQuantity` | `Integer` | `transport_packages.item_quantity` |
| `includedConsignmentItems.transportPackages[].typeCode` | `String` | `transport_packages.type_code` |
| `includedConsignmentItems.transportPackages[].grossWeight` | `Decimal` | `transport_packages.gross_weight` |
| `includedConsignmentItems.transportPackages[].grossVolume` | `Decimal` | `transport_packages.gross_volume` |
| `transportDetails.cargoType` | `CargoType` | `transport_details.cargo_type` |
| `transportDetails.incoterms` | `Incoterms` | `transport_details.incoterms` |
| `hashcodeDetails.json` | `String` | `transport_operations.hashcode` |
| `hashcodeDetails.algorithm` | `String` | `transport_operations.hashcode_algorithm` |

**ECMRResponse** — risposta alla creazione:

| Campo | Tipo | Salvato in |
|---|---|---|
| `eCMRID` | `String` | `efti_messages.external_id` |
| `uuid` | `String` | `efti_messages.external_uuid` |
| `consignorSender` | `Player` | Dati mittente confermati da MILOS |

**Enumerazioni principali MILOS:**

- `DatasetType`: `ECMR`, `EDDT`
- `PlayerType`: `CONSIGNOR_SENDER`, `SELLER`, `CONSIGNEE`, `CARRIER`, `FREIGHT_FORWARDER`
- `CargoType`: `FTL`, `LTL`, `groupage`
- `Incoterms`: `EXW`, `FCA`, `CPT`, `CIP`, `DAT`, `DAP`, `DDP`, `FAS`, `FOB`, `CFR`, `CIF`
- `EcmrEquipmentCategory`: `CONTAINER` (CN), `SEMITRAILER` (SM), `TRAILER` (TE), `SWAP_BODY` (SW), `TANK` (TN), e altri 12 valori

### 6.4 Flusso Outbound Fase 1

```
1. SUBMIT     Modulo sorgente → POST /api/v1/transport  (API Key)
                  ↓
2. VALIDATE   Validation Service verifica struttura ECMRRequest MILOS
              (eCMRID obbligatorio, consignorSender, consignee, carriers con tractorPlate)
                  ↓
3. UPSERT     Normalization Service:
              · lookup/crea customer (customers.customer_code)
              · lookup/crea destination (customer_destinations.destination_code)
              · mappa payload → ECMRRequest MILOS
              · popola consignorSender da customers + customer_destinations
                  ↓
4. ENQUEUE    efti_messages (status=PENDING, gateway_provider=MILOS) → EftiSendQueue
                  ↓
5. SEND       MilosTfpGateway:
              POST <server>/api/ecmr-service/ecmr
              Body: ECMRRequest JSON
              MILOS risponde: ECMRResponse { eCMRID, uuid }
                  ↓
6. STORE      status → SENT
              external_id = eCMRID, external_uuid = uuid
                  ↓
7. EFTI AUTO  MILOS internamente genera dataset eFTI → eFTI Gate → QR Code autista
                  ↓
8. NOTIFY     Notification Service avvisa sorgente via webhook
```

### 6.5 Esempio Payload MILOS (dataset minimo)

```json
{
  "shipping": {
    "eCMRID": "202526-IT-001",
    "datasetType": "ECMR"
  },
  "consignorSender": {
    "name": "Azienda Mittente S.r.l.",
    "type": "CONSIGNOR_SENDER",
    "postalAddress": {
      "streetName": "Via del Mittente, 1",
      "postCode": "20124",
      "cityName": "Milano",
      "countryCode": "IT",
      "countryName": "Italy"
    },
    "taxRegistration": "07869320965",
    "EORICode": "IT07869320965"
  },
  "consignee": {
    "name": "Receiver S.P.A.",
    "postalAddress": {
      "streetName": "Calle del Destinatario, 2",
      "cityName": "Barcelona",
      "countryCode": "ES",
      "countryName": "Spain"
    },
    "taxRegistration": "10000000001"
  },
  "carriers": [{
    "name": "Trasportatore S.r.l.",
    "postalAddress": {
      "cityName": "Verona",
      "countryCode": "IT",
      "countryName": "Italy"
    },
    "taxRegistration": "10000000002",
    "tractorPlate": "AA000AA"
  }],
  "contractualCarrierAcceptanceLocation": {
    "postalAddress": {
      "cityName": "Milano",
      "countryCode": "IT",
      "countryName": "Italy"
    },
    "date": "2026-03-01 08:00:00"
  },
  "contractualConsigneeReceiptLocation": {
    "postalAddress": {
      "cityName": "Barcelona",
      "countryCode": "ES",
      "countryName": "Spain"
    }
  },
  "includedConsignmentItems": {
    "transportPackages": [{
      "shippingMarks": "COD0001",
      "itemQuantity": 10,
      "typeCode": "PALLET",
      "grossWeight": 1200,
      "grossVolume": 8
    }],
    "totalItemQuantity": 10,
    "totalWeight": 1200,
    "totalVolume": 8
  },
  "hashcodeDetails": {
    "json": "<sha256-del-payload>",
    "algorithm": "SHA-256"
  }
}
```

---

## 7. Fase 2 — Integrazione Diretta con EFTI

### 7.1 Protocolli e Standard

| Standard | Descrizione |
|---|---|
| REST over HTTPS | Interfaccia primaria con API EFTI Gate (TLS 1.3) |
| eDelivery / AS4 | Protocollo obbligatorio per interoperabilità tra piattaforme EFTI nazionali |
| EN 17532 | Standard europeo per i dataset eFTI |
| OAuth2 + X.509 | Autenticazione verso EFTI con `client_credentials` e certificato qualificato |
| eIDAS QES | Firma elettronica qualificata per eBL, eCMR qualificato |

### 7.2 Differenze rispetto alla Fase 1

| Aspetto | Fase 1 (MILOS) | Fase 2 (EFTI Nativo) |
|---|---|---|
| Autenticazione | API Key MILOS | OAuth2 `client_credentials` + X.509 |
| Protocollo | REST HTTP semplice | REST + eDelivery AS4 |
| Formato payload | `ECMRRequest` MILOS | Dataset EN 17532 |
| QR Code | Generato da MILOS | Generato da eFTI Gate o internamente |
| Query autorità | Gestite da MILOS | Query Proxy Service attivo |
| Firma digitale | MILOS/Accudire | Da implementare con eIDAS QES |
| Certificato X.509 | Non richiesto | CA accreditata EU obbligatoria |

### 7.3 Flusso Outbound Fase 2

```
1-3. [Identici alla Fase 1: Submit → Validate → Upsert/Map]
                  ↓
4. ENQUEUE    efti_messages (status=PENDING, gateway_provider=EFTI_NATIVE)
                  ↓
5. SEND       EftiNativeGateway:
              · Recupera token OAuth2 da cache Redis (TTL 1h)
              · Invia dataset EN 17532 via REST HTTPS o AS4
              · EFTI Gate risponde: 202 Accepted + { messageId }
                  ↓
6. STORE      status → SENT; external_id = messageId EFTI
                  ↓
7. CONFIRM    EFTI invia callback asincrono con ACK finale
                  ↓
8. QUERY      Query Proxy Service risponde a query delle autorità
              + audit log GDPR obbligatorio per ogni accesso
                  ↓
9. NOTIFY     Notification Service avvisa sorgente
```

### 7.4 Gestione della Transizione Fase 1 → Fase 2

La transizione è progettata per essere **zero-downtime** e **reversibile**:

```
1. Deploy nuova config: EftiGateway:Provider = "EftiNative"
2. Kubernetes rolling update su EFTI Gateway Service
3. I nuovi messaggi usano EftiNativeGateway
4. I messaggi MILOS in volo completano il loro ciclo (SENT/ACK/DEAD)
5. Se problemi → rollback config → ritorno immediato a MILOS
6. MILOS rimane disponibile come fallback per il periodo di stabilizzazione
```

---

## 8. Frontend React — Funzionalità Principali

| Modulo | Funzionalità |
|---|---|
| Form Trasporto | Inserimento e-CMR/e-DDT con selezione cliente per codice, autocomplete destinazioni |
| Anagrafica Clienti | CRUD clienti, ricerca per codice/ragione sociale, lista `auto_created` da verificare |
| Destinazioni Cliente | Lista destinazioni per cliente, form indirizzo, impostazione default |
| Dashboard | Stato real-time messaggi (SSE), KPI, alerting, **badge fase attiva MILOS/EFTI** |
| Elenco Messaggi | Griglia paginata, filtro per `gateway_provider` (MILOS / EFTI), export CSV |
| Dettaglio Messaggio | Timeline stato, payload JSON, risposta gateway, storico retry, eCMRID/UUID MILOS |
| Dead Letter Queue | Lista messaggi falliti, riprocessamento manuale, note operatore |
| Gestione Sorgenti | CRUD sorgenti, configurazione webhook, rinnovo API key |
| Audit Log | Ricerca log per entità/utente/periodo, export compliance |
| Admin | Gestione utenti, RBAC, parametri sistema, **switch provider MILOS↔EFTI** |

Il pannello Admin mostra chiaramente quale provider è attivo e permette il cambio senza deploy:

```
┌──────────────────────────────────────┐
│  Gateway EFTI — Provider attivo      │
│   ○ MILOS TFP   ● EFTI Nativo        │
│   Stato: ✓ Connesso · ping 2 min fa  │
└──────────────────────────────────────┘
```

---

## 9. Sicurezza e Conformità Normativa

### 9.1 Sicurezza Applicativa (entrambe le fasi)

- OWASP Top 10: input sanitization, CSRF token, CSP headers, SQL parametrizzato
- Secrets Management: HashiCorp Vault / Azure Key Vault, rotazione automatica
- Zero Trust: mTLS tra microservizi in produzione (Istio)
- Penetration test trimestrale + SAST/DAST integrato in CI/CD

### 9.2 Specifico Fase 1 (MILOS)

- API Key MILOS in Vault, mai in codice o variabili d'ambiente
- TLS 1.3 verso tutti gli endpoint MILOS
- Hashcode SHA-256 obbligatorio su ogni payload `ECMRRequest` (`hashcodeDetails`)

### 9.3 Specifico Fase 2 (EFTI Nativo)

- Certificato X.509 da CA accreditata EU: storage in Key Vault, rinnovo automatico
- Token OAuth2 `client_credentials` con TTL 1h, caching in Redis
- mTLS per comunicazione AS4
- eIDAS QES per firma documenti qualificati (eBL, eCMR)
- MariaDB Data-at-Rest Encryption + AES\_ENCRYPT per colonne JSON critiche

### 9.4 Compliance GDPR

- Audit log immutabile per ogni accesso ai dati di trasporto (Art. 5 Reg. 2020/1056)
- Data retention configurabile per sorgente con purging automatico
- **Fase 1**: query delle autorità gestite da MILOS; audit interno traccia le sole azioni del Connector
- **Fase 2**: Query Proxy Service implementa audit GDPR completo obbligatorio per ogni query

---

## 10. Deployment e Scalabilità

- Ogni microservizio deployato come pod Kubernetes con HPA
- **MariaDB**: Galera Cluster 3 nodi (replica sincrona multi-master)
- **RabbitMQ**: cluster 3 nodi con quorum queues
- **Redis**: Sentinel mode (1 master + 2 replica)
- **NGINX Ingress** con cert-manager per TLS automatico
- CI/CD: GitHub Actions / Azure DevOps — build, test, publish, deploy staging automatico

---

*EFTI Connector Platform — Documentazione Architetturale v2.1 — Febbraio 2026*  
*Sezione 4 aggiornata per rispecchiare il modello dati implementato (12 tabelle, EF Core 9 + Pomelo + MariaDB 11.4)*  
*Fase 1: Integrazione MILOS TFP (Circle SpA) · Fase 2: Integrazione Diretta EFTI Gate Nazionale*
