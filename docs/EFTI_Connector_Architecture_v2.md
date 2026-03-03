# EFTI Connector Platform — Documentazione Architetturale

> **Versione:** 2.2 · **Data:** Febbraio 2026  
> Documento di riferimento architetturale per il progetto **EFTI Connector Hub**.  
> Repository: [github.com/alisandre/ilp_efti_connector](https://github.com/alisandre/ilp_efti_connector) · Branch: `main`

---

## Indice

- [1. Introduzione e Contesto](#1-introduzione-e-contesto)
- [2. Strategia di Integrazione a Due Fasi](#2-strategia-di-integrazione-a-due-fasi)
- [3. Stack Tecnologico](#3-stack-tecnologico)
- [4. Diagramma del Database](#4-diagramma-del-database)
- [5. Architettura a Microservizi](#5-architettura-a-microservizi)
- [6. Fase 1 — Integrazione con MILOS TFP](#6-fase-1--integrazione-con-milos-tfp)
- [7. Fase 2 — Integrazione Diretta con EFTI](#7-fase-2--integrazione-diretta-con-efti)
- [8. Frontend React](#8-frontend-react)
- [9. Sicurezza e Conformità Normativa](#9-sicurezza-e-conformità-normativa)
- [10. Deployment e Scalabilità](#10-deployment-e-scalabilità)
- [11. Stato di Implementazione](#11-stato-di-implementazione)

*(Argomenti previsti per le Guide di Sviluppo e Operative)*
- [12. Guida all'Ambiente di Sviluppo](#12-guida-all-ambiente-di-sviluppo)
  - [Prerequisiti e Configurazione Locale (Docker Compose)](#prerequisiti-e-configurazione-locale)
  - [Creazione e Struttura della Solution](#creazione-e-struttura-della-solution)
  - [Configurazione dei Servizi (Database, Broker, Identity Provider, Gateways)](#configurazione-servizi)
  - [Progetti di Test e Verifica](#progetti-di-test-e-verifica)
- [13. Operations e CI/CD](#13-operations-e-cicd)
  - [CI/CD Pipeline](#cicd-pipeline)
  - [Produzione e Orchestrazione (Kubernetes)](#produzione-e-orchestrazione)
  - [Secrets Management e Security](#secrets-management)
  - [Monitoring, Health Checks e Logging (Prometheus, Grafana, Serilog)](#monitoring-health-checks-e-logging)
  - [Runbook Operativo](#runbook-operativo)

---

## 1. Introduzione e Contesto

**EFTI** (Electronic Freight Transport Information) è il framework normativo europeo (Regolamento EU 2020/1056) che impone la digitalizzazione e la standardizzazione dei documenti di trasporto merci all'interno dell'Unione Europea. La piattaforma EFTI stabilisce le basi giuridiche e tecniche per consentire alle autorità pubbliche di controllo di accedere elettronicamente e in tempo reale alle informazioni di trasporto (eCMR per strada, eAWB per aereo, eBL per mare, eRSD per ferrovia).

L'applicazione descritta in questo documento è un **EFTI Connector Hub**: un componente middleware (architettura *Any-to-EFTI*) multi-sorgente che funge da gateway centralizzato verso l'infrastruttura EFTI. Questo hub permette a innumerevoli moduli aziendali preesistenti (TMS, WMS, ERP, software doganali, portali vettori) di comunicare in modo trasparente e uniforme con l'ecosistema istituzionale europeo, nascondendo la complessità dei protocolli di stato.

### 1.1 Obiettivi Principali

- **Centralizzazione e Disaccoppiamento:** Creare un singolo punto d'uscita verso EFTI, evitando *N* integrazioni punto-a-punto dispendiose da mantenere per ciascun software aziendale.
- **Supporto multi-sorgente omnicanale:** Accettare l'ingresso dati da moduli interni (tramite API REST e code di eventi), sistemi di terze parti e operatori umani (tramite interfaccia web dedicata).
- **Gestione autonoma dell'anagrafica (Upsert intelligente):** Mantenere un'anagrafica di clienti (mittenti) e destinazioni che si auto-popola e si aggiorna dinamicamente decodificando in corsa i payload degli ordini in transito.
- **Conformità e Validazione in-flight:** Garantire l'aderenza formale e semantica ai requirement normativi, validando l'input rispetto al rigoroso dataset standard europeo (EN 17532) prima della trasmissione.
- **Resilienza e Osservabilità Operativa:** Offrire un audit log immutabile (cruciale per compliance e GDPR), meccanismi di retry avanzati per indisponibilità di rete (backoff esponenziale) e gestione automatizzata degli scarti (Dead Letter Queue).
- **Gestione Manuale e Monitoraggio UI:** Fornire una Single Page Application moderna in React per permettere al team di supervisionare lo stato di invio dei viaggi, correggere errori o compilare documenti digitali a mano.

### 1.2 Punti Focali di Business e Sfide Normative

Il decollo dell'EFTI implica la sostituzione probatoria della carta in favore del formato puramente digitale basato su dati strutturati interconnessi (e non di semplici PDF inviati via mail). L'EFTI Connector Hub è pensato espressamente per affrontare questa transizione offrendo un vitale **layer di astrazione**:
1. **Adattabilità Normativa:** Quando le specifiche EU sull'eFTI si aggiornano (es. nuovi codici LOCODE obbligatori, variazioni minor al protocollo), l'azienda deve aggiornare solo il Connector, senza intervenire sul codice o patti d'integrazione degli svariati ERP retrostanti.
2. **Indipendenza dal Provider Esterno:** La capacità di passare dinamicamente da un partner terzo (es. la piattaforma MILOS usata per il ramp-up iniziale) ad una connessione istituzionale diretta con lo stato senza downtime, rappresentando di fatto una "polizza di assicurazione" tecnologica per l'azienda sul lungo termine.

---

## 2. Strategia di Integrazione a Due Fasi

L'integrazione verso l'infrastruttura EFTI istituzionale non è banale: richiede rigorose certificazioni crittografiche (X.509 qualificate, eIDAS, AS4) e l'interfacciamento con nodi europei (eFTI Gates) attualmente in via di finalizzazione in molti stati membri.
Per mitigare i rischi del progetto e consentire un test immediato sul campo (*time-to-market* ridotto), il Connector implementa un'architettura **bifase** basata su interfacce scambiabili dinamicamente.

### 2.1 Fase 1 — Integrazione via Provider Intermediario MILOS TFP (MVP)

In questa fase iniziale, il Connector si affida a **MILOS TFP** (piattaforma commerciale di Circle SpA) in veste di broker intermediario accreditato. Il Connector invia i dati di trasporto a MILOS usando semplici API REST. MILOS si fa carico di tutta la complessità crittografica e di "traduzione" governativa: convalida finale, invio sui nodi EFTI di stato e generazione del **QR Code di viaggio** (obbligatorio per i controlli della polizia stradale).

**I vantaggi di questa fase del MVP:**
- Barriera d'ingresso abbassata (si ignorano i protocolli complessi come eDelivery AS4).
- Nessun acquisto e mantenimento immediato di certificati digitali per gli hub o test eIDAS.
- Rollout rapido dei moduli TMS in produzione, permettendo all'azienda di digitalizzare le spedizioni.
- Ottimo per le sessioni di training degli operatori di piazzale e doganali.

### 2.2 Fase 2 — Integrazione Diretta e Nativa con l'EFTI Gate di Stato (Evolutiva)

Inizierà la sostituzione di MILOS. Il Connector attiverà la capacità di comunicare **direttamente con gli EFTI Gate istituzionali** (italiani o europei). Il layer di astrazione architetturale fa sì che questo *salto* avvenga a costo di sviluppo quasi nullo per il resto del sistema.

**I cambiamenti in Fase 2:**
- Indipendenza assoluta: zero costi ricorrenti verso intermediari di terze parti e SLA gestiti internamente.
- Gestione in house delle firme crittografiche qualificate, delle connessioni via AS4 e token basati su OAuth2 via Mutual TLS (mTLS).
- Il provider MILOS diventa una connessione di fallback di emergenza.

### 2.3 Astrazione e Feature Flags (Pattern `Strategy`)

A livello di codice, il Core del Connector non conosce i dettagli né di MILOS né di EFTI Gate. Conosce solo un'interfaccia standardizzata (`IEftiGateway`). Questa struttura permette al sistema di deviare i messaggi sull'uno o sull'altro in millisecondi in base alla configurazione.

```csharp
// L'interfaccia architetturale immutabile sia in Fase 1 che in Fase 2
public interface IEftiGateway
{
    Task<EftiSendResult>      SendEcmrAsync(EcmrPayload payload, CancellationToken ct);
    Task<EftiSendResult>      UpdateEcmrAsync(string ecmrId, EcmrPayload payload, CancellationToken ct);
    Task<EftiSendResult>      DeleteEcmrAsync(string ecmrId, CancellationToken ct);
    Task<EcmrPayload>         GetEcmrAsync(string ecmrId, CancellationToken ct);
    Task<GatewayHealthStatus> HealthCheckAsync(CancellationToken ct = default);
}

// Fase 1: Classe implementativa che traduce in chiamate REST verso MILOS
public class MilosTfpGateway : IEftiGateway { /* ... */ }

// Fase 2: Classe che traduce il payload e usa certificati di stato EFTI e AS4
public class EftiNativeGateway : IEftiGateway { /* ... */ }
```

Il cambio del comportamento avviene *live* alterando la configurazione, senza dover ricompilare o fermare in blocco il servizio:

```json
{
  "EftiGateway": {
    "Provider": "Milos", // Switch operativo tra "Milos" o "EftiNative"
    "Milos": {
      "BaseUrl": "https://milos.provider.example/api/",
      "ApiKey": "<secret-from-vault>"
    },
    "EftiNative": {
      "BaseUrl": "https://efti-gate.gov.it/api/",
      "ClientId": "<oauth2-client-id>",
      "CertificatePath": "<secret-from-vault>"
    }
  }
}
```

---

## 3. Stack Tecnologico

Il Connector adotta principi di *Cloud-Native Development*: containerizzazione rigorosa, elaborazione asincrona a eventi, pattern moderni per l'infrastruttura Microsoft e isolamento delle componenti frontend/backend.

### 3.1 Backend Enterprise in .NET 9

L'applicativo si affida alle ultime tecnologie del framework Microsoft, studiato per latenze minime e massimi carichi in multi-threading.

| Scopo Core | Componente / Framework | Note Operative |
|---|---|---|
| Framwork Base | **.NET 9** (C# 13) | Performance estreme nella gestione della I/O HTTP e serializzazione Json. |
| API Layer | **ASP.NET Core Web API** | Minimal API con OpenAPI (Swagger) esposto per documentare i contatti verso team terzi. |
| Message Bus | **MassTransit + RabbitMQ** | Orchestrazione degli eventi interni (es. *EcmrValidated*, *TransmissionFailed*). Assicura solidità in caso di picchi. |
| Data Access | **Entity Framework Core 9** | Object-Relational Mapper (ORM) connesso a driver di alto livello (*Pomelo.EntityFrameworkCore.MySql*). |
| Resilienza H/C | **Polly v8** | Policy avanzate di timeout per le chiamate HTTP, es. *Circuit Breaker* (quando EFTI Gate o MILOS cadono), Retry ed Exponential backoff. |
| Retry Asincroni | **Hangfire** | Scheduling della logica per ri-processare ordini scartati dal gateway e per lavori notturni (vacuum dati). |
| Sicurezza Auth | **Keycloak (OAuth2/OIDC)** | Identity Provider decentralizzato per utenti umani (Dashboard) e generazione di token Machine-to-Machine. |
| Telemetria | **Serilog + Seq** | Logging JSON strutturato. Trace ID propagati tra i microservizi per correlare gli errori in log distribuiti. |

### 3.2 Frontend (Management ed Entry Point Umano)

Una Single Page Application studiata per reattività ed ergonomia per il personale logistico.

| Ruolo Componente | Strumento Frontend | Dettagli di Utilizzo |
|---|---|---|
| Piattaforma SPA | **React 19 + TypeScript** | Robustezza al compilatore e modern hook pattern. Tooling di aggregazione via **Vite**. |
| Interfaccia Dati | **Ant Design / MUI** | Utilizzati intensivamente per griglie complesse (*TanStack Table*) con filtri a colonne su migliaia di transiti logistici. |
| Form & Controlli | **React Hook Form + Zod** | Tutti i form (es: compilazione Ddt/Ecmr manuali a mano quando l'ERP fallisce) sono protetti da validazioni Zod sincronizzate con le logiche backend. |
| Cache & Dati | **Zustand + React Query** | Gestione locale dello stato applicativo e server state ri-validato dal vivo evitando chiamate inefficaci e colli di bottiglia all'API. |

### 3.3 Persistenza Dati e Servizi Infrastrutturali

Tecnologie open source classiche scalabili affidate a storage persistente e clusterizzabile.

| Layer di Rete | Tecnologia | Utilizzo nel Sistema |
|---|---|---|
| Database Transazionale | **MariaDB 11.4** LTS | Custode inesorabile degli Audit Log immutabili, stati avanzamento spedizioni, anagrafiche aziende clienti e vettori. |
| Cache in RAM & Lock | **Redis 7** | Usato massicciamente per memorizzare i token Bearer di ritorno, imporre Rate-Limiting alle richieste ERP per abuso e gestire lock distribuiti su ID doppioni. |
| Code asincrone | **RabbitMQ 3.13** | Scatola d'ingranaggi pulsante per i messaggi tra l'esposizione del Connector e e l'invio fisico dell'eCMR (Pattern Produce/Consumer). |
| Binary Storage | **MinIO (S3 compatibile)** | *[Futuro]* Necessario per accogliere foto dei carichi, moduli autografati digitalmente, file di bolle tradotti e file di attestazione scannerizzati generati. |

---

## 4. Diagramma del Database

Il database transazionale **MariaDB 11.4** è strutturato in **12 tabelle** interconnesse, ottimizzate per la deduplicazione anagrafica e la tracciabilità delle operazioni. Lo schema è stato progettato per restare **invariato** in caso di transizione dalla Fase 1 alla Fase 2.

```mermaid
erDiagram
    %% Core Entities
    sources ||--o{ customers : "generates"
    sources ||--o{ transport_operations : "submits"

    customers ||--o{ customer_destinations : "owns"
    customers ||--o{ transport_operations : "is sender for"
    customer_destinations ||--o{ transport_operations : "is delivery point for"

    %% Transport Data Normalization
    transport_operations ||--o{ efti_messages : "generates messages"
    transport_operations ||--|| transport_consignees : "has 1 consignee"
    transport_operations ||--o{ transport_carriers : "has N carriers"
    transport_operations ||--|| transport_details : "has 1 details"
    transport_operations ||--|| transport_consignment_items : "has 1 items summary"

    transport_consignment_items ||--o{ transport_packages : "contains N packages"

    %% Security & Audit
    users ||--o{ transport_operations : "creates manually"
    users ||--o{ audit_logs : "performs actions"
```

---

### 4.1 Tabella: `customers`

Anagrafica dei mittenti principali (spesso i clienti dell'azienda che installa il Connector).

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID interno |
| `customer_code` | `VARCHAR(100)` | UNIQUE NOT NULL | Codice cliente dal sorgente ERP — **chiave di lookup** per upsert automatico |
| `business_name` | `VARCHAR(300)` | NOT NULL | Ragione sociale del mittente |
| `vat_number` | `VARCHAR(50)` | | Partita IVA / VAT number |
| `eori_code` | `VARCHAR(20)` | | Codice EORI (obbligatorio in EFTI per tratte cross-border) |
| `is_active` | `BOOLEAN` | DEFAULT true | Soft delete /abilitazione |
| `auto_created` | `BOOLEAN` | DEFAULT false | `true` = creato automaticamente da un payload in corsa, nessuna validazione umana |
| `source_id` | `CHAR(36)` | FK `sources` | Sorgente che ha generato la creazione automatica |

### 4.2 Tabella: `customer_destinations`

Indirizzi di scarico e terminal associati a un `customer` specifico. Si auto-popolano grazie a `destination_code`.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID interno |
| `customer_id` | `CHAR(36)` | FK `customers` NOT NULL | Cliente proprietario |
| `destination_code` | `VARCHAR(100)` | UNIQUE NOT NULL | Codice destinazione dal sorgente |
| `address_line1` | `VARCHAR(300)` | NOT NULL | Via e numero civico |
| `city` | `VARCHAR(200)` | NOT NULL | Città e `country_code` (ISO 3166-1 alpha-2) |
| `un_locode` | `VARCHAR(10)` | | Codice UN/LOCODE (fondamentale per le direttive marittime e ferrovia) |
| `is_default` | `BOOLEAN` | DEFAULT false | Usata per fallback form UI |

**Logica di Upsert Anagrafico Intelligente** (Avviene al volo, pre-invio):
1. Cerca il record `WHERE customer_code = :code`.
2. Se trovato, esegue `UPDATE` di p.iva o campi disallineati se più recenti.
3. Se non trovata, lo crea al volo (`INSERT` con flag `auto_created = true`).

### 4.3 Tabella: `sources`

I sistemi che parlano con il Connector. Ognuno ha una API Key dedicata (hashata) per motivi di auditing e partizionamento.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | Identificatore univoco |
| `code` | `VARCHAR(50)` | UNIQUE NOT NULL | Es. `TMS_MAGAZZINO_A`, `ERP_SAP_FINANCE` |
| `type` | `VARCHAR(20)` | NOT NULL | Enum: `TMS` \| `WMS` \| `ERP` \| `MANUAL` (Front-end) |
| `api_key_hash` | `VARCHAR(64)` | | Hash SHA-256 della API key Bearer usata |
| `config_json` | `JSON` | | Configurazione flessibile per custom Webhook payload URLs di ritorno |

### 4.4 Tabella: `transport_operations`

È il cuore del business logistico. Traccia la spedizione logica astraendo dalla singola chiamata HTTP.

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `id` | `CHAR(36)` | PK | UUID operazione logistica |
| `operation_code` | `VARCHAR(100)` | NOT NULL INDEXED | Codice univoco CMR/DDT (spesso `eCMRID` per MILOS) |
| `dataset_type` | `VARCHAR(50)` | NOT NULL | `ECMR` (Strada) \| `EDDT` (DDT interno) |
| `status` | `VARCHAR(30)` | NOT NULL | `DRAFT`, `VALIDATED`, `SENDING`, `SENT`, `ERROR`, ecc. |
| `hashcode` | `VARCHAR(64)` | | Hash SHA-256 matematico del payload (richiesto per fini legali e non modificabilità) |
| `raw_payload_json` | `JSON` | | Fotografia del JSON originale prima delle normalizzazioni (utile in caso di dispute e debug) |

### 4.5 Tabella: `efti_messages`

Tabella che traccia i *tentativi fisici* di comunicazione verso il nodo di stato (invariata tra F1 e F2).

| Colonna | Tipo | Vincolo | Descrizione |
|---|---|---|---|
| `gateway_provider` | `VARCHAR(20)` | NOT NULL | `MILOS` \| `EFTI_NATIVE` |
| `direction` | `VARCHAR(10)` | NOT NULL | `INBOUND` \| `OUTBOUND` |
| `status` | `VARCHAR(20)` | NOT NULL INDEXED | `PENDING`, `ACKNOWLEDGED`, `RETRY`, `DEAD` |
| `external_id` | `VARCHAR(100)` | INDEXED | F1: `eCMRID` da MILOS — F2: `UUID/MessageId` di stato EFTI |
| `retry_count` | `SMALLINT` | DEFAULT 0 | Fondamentale per backoff asincrono (Hangfire) |

### 4.6 — 4.10 Tabelle `transport_*` minori (Normalizzazione Payload)

I dati del viaggio non vivono tutti in file JSON: vengono disassemblati ed esplosi in tabelle relazionali per poter fare statistiche e filtri d'interfaccia complessi (es. "Mostrami tutti i viaggi dove il vettore si chiama X").

- `transport_consignees`: Chi riceve la merce (Street, City, EORI).
- `transport_carriers`: Lista ordinata (1..N) di chi fisicamente trasporta (camionista/azienda). Campo importante: `tractor_plate` (targa motrice).
- `transport_details`: Aspetti contrattuali. `incoterms` (EXW, DAP, FOB), `cargo_type` (FTL/LTL) o luoghi di Presa In Carico contrattuale.
- `transport_consignment_items`: I pesi e volumi totali in kilogrammi o metri cubi (es: 12.000 kg totali).
- `transport_packages`: Splittaggio al dettaglio in righe del carico, con descrizioni. Es: "Bancali", "Box", "Reefer" e i singoli pesi (es: 1200kg colli 1-5, 500kg colli 6-30).

### 4.11 e 4.12 Sicurezza: `users` e `audit_logs`

I log GDPR immutabili previsti per Art. 5 Reg. EU 2020/1056. È la "Security Box" legale.

- L'entità `audit_logs` **non ha permessi di DELETE** a livello database.
- Memorizza il JSON del record prima (`old_value_json`) e dopo la modifica (`new_value_json`).
- Si incrocia nativamente via `keycloak_id` per identificare l'umano o via `api_key_hash` per le automazioni.

---

## 5. Architettura a Microservizi (Message-Driven)

Il backend è segmentato internamente usando il pattern In-Memory Publisher/Subscriber (MassTransit + RabbitMQ) per garantire fault-tolerance. Se un servizio vitale come l'invio esterno fallisce, l'ingestione tramite API non si blocca e non perde chiamate dall'ERP.

### 5.1 Macro-Flusso dei Componenti 

1. **Ingestion & API Gateway Service (`Source Ingestion`)**: Espone la porta `POST /api/v1/transport` per l'ingresso da TMS/WMS esterni. Autentica il Jwt/API Key, verifica i limiti (Rate Limiting via Redis) e converte la richiesta sincrona in un pacchetto `TransportSubmitted` asincrono gettato su code RabbitMQ. Restituisce HTTP 202.
2. **Validation Service**: Pesca il body e ne applica validazioni formali.
   - Fase 1: Verifica presenza chiavi minime per i provider di mercato (MILOS chiede almeno P.IVA valida, Targa, Nazione ISO).
   - Fase 2: Check formale *EN 17532* (struttura xml/json e id semanticamente stretti). Pubblica `TransportValidated` o genera eccezione notificata in dashboard.
3. **Normalization & Mapping Service**: Cuore della logica transazionale interna. Applica la logica di *Upsert* esposta nella Sezione 4 sulle anagrafiche e genera le insert EF Core nel DB. Riformatta poi la stringa trasformandola da lingua universale a JSON proprietario di MILOS e apre la pratica in stato di `DRAFT / SENDING`.
4. **EFTI Gateway Service *(Componente Bifasico Core)***: 
   - Iniettato tramite `GatewaySelector` a runtime.
   - Si serve di circuit breaker (`Polly v8 GatewayResilienceHandler`).
   - Se il provider non risponde (es. MILOS in manutenzione programmata), ingabbia le eccezioni, mette il messaggio in status transitorio e demanda il problema a Hangfire per retry.
5. **Response Handler e Webhook Notification Service**: Al check di ritorno OK dal provider. Esegue query su anagrafiche webhook e spara post HTTP all'indietro per svegliare e notificare in automatico gli ERP (così che i terminalisti possano far partire i camion col QR generato). In contemporanea, aggiorna in SSE (Server-Sent-Events) le UI React affinché i cruscotti diventino verdi real-time.
6. **Query Proxy e Audit Service**: Endpoints per lettura read-only (es `GET /query/operations?page=3`). È l'unico punto che interagisce pesantemente con Redis cache per non uccidere il DB e appende **obbligatoriamente** righe sull'Audit GDPR per tracciare chi (quale autorità stradale e umana) ha sbirciato la lettera di vettura.

---

## 6. Fase 1 — Integrazione con MILOS TFP (MVP)

### 6.1 Overview MILOS TFP (Partner Tecnologico)

Per accelerare il *time-to-market* e bypassare le complessità della certificazione crittografica eIDAS ed eDelivery AS4 (richieste dai nodi di stato), la **Fase 1** adotta l'hub di mercato **MILOS TFP** (sviluppato da Circle SpA) come Intermediario Fiduciario accreditato. 

MILOS offre all'EFTI Connector due layer architetturali trasparenti in cascata:
- **e-CMR Service**: Un modulo di business puro per la compilazione della Lettera di Vettura Elettronica. È l'interfaccia con cui il Connector dialoga.
- **eFTI / UUM&DS Platform**: Il motore che, invisibilmente, prende i dati dell'e-CMR, li impacchetta secondo la semantica **EN 17532**, firma digitalmente (tramite HSM / Accudire) e invia il plico al nodo EFTI nazionale. Inoltre genera il **QR Code univoco** da stampare o fornire sull'app dell'autista, fondamentale per ispezioni su strada.

### 6.2 Contratti API (e-CMR Service)

Le chiamate dal nostro *EFTI Gateway Service* a MILOS avvengono via **REST JSON (TLS 1.3)** e sono protette da *API Key* statica (long-lived) injectata tramite Vault di configurazione.

**Endpoint Base**: `https://<milos-environment-url>/api/ecmr-service/`

| Operazione | Metodo HTTP | Percorso Relativo | Scopo |
|---|---|---|---|
| **Issue e-CMR** | `POST` | `ecmr` | Invia un nuovo trasporto. Ritorna `eCMRID` autogenerato. |
| **Update / Amend**| `PUT` | `ecmr/{id}` | Aggiorna dati di un viaggio non ancora chiuso (es. cambio targa). |
| **Cancel** | `DELETE` | `ecmr/{id}` | Abortisce legalmente l'operazione. |
| **Retrieve** | `GET` | `ecmr/get/{id}` | Forza un allineamento dello stato se i webhook di MILOS falliscono. |

### 6.3 Data Mapping `ECMRRequest` vs Internal Model

La mappatura è condotta dal *Normalization Service*. Ogni JSON transita attraverso la classe DTO `ECMRRequest` pretesa da MILOS. I campi principali sono associati come segue:

| Nodo MILOS `ECMRRequest` | Entità DB / Tabella Origine | Note e Regole di business |
|---|---|---|
| `shipping.eCMRID` | `transport_operations.operation_code` | Chiave di correlazione idempotente originata dal TMS. |
| `consignorSender.*` | `customers` & `customer_destinations` | Dati Mittente e indirizzo pick-up. P.Iva o *EORI Code* obbligatori. |
| `consignee.*` | `transport_consignees` | Destinatario finale (Receiver) e relativo scarico. |
| `carriers[i].*` | `transport_carriers` | Array dei vettori. **`tractorPlate`** è blindato e obbligatorio per i controlli del QR Code stradale. |
| `includedConsignmentItems` | `transport_packages` (totali & colli) | Sommatoria logica di pesi, numero colli, volumi e *shippingMarks*. |
| `transportDetails.incoterms` | `transport_details.incoterms` | Condizioni di resa della merce (DAP, EXW, ecc.). |
| `hashcodeDetails` | Elaborazione generata *in-transit* (C#) | Hash SHA-256 applicato all'intero payload pulito. La Piattaforma di stato scarta richieste con hash asimmetrico. |

### 6.4 Sequenza Outbound (Fase 1)

Il seguente diagramma modella il viaggio di un payload all'interno della rete a microservizi nella Fase 1.

```mermaid
sequenceDiagram
    autonumber
    actor TMS as ERP Aziendale (TMS)
    participant API as Ingestion API / Auth
    participant Val as Validation & Upsert Service
    participant Msg as RabbitMQ (Message Bus)
    participant GTW as MILOS Gateway (Polly/HTTP)
    participant MILOS as Piattaforma MILOS

    TMS->>API: POST /api/v1/transport (API Key)
    API-->>TMS: 202 Accepted (CorrelationID)
    API->>Msg: Publish [TransportSubmittedEvent]

    Msg->>Val: Consume & Validate Schema
    Val->>Val: Upsert su `customers` e normalizzazione Db
    Val->>Msg: Publish [EftiSendRequestedEvent]

    Msg->>GTW: Consume Msg
    GTW->>MILOS: HTTP POST /ecmr (Body: ECMRRequest)

    alt Success
        MILOS-->>GTW: 200 OK { eCMRID, uuid }
        GTW->>Msg: Publish [SendSuccessEvent]
    else Failure (es. Dati P.Iva errati)
        MILOS-->>GTW: 400 Bad Request
        GTW->>Msg: Publish [TransmissionFailedEvent] → Dead Letter
    else Timeout / Network
        GTW--xMILOS: Connection Refused
        GTW->>GTW: Polly Retry (Esponenziale)
    end
```

---

## 7. Fase 2 — Integrazione Diretta con l'EFTI Gate di Stato

### 7.1 Overview del Paradigma Nativo

L'evoluzione prevista per il prodotto rimuove l'intermediario commerciale (MILOS) instaurando un canale *Machine-to-Machine* puro (M2M) direttamente verso i nodi (EFTI Gate) predisposti dagli Stati dell'Unione Europea. Questo approccio è **disaccoppiato contrattualmente** e garantisce alla piattaforma logistica una forte *Self-Sovereignty* digitale.

### 7.2 Protocolli e Standard (Architettura di Rete EU)

La connettività passa da una classica interazione "Web API API-Key based" a un'infrastruttura Enterprise regolamentata istituzionalmente:

| Modulo Normativo | Specifica Tecnica Implementativa |
|---|---|
| **Data Payload (Standard)** | Conversione dal linguaggio interno verso XML/JSON secondo i rigidi standard UML europei **EN 17532**. |
| **Livello Trasporto (B2G)** | Protocollo **eDelivery / AS4** su base mutual TLS (mTLS), obbligatorio per l'interoperabilità sicura e il non-ripudio. |
| **Trust e Identità** | Passaggio dall'API Key al rilascio di **Token OAuth2 via eIDAS**, corroborati da *Certificati X.509 EU* archiviati in *Key Vault*. |
| **Sicurezza Apposizione** | Modulo interno di **Firma Elettronica Qualificata (QES)** e generazione autonoma dell'URI dinamico (per il QR Code sul fronte mezzo). |

### 7.3 Tabella Comparativa di Transizione

Identificare in modo chirurgico i confini di mutamento garantisce di minimizzare gli impatti di codice durante questa evoluzione:

| Strato Architetturale | Fase 1 (MILOS) | Fase 2 (EFTI Nativo) | Impatto di Riscrittura |
|---|---|---|---|
| **Interfaccia ERP (Ingresso)** | Webhook JSON custom + REST | Webhook JSON custom + REST | Nessuno |
| **Database Strutturale** | MariaDB Relazionale | MariaDB Relazionale | Nessuno |
| **Autenticazione Gateway** | API Key in header (`Authorization`) | M2M OAuth2 Token (Redis Cache) + X.509 | Isolato nel `GatewaySelector` |
| **Payload Esterno** | Classe `.NET` `ECMRRequest` custom | Dataset standardizzato EN 17532 | Sviluppo di un nuovo Mapping Service dedicato |
| **QR Code / Ispezioni** | Generato remotamente da MILOS | Generato e firmato dall'EFTI Connector stesso | Richiede libreria dedicata + QES |
| **GDPR & Autorità (Audit)** | Le Polizie Stradali interrogano MILOS | Il *Query Proxy Service* deve esporre API pubbliche traccianti | Esporre Endpoints Pubblici e log su DB |

### 7.4 Flusso Operativo della Transizione (Zero-Downtime)

La sostituzione del provider in linea di produzione è gestita in ottica *Circuit Toggle* (Feature Flag) accoppiata ad un rollout morbido in Kubernetes (k8s), per prevenire disservizi ai mezzi in parcheggio:

1. **Deployment Ombra:** Le classi `EftiNativeGateway` (il nuovo SDK) vengono deployate su tutti i pod logici dormienti (inattive).
2. **Flag Flip:** Da pannello Admin (React UI), o alterando un `ConfigMap`, il flag applicativo `EftiGateway.Provider` muta da `"Milos"` a `"EftiNative"`.
3. **Drain delle Code:** RabbitMQ instrada lecendosi nativamente i nuovi payload in ingresso alla nuova pipeline del mapper *EN 17532*.
4. **Smaltimento Graceful:** I messaggi *in-volo* pendenti nella coda legata a MILOS vengono progressivamente processati in background senza venir abortiti malamente.
5. **Fallback Safety:** Qualsiasi criticità di handshake Oauth2 o timeout AS4 sul nodo statale produce allarmi immediati (Prometheus/Serilog). È sufficiente deflaggare in UI l'hub statale per deviare nuovamente il fiume transazionale sul rassicurante backup MILOS in meno di un secondo.

---

## 8. Frontend React — Pannello di Gestione

Una Single Page Application (SPA) realizzata in **React 19 + TypeScript**, dotata di design system ergonomico per gli operatori logistici (Ant Design / MUI). 
Il front-end funge da "torre di controllo" per esaminare in tempo reale il flusso B2B generato dagli ERP, intervenendo dove necessario e amministrando parametri di sistema.

### 8.1 Struttura e Architettura UI

| Modulo UI | Ruolo nel dominio |
|---|---|
| **Vite & TS** | Bundler ultraveloce (HMR) e sicurezza al transpiling sui DTO di rete per eliminare "undefined is not a function". |
| **Zustand + React Query** | Il client memorizza localmente lo stato dell'interfaccia (Zustand). I dati asincroni dalle API sono invece serviti, con caching a 5 min, da *TanStack React Query*. |
| **SSE (Server-Sent Events)**| Mantengono *live* il cruscotto: un CMR che passa a "Errore" accende una spia rossa nello schermo dell'operatore senza bisogno di refresh della pagina (Polling). |
| **Keycloak Auth** | Login centralizzato. Genera un Bearer Token per le restrizioni di Role-Based Access Control (es: utente "Viewer" vs "Admin"). |

### 8.2 Funzionalità Business

- **Dashboard Real-Time**: KPI aggregati sui volumi della settimana, tassi di scarto MILOS, e un **Badge di status globale** ben visibile che chiarisce in quale fase (1 MILOS o 2 STATO) sta lavorando il Connector.
- **Griglia Spedizioni (e-CMR)**: Elenca ogni operazione con filtri testuali rapidi per CMR id, P.Iva, Nazione e Targa del Trattore. Modalità di export nativo CSV o JSON.
- **Operazioni Manuali (Form)**: Pensato come backup in caso di *down* dei gestionali interni. Strutturato tramite form dinamici e complessi (via *React Hook Form* & Zod object validation). Consente ad un umano di compilare graficamente e per intero una Lettera di Vettura e premere INVIA.
- **Anagrafica Adattiva**: Specchio delle tabelle SQL `customers`. Presenta interruttori "Soft-Delete", moduli di sovrascrittura di un indirizzo di consegna (LOCODE) ed evidenzia chiaramente i record etichettati `<Auto-Created>` dai flussi notturni che meritano un check dell'operatore.
- **Dead Letter Queue (I fallimenti)**: Un'infermeria dove finiscono i payload in coma (es: Partita IVA non formattata logicamente, superati i 6 tentativi automatici di Hangfire). L'utente può editare il JSON grezzo ed eseguire un riprocessamento manuale ("Re-Queue").

### 8.3 Il Pannello Admin System

Fondamentale per i DevOps e i manager tecnici. Oltre alla gestione delle API keys e la rotazione dei ruoli, contiene lo **Switch Bifasico**:

```
┌───────────────────────────────────────────────┐
│ ⚙️ Gestione Gateway EFTI — Ambiente di Prod   │
│                                               │
│    Provider attivo al momento:                │
│    ○ MILOS Partner Com.   ● EFTI Nativo M2M   │
│                                               │
│   [Stato: ✓ Connesso]  · [Ping: 12ms]         │
└───────────────────────────────────────────────┘
```
L'interazione con questo switch richiama le API admin del *GatewaySelector* provocando in frazioni di secondo lo shift del routing dietro le quinte.

---

## 9. Sicurezza e Conformità Normativa

La natura istituzionale e "Legal Tech" della piattaforma EFTI richiede garanzie di sicurezza pari a quelle dei sistemi bancari. Sostituendo la lettera di vettura cartacea firmata a penna con dati puramente digitali, i rischi collegati a *tampering* (falsificazione di documenti di trasporto) e infrazioni GDPR risultano i driver primari dell'architettura di sicurezza.

### 9.1 Identità, Autenticazione ed RBAC

- **Autenticazione Macchina-Macchina (M2M):** Gli ERP sorgente non usano password umane, ma interagiscono con il *Connector API Gateway* avvalendosi di Json Web Token (JWT) rilasciati da Keycloak o tramite API Key con hashing forte (SHA-256) salvate su Vault.
- **Micro-segmentazione e Zero-Trust:** Nel cluster Kubernetes di produzione, i microservizi dialogano esclusivamente tramite `mTLS` (Mutual TLS). Se un attore malevolo riuscisse a bucare l'interfaccia React, non potrebbe mai eseguire una query su MariaDB, poiché il DB accetta connessioni solo dai pod legittimati dalla PKI interna.
- **Roles-Based Access Control (RBAC) granulare:** Nel Frontend React, i permessi di lettura JSON, sovrascrittura forzata (DLQ) o cambio Provider (MILOS ↔ Nativo) sono vincolati a scope specifici restituiti dagli *Access Token* OAuth2.

### 9.2 Protezione del Dato e Crittografia (Data-at-Rest & In-Flight)

La confidenzialità commerciale (es: il costo di un trasporto, la tipologia di merce e l'identità del cliente finale) è vitale per le aziende logistiche.

- **Fase 1 (MILOS):** Le comunicazioni avvengono tramite **TLS 1.3**. Per garantire l'integrità durante la trasmissione, l'SDK `.NET` calcola un Hash SHA-256 esatto dell'intero payload (`ECMRRequest`), allegandolo come `hashcodeDetails`. MILOS rispedisce errore se l'hash ricalcolato non combacia (prevenzione Man-In-The-Middle attacks).
- **Fase 2 (EFTI Nativo - Imminente):** L'impianto crittografico scala verso gli standard governativi. 
  - Archiviazione di **Certificati X.509 (Qualificati EU)** in Azure Key Vault / HashiCorp.
  - Firma Elettronica Qualificata (QES) eIDAS per apporre il sigillo legale al pacchetto aziendale.
  - Implementazione del rigido protocollo di messaggistica inter-nazionale **AS4 via eDelivery**, che assicura crittografia asimmetrica *End-to-End* fino al ministero competente.
- **Storage Encryption:** Il Database transazionale prevede crittografia a riposo nativa sul volume disco. Per dati altamente sensibili, le colonne JSON in EF Core sono decifrate a runtime via funzioni `AES_DECRYPT()`.

### 9.3 Conformità Legislativa EFTI e GDPR Audit Logging

Il Regolamento (UE) 2020/1056 impone che ogni accesso umano o di sistema ai dati logistici da parte di soggetti terzi avvenga in modo inequivocabilmente tracciabile.

- **Pattern *Append-Only Audit Log*:** Le entità `users` e i sistemi sorgenti che operano un CRUD non cancellano né sovrascrivono i record transitati, ma inseriscono istruzioni nella tabella segregata `audit_logs` priva di trigger di `DELETE`.
- **Interfaccia e Autorità di Controllo:** 
  - *(Fase 1)*: Le autorità (es. agenti di stradale in ispezione) interrogano il nodo MILOS inserendo l'UUID del QR Code. MILOS si fa garante del tracciamento.
  - *(Fase 2)*: Sarà il nostro *Query Proxy Service* a dover soddisfare le interrogazioni istituzionali per conto del cliente aziendale. Un middleware intercetta la Action API e obbliga ad applicare un Audit Entry, comprensivo di IP, Identificatore Agente e timestamp, prima di restituire il Payload del Documento Logistico.
- **Data Lifecycle (Purging automatico):** Un job cron *Hangfire* anonimizza ed espunge permanentemente i record transattivi vecchi di *N* mesi (parametro GDPR aziendale configurabile per sorgente), lasciando solo aggregati statistici, sollevando l'azienda dai costi di Cloud Storage evitabili.

---

## 10. Deployment e Scalabilità Infrastrutturale

In logistica, la documentazione è "Time-Critical": un camion che attende alla barriera doganale o al terminale portuale perché il Connector EFTI è irraggiungibile genera costi di demurrage e impatti drammatici. L'approccio al deployment è totalmente focalizzato sull'Alta Affidabilità (High Availability).

### 10.1 Orchestrazione Kubernetes (K8S) e Microservizi

L'intera applicazione *Backend Formatter* ed i *Poller* sono containerizzati tramite OCI / Docker immagini `alpine-distroless` (per minimizzare la superficie d'attacco e le vulnerabilità OS) e ospitati su un orchestratore Kubernetes.

- **Horizontal Pod Autoscaling (HPA):** Sfruttando l'architettura Message-Driven di RabbitMQ, se l'API Gateway dovesse ricevere un Flood/Spike da 15.000 ordini simultanei (es: un ERP sversa tutti i viaggi notturni alle ore 06:00), il cluster k8s nota l'aumento dei pacchetti nel bus e invoca nuove repliche dinamiche dei vari Worker Service. 
- **Graceful Shutdown & Readiness Probes:** Le *HealthCheck API* integrate in .NET 9 informano costantemente l'Ingress NGINX sullo stato del nodo. L'aggiornamento dell'applicazione avviene senza interruzione del servizio (Rolling Update).

### 10.2 Storage e Middleware Clusterizzati

Tutti gli ingranaggi fondamentali sono immuni al Single-Point-Of-Failure (SPOF):

- **Data Tier Relazionale (MariaDB 11.4):** Eseguito in modalità *Galera Cluster* con architettura a 3 Nodi multi-master. Questo garantisce lock distribuiti sicuri e sincronizzazione garantita senza drift di dati, anche in caso di kernel panic di un nodo.
- **Broker (RabbitMQ 3.13):** Cluster a 3 nodi in modalità `Quorum Queues` sfruttando il protocollo *Raft* per tollerare la caduta secca del leader, assicurando che l'evento critico `TransportValidatedEvent` non venga mai evaporato.
- **Object Serialization Cache (Redis 7):** Operante in schema `Sentinel Mode` con un master e due repliche, necessario per distribuire i Rate Limiters multi-istanza e reggere migliaia di operazioni in millisecondi in fase di validazione del payload MILOS.

### 10.3 Pipeline CI/CD Operativa (GitOps)

Lo sviluppo procede in totale automazione in ottica "Push-to-Deploy":

- **Continuous Integration:** Ad ogni pull-request su branch protetto, *GitHub Actions* o *Azure DevOps* provvede alla compilazione asincrona, esegue il linter automatico su React (`eslint/prettier`), fa correre i Test di Unità .NET, valuta la *Test Coverage*, e controlla che l'albero non includa CVE gravi su dipendenze NuGet o NPM.
- **Continuous Deployment:** Se la CI passa, generano i tag semantici (es: `v2.2.14`). I manifesti Helm / Kustomize vengono automaticamente alterati sui cluster di Staging per simulazioni inter-modulari contro Sandbox MILOS.
- **Prometheus & Grafana:** Metriche telemetriche come *Consumer Lag* (quanto tempo il bus è intasato) e tassi di errore sul Gateaway vengono esposte agli standard OpenTelemetry dai plugin interni, visualizzati da dashboard interattive.

---

## 11. Stato di Implementazione

Stato di avanzamento al **Febbraio 2026** — Fase 1 (MILOS TFP).

### 11.1 Moduli Backend Completati

| Modulo | Layer | Componente | Stato |
|---|---|---|---|
| AuditLog | Domain | `AuditLog`, `IAuditLogRepository`, `AuditEntityType`, `AuditActionType` | ✅ Completo |
| AuditLog | Infrastructure | `AuditLogRepository`, `AuditLogConfiguration`, registrazione `InfrastructureExtensions` | ✅ Completo |
| AuditLog | Application | `AuditLogDto`, `GetAuditLogsQuery`, `GetAuditLogsQueryHandler` | ✅ Completo |
| AuditLog | QueryProxyService | `AuditQueryEndpoints` (`GET /api/query/audit-logs`, `GET /api/query/audit-logs/{id}`) | ✅ Completo |
| Gateway Resilienza | Shared.Infrastructure | `GatewayResilienceHandler` (Polly v8 `DelegatingHandler`) | ✅ Completo |
| Gateway Resilienza | Gateway.Milos | `MilosGatewayExtensions` — handler wired prima di `MilosApiKeyHandler` | ✅ Completo |
| Gateway Resilienza | Gateway.EftiNative | `EftiNativeGatewayExtensions` — handler wired prima di `EftiOAuth2Handler` | ✅ Completo |
| Health Monitoring | EftiGatewayService | `GatewayHealthMonitor` (BackgroundService, 60s, `HealthCheckAsync` su MILOS e EFTI_NATIVE) | ✅ Completo |
| Unit Test MILOS | Gateway.Milos.Tests | `MilosHashcodeCalculatorTests` (5), `EcmrPayloadToMilosMapperTests` (9), `MilosTfpGatewayTests` (7) | ✅ Completo |

### 11.2 Pipeline di Elaborazione Fase 1

| Step | Servizio | Evento MassTransit | Stato |
|---|---|---|---|
| 1 · Ingestione | `ApiGatewayService` | pubblica `TransportSubmittedEvent` | ✅ Implementato |
| 2 · Validazione | `ValidationService` | consuma → pubblica `TransportValidatedEvent` | ✅ Implementato |
| 3 · Normalizzazione | `NormalizationService` | consuma → pubblica `EftiSendRequestedEvent` | ✅ Implementato |
| 4 · Invio Gateway | `EftiGatewayService` | consuma → pubblica `EftiResponseReceivedEvent` | ✅ Implementato |
| 5 · Gestione Risposta | `ResponseHandlerService` | consuma → pubblica `SourceNotificationRequiredEvent` | ✅ Implementato |
| 6 · Notifica | `NotificationService` | consuma → webhook HTTP + SSE | ✅ Implementato |
| 7 · Retry / DLQ | `RetryService` | backoff esponenziale 1m→24h, max 6 tentativi | ✅ Implementato |

### 11.3 Stato Test

| Progetto Test | Tipo | N° Test | Stato |
|---|---|---|---|
| `Gateway.Milos.Tests` | Unit | 21 | ✅ Completo |
| `Gateway.EftiNative.Tests` | Unit | — | 🔄 Placeholder |
| `Application.Tests` | Unit | — | 🔄 Placeholder |
| `IntegrationTests` | Integration | — | 🔄 Placeholder |

### 11.4 Prossimi Passi

- Implementare test di integrazione con Testcontainers (MariaDB + RabbitMQ)
- Completare unit test per `Gateway.EftiNative` e layer `Application`
- Implementare `EftiNativeGateway` completo (Fase 2)
- Aggiungere audit writing automatico negli handler di dominio
- Frontend React: integrare endpoint AuditLog nel modulo Audit Log

---

*EFTI Connector Platform — Documentazione Architetturale v2.2 — Febbraio 2026*  
*Sezione 4 aggiornata per rispecchiare il modello dati implementato (12 tabelle, EF Core 9 + Pomelo + MariaDB 11.4)*  
*Sezione 5 aggiornata: GatewayResilienceHandler (Polly v8), GatewayHealthMonitor, Query Proxy Service endpoint AuditLog*  
*Sezione 11 aggiunta: Stato di Implementazione al Febbraio 2026*  
*Fase 1: Integrazione MILOS TFP (Circle SpA) · Fase 2: Integrazione Diretta EFTI Gate Nazionale*
