# EFTI Connector Hub — Analisi Tecnica Fase 2: Integrazione Diretta EFTI Gate

> **Versione:** 1.1 · **Data:** Febbraio 2026  
> **Riferimento:** [EFTI4EU/reference-implementation](https://github.com/EFTI4EU/reference-implementation) · Release `v0.5.0` (7 Gennaio 2025)  
> **Scopo:** Documento di analisi dell'implementazione di riferimento eFTI4EU, stato del Gate italiano e mappatura degli impatti sul codebase per il completamento di `EftiNativeGateway` (Fase 2).

---

## Indice

- [1. Il Repository di Riferimento eFTI4EU](#1-il-repository-di-riferimento-efti4eu)
- [2. Architettura del Reference Implementation](#2-architettura-del-reference-implementation)
- [3. Stack Tecnologico e Componenti](#3-stack-tecnologico-e-componenti)
- [4. Protocolli e Standard Tecnici](#4-protocolli-e-standard-tecnici)
- [5. Schema Dati EN 17532](#5-schema-dati-en-17532)
- [6. Flusso di Comunicazione Gate-to-Gate](#6-flusso-di-comunicazione-gate-to-gate)
- [7. Italia e l'EFTI Gate Nazionale](#7-italia-e-lefti-gate-nazionale)
- [8. Come Connettersi al Gate Italiano — Guida Pratica](#8-come-connettersi-al-gate-italiano--guida-pratica)
- [9. Analisi dello Stato Attuale — EftiNativeGateway](#9-analisi-dello-stato-attuale--eftinativegateway)
- [10. Gap Analysis — Cosa Rimane da Fare](#10-gap-analysis--cosa-rimane-da-fare)
- [11. Piano di Implementazione](#11-piano-di-implementazione)
- [12. Impatti su Altri Layer del Sistema](#12-impatti-su-altri-layer-del-sistema)
- [13. Configurazione Aggiornata](#13-configurazione-aggiornata)
- [14. Test e Validazione](#14-test-e-validazione)

---

## 1. Il Repository di Riferimento eFTI4EU

Il repository [EFTI4EU/reference-implementation](https://github.com/EFTI4EU/reference-implementation) è il **codice sorgente ufficiale** dell'implementazione di riferimento per il Regolamento EU 2020/1056 (eFTI). È sviluppato e mantenuto da diversi Stati Membri partecipanti al progetto eFTI4EU ed è distribuito sotto licenza **Apache-2.0**.

### 1.1 Scopo e Limiti Dichiarati

| Caratteristica | Dettaglio |
|---|---|
| **Release attuale** | `v0.5.0` (7 Gennaio 2025) — ancora in progress |
| **Uso dichiarato** | Base di codice per implementazioni nazionali, riferimento architetturale, test di interoperabilità |
| **Uso vietato** | Produzione diretta senza adattamenti nazionali |
| **Linguaggio primario** | Java 90%, TypeScript 5%, HTML 2.7%, XSLT 1.5% |
| **Autenticazione** | OpenID Connect (limitata — da adattare per produzione) |

> ⚠️ **Nota critica:** Il reference implementation usa **Java** (Spring Boot, presunto) e **Domibus** come access point AS4. Per il nostro Connector Hub in **.NET 9** sarà necessario adattare i protocolli e non copiare il codice direttamente.

### 1.2 Struttura del Repository

```
reference-implementation/
├── implementation/
│   ├── gate/                     ← Servizio principale EFTI Gate (Spring Boot Java)
│   ├── registry-of-identifiers/  ← Registro UUID dei documenti eFTI
│   ├── edelivery-ap-connector/   ← Connettore AS4 via Domibus
│   ├── efti-ws-plugin/           ← Web Service Client (interfaccia per TFP/Piattaforme)
│   ├── commons/                  ← Libreria condivisa (modelli, utilities)
│   ├── efti-logger/              ← Logging centralizzato
│   ├── platform-gate-simulator/  ← Simulatore piattaforma/gate per testing
│   └── test-support/             ← Utilities per test di integrazione
│
├── schema/
│   ├── xsd/                      ← Data models EN 17532 (XSD)
│   └── api-schemas/              ← Definizioni REST API (OpenAPI)
│
├── deploy/
│   ├── local/
│   │   ├── efti-gate/            ← Docker Compose per il gate locale
│   │   └── domibus/              ← Configurazione Domibus AS4
│
└── utils/
    └── postman/                  ← Collection Postman per test
```

---

## 2. Architettura del Reference Implementation

### 2.1 Topologia Multi-Gate

Il modello eFTI prevede una **rete di Gate nazionali** interconnessi. Ogni Stato Membro gestisce almeno un EFTI Gate. Le comunicazioni tra gate avvengono via AS4 (eDelivery).

```
┌───────────────────────────────────────────────────────────────────────┐
│  EFTI Network                                                         │
│                                                                       │
│  ┌──────────────┐   AS4 / eDelivery   ┌──────────────┐               │
│  │  Gate IT     │◄──────────────────►│  Gate DE     │               │
│  │  (EFTI Gate  │                     │  (EFTI Gate  │               │
│  │   Italiano)  │◄──────────────────►│   Tedesco)   │               │
│  └──────┬───────┘   AS4 / eDelivery   └──────┬───────┘               │
│         │ REST / HTTP                        │ REST / HTTP           │
│         ▼                                    ▼                       │
│  ┌──────────────┐                    ┌──────────────┐                │
│  │  TFP/        │                    │  TFP/        │                │
│  │  Piattaforma │                    │  Piattaforma │                │
│  │  (es. ns.    │                    │  (es. MILOS) │                │
│  │   Connector) │                    │              │                │
│  └──────────────┘                    └──────────────┘                │
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │  Registry of Identifiers  (UID lookup condiviso tra gate)    │    │
│  └──────────────────────────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────────────┘
```

### 2.2 Due Layer di Comunicazione Distinti

| Layer | Direzione | Protocollo | Chi comunica |
|---|---|---|---|
| **Piattaforma → Gate** | TFP → Gate nazionale | REST/HTTP + mTLS + OAuth2 | Il nostro Connector → Gate IT |
| **Gate → Gate** | Gate IT → Gate DE | eDelivery / AS4 (Domibus) | Gate IT → Gate DE |

> ✅ **Implicazione diretta per noi:** Il nostro `EftiNativeGateway` implementa il layer **Piattaforma → Gate**. Il protocollo AS4 è **interno al Gate** e non è nostra responsabilità.

---

## 3. Stack Tecnologico e Componenti

### 3.1 Reference Implementation (Java)

| Componente | Tecnologia | Equivalente .NET nel nostro progetto |
|---|---|---|
| Gate Core | Spring Boot 3.x | `EftiNativeGateway` + nuovi servizi |
| eDelivery Connector | Domibus 5.x (AS4) | **Non necessario** — il gate gestisce AS4 internamente |
| Registry | Spring Boot + DB | Client REST per chiamate al registry |
| WS Plugin | Apache CXF (SOAP) | `IEftiGateClient` (Refit REST) |
| Data Models | XSD (EN 17532) | `EftiEcmrDataset` (presente e completo) |
| Auth | OpenID Connect | `EftiOAuth2Handler` (presente con JWT RFC 7523) |
| AS4 Transport | Domibus | **Non riguarda il Connector Hub** |

### 3.2 API Esposte dal Gate (schema/api-schemas/)

```
POST   /datasets                       ← Invia nuovo dataset eFTI (202 Accepted)
PUT    /datasets/{id}                  ← Aggiorna dataset esistente
DELETE /datasets/{id}                  ← Cancella dataset
GET    /datasets/{id}                  ← Recupera dataset completo
GET    /datasets/{id}/status           ← Polling stato (RECEIVED/PROCESSING/…)
POST   /notifications/register         ← Registra callback URL per notifiche asincrone
GET    /health                         ← Health check
GET    /identifiers/{uid}              ← Lookup nel Registry of Identifiers
```

---

## 4. Protocolli e Standard Tecnici

### 4.1 Formato UID (Identificatore Univoco eFTI)

```
{countryCode}.{platformId}.{datasetType}.{uniqueId}

Esempio: IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123
```

| Parte | Descrizione | Esempio |
|---|---|---|
| `countryCode` | ISO 3166-1 alpha-2 | `IT` |
| `platformId` | Identificatore della piattaforma TFP | `EFTI_CONNECTOR` |
| `datasetType` | Tipo documento | `ECMR` |
| `uniqueId` | ID univoco della piattaforma | `CMR-2026-00123` |

### 4.2 Autenticazione OAuth2 + eIDAS

| Modalità | Quando | Come |
|---|---|---|
| **client_credentials + client_secret** | Sviluppo / test | `grant_type=client_credentials&client_id=X&client_secret=Y` |
| **client_credentials + client_assertion** | Produzione (eIDAS) | JWT RS256 firmato con chiave privata X.509 (RFC 7523) |

### 4.3 Mutual TLS (mTLS)

Il client (nostro Connector) presenta il certificato X.509 durante l'handshake TLS verso il Gate. Ora configurato nell'`HttpClientHandler` di `EftiNativeGatewayExtensions`.

### 4.4 Comunicazione Asincrona e Callbacks

Il Gate risponde con `202 Accepted` all'invio. L'esito viene comunicato tramite:
1. **Callback HTTP**: Il Gate chiama `POST {WebhookCallbackUrl}` con il nuovo stato
2. **Polling**: `GET /datasets/{id}/status` come fallback (ora presente in `IEftiGateClient`)

---

## 5. Schema Dati EN 17532

### 5.1 Dataset Type Supportati

| Dataset Type | Documento | `typeCode` |
|---|---|---|
| eCMR | Lettera di Vettura Elettronica (Strada) | `ECMR` |
| eAWB | Air Waybill (Aereo) | `EAWB` |
| eBL | Bill of Lading (Mare) | `EBL` |
| eRSD | Rail Supplementary Document (Ferrovia) | `ERSD` |
| eDAD | Dispatching Advice Document | `EDAD` |

### 5.2 Confronto `EftiEcmrDataset` vs EN 17532

| Campo EN 17532 | In `EftiEcmrDataset` | Stato |
|---|---|---|
| `id` (formato UID) | `Id` — ora generato da `EftiUidGenerator` | ✅ Corretto |
| `typeCode` | `TypeCode` | ✅ |
| `issueDateTime` | `IssueDateTime` | ✅ |
| `consignor` | `Consignor` | ✅ |
| `consignee` | `Consignee` | ✅ |
| `carriers[]` | `Carriers` (con `tractorPlate`) | ✅ |
| `acceptanceLocation` | `AcceptanceLocation` | ✅ |
| `deliveryLocation` | `DeliveryLocation` | ✅ |
| `consignmentItems` + `packages[]` | `EftiGoods` + `EftiPackage` | ✅ |
| `transportDetails` | `TransportDetails` | ✅ |
| `hashcode` | `Hashcode` | ✅ |

---

## 6. Flusso di Comunicazione Gate-to-Gate

### 6.1 Flusso Completo Fase 2

```
Connector Hub                    EFTI Gate IT              EFTI Gate DE
     │                                │                          │
     │  POST /datasets                │                          │
     │  (EftiEcmrDataset JSON)        │                          │
     │──────────────────────────────►│                          │
     │                                │                          │
     │  202 Accepted                  │                          │
     │  { messageId, status:RECEIVED }│                          │
     │◄──────────────────────────────│                          │
     │                                │  Schema validation       │
     │                                │  UID registration        │
     │                                │  AS4 (Domibus) ─────────►│
     │                                │  AS4 ACK ◄──────────────│
     │  POST {WebhookCallbackUrl}     │                          │
     │  { status: ACKNOWLEDGED }      │                          │
     │◄──────────────────────────────│                          │
```

### 6.2 Stati del Dataset nel Gate

| Stato | Significato |
|---|---|
| `RECEIVED` | Gate ha ricevuto (202 Accepted) |
| `PROCESSING` | Gate sta validando EN 17532 |
| `VALIDATED` | Validazione OK |
| `FORWARDED` | Inviato via AS4 al gate destinatario |
| `ACKNOWLEDGED` | Gate destinatario ha confermato |
| `ERROR` | Errore validazione o routing |

---

## 7. Italia e l'EFTI Gate Nazionale

### 7.1 Autorità Responsabile e Contesto Normativo

Il **Ministero delle Infrastrutture e dei Trasporti (MIT)** è l'autorità competente italiana per l'implementazione del Regolamento EU 2020/1056 eFTI. L'Italia ha partecipato attivamente al progetto eFTI4EU (programma ISA²) che ha prodotto il reference implementation, e sarà tra i primi stati a disporre di un gate operativo.

**Timeline normativa:**

| Data | Evento |
|---|---|
| 21 Agosto 2020 | Entrata in vigore Regolamento EU 2020/1056 |
| 21 Agosto 2024 | Data di applicazione — obbligatorietà accettazione eFTI da parte delle autorità |
| 2025 – 2026 | Deployment gate nazionali EU (fase di rollout progressivo) |
| **2026** | **Gate italiano atteso in produzione / sandbox avanzata** |

> ⚠️ **Stato al febbraio 2026:** Il gate italiano è in fase di deployment avanzato. Le URL definitive di produzione sono da confermare con il MIT. Per lo sviluppo usare il simulatore del reference implementation o la sandbox ufficiale quando disponibile.

### 7.2 Stack Tecnico del Gate Italiano

Il gate italiano è basato sul reference implementation eFTI4EU, con adattamenti nazionali. Lo stack atteso:

| Componente | Tecnologia | Note |
|---|---|---|
| **Gate Core** | Spring Boot 3.x (Java) | Basato su `implementation/gate/` del ref. impl. |
| **AS4 Access Point** | Domibus 5.x | Utilizzato per comunicazioni gate-to-gate EU |
| **Identity Provider** | Sistema IAM MIT / OAuth2 | Compatibile OpenID Connect — token M2M per TFP |
| **PKI / Certificati** | CA Accreditata eIDAS / AgID | Certificati X.509 qualificati per mTLS e firma |
| **Registry UID** | `registry-of-identifiers` | Condiviso con gli altri gate EU |
| **API TFP** | REST/HTTP su `efti-ws-plugin` | Interfaccia che il nostro Connector chiama |

### 7.3 Endpoint Attesi del Gate Italiano

| Ambiente | Base URL (attesa) | Stato |
|---|---|---|
| **Produzione** | `https://efti-gate.mit.gov.it/api/` | Da confermare MIT — 2026 |
| **Sandbox / UAT** | `https://efti-sandbox.mit.gov.it/api/` | Da confermare MIT |
| **Token endpoint** | `https://auth.efti.mit.gov.it/oauth2/token` | Da confermare |
| **Simulatore locale** | `http://localhost:8080` | Disponibile via Docker (ref. impl.) |

> 📧 **Contatto ufficiale MIT per accreditamento TFP:** La registrazione della piattaforma (`PlatformId`) e il rilascio delle credenziali OAuth2 avviene tramite il portale MIT / modulo di accreditamento. Verificare aggiornamenti su [mit.gov.it](https://mit.gov.it).

### 7.4 Processo di Accreditamento come TFP (Transport Freight Platform)

Per accedere al gate italiano in produzione, una TFP deve:

1. **Registrazione ufficiale:** Inviare richiesta al MIT con dati aziendali e tecnici della piattaforma.
2. **Assegnazione PlatformId:** Il MIT assegna un `PlatformId` unico (es. `EFTI_CONNECTOR`) che diventa parte dell'UID di ogni documento.
3. **Rilascio credenziali OAuth2:** Il MIT fornisce `ClientId` e, per test, eventuale `ClientSecret`. In produzione si usa `client_assertion` con X.509.
4. **Certificato X.509 per mTLS:** Ottenere un certificato qualificato eIDAS da una CA accreditata AgID (es. Infocert, Aruba, Namirial). Il certificato deve essere registrato presso il MIT.
5. **Test in sandbox:** Validare il flusso completo (invio → callback ACKNOWLEDGED) contro la sandbox MIT.
6. **Go-live produzione:** Firma dell'accordo di interconnessione e attivazione.

### 7.5 Formato UID per l'Italia

```
IT.{PlatformId}.{DatasetType}.{OperationCode}

Esempio:  IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123
```

Questo formato è ora **generato automaticamente** da `EftiUidGenerator` in base a `CountryCode` e `PlatformId` da `appsettings.json`.

### 7.6 Differenze tra Gate Italiano e Reference Implementation

| Aspetto | Reference Implementation | Gate Italiano (atteso) |
|---|---|---|
| **Autenticazione** | OpenID Connect semplificato | OAuth2 + mTLS con PKI AgID / eIDAS |
| **Certificati** | Auto-signed (solo test) | X.509 qualificati da CA accreditata AgID |
| **Scopo** | Interoperabilità EU multi-stato | Gateway nazionale IT + forwarding EU |
| **API** | REST/HTTP (ref. impl.) | REST/HTTP — stessa interfaccia |
| **Monitoring** | Limitato | Integrazione con BDAP MIT / Registro Nazionale |

### 7.7 Integrazione con Altri Gate EU

Quando un trasporto eCMR attraversa i confini (es. Italia → Germania), il Gate IT effettua automaticamente:

1. Lookup nel Registry of Identifiers per trovare il Gate competente per il paese di destinazione.
2. Invio del documento via AS4 / eDelivery al Gate DE.
3. Ricezione dell'ACK dal Gate DE.
4. Notifica callback al nostro Connector con status `ACKNOWLEDGED`.

**Non è nostra responsabilità** gestire il routing inter-gate: il Gate IT lo fa in autonomia.

---

## 8. Come Connettersi al Gate Italiano — Guida Pratica

### 8.1 Fase di Sviluppo: Simulatore Locale

```bash
# 1. Clona il reference implementation
git clone https://github.com/EFTI4EU/reference-implementation.git
cd reference-implementation

# 2. Avvia il simulatore gate + auth locale
cd deploy/local/efti-gate
docker compose up -d

# Il simulatore espone:
# Gate REST API:   http://localhost:8080
# Auth (token):    http://localhost:8090/oauth2/token
# Portale gate:    http://localhost:8081
```

Configurazione `appsettings.Development.json` puntando al simulatore:

```json
"EftiGateway": {
  "Provider": "EftiNative",
  "EftiNative": {
    "BaseUrl":           "http://localhost:8080/",
    "TokenEndpoint":     "http://localhost:8090/oauth2/token",
    "ClientId":          "efti-connector-dev",
    "ClientSecret":      "dev-secret",
    "UseClientAssertion": false,
    "CountryCode":       "IT",
    "PlatformId":        "EFTI_CONNECTOR",
    "WebhookCallbackUrl": "http://localhost:5090/api/efti/callback",
    "TimeoutSeconds":    30
  }
}
```

### 8.2 Fase di Test: Sandbox MIT

Configurazione per sandbox MIT (valori da sostituire con quelli reali comunicati dal MIT):

```json
"EftiGateway": {
  "Provider": "EftiNative",
  "EftiNative": {
    "BaseUrl":            "https://efti-sandbox.mit.gov.it/api/",
    "TokenEndpoint":      "https://auth.efti-sandbox.mit.gov.it/oauth2/token",
    "ClientId":           "<assegnato dal MIT>",
    "ClientSecret":       "<assegnato dal MIT — solo sandbox>",
    "CertificatePath":    "certs/sandbox-efti-client.pfx",
    "CertificatePassword": "<da vault>",
    "UseClientAssertion": false,
    "CountryCode":        "IT",
    "PlatformId":         "<PlatformId assegnato dal MIT>",
    "WebhookCallbackUrl": "https://connector-sandbox.azienda.it/api/efti/callback",
    "TimeoutSeconds":     30
  }
}
```

### 8.3 Fase di Produzione: Gate MIT

```json
"EftiGateway": {
  "Provider": "EftiNative",
  "EftiNative": {
    "BaseUrl":            "https://efti-gate.mit.gov.it/api/",
    "TokenEndpoint":      "https://auth.efti.mit.gov.it/oauth2/token",
    "ClientId":           "<assegnato dal MIT>",
    "UseClientAssertion": true,
    "CertificatePath":    "<da vault — certificato X.509 qualificato eIDAS>",
    "CertificatePassword": "<da vault>",
    "CountryCode":        "IT",
    "PlatformId":         "<PlatformId assegnato dal MIT>",
    "WebhookCallbackUrl": "https://connector.azienda.it/api/efti/callback",
    "TimeoutSeconds":     30
  }
}
```

> In produzione `ClientSecret` è `null` e `UseClientAssertion = true`. Il JWT `client_assertion` viene costruito da `EftiOAuth2Handler` usando la chiave privata RSA del certificato `.pfx`.

---

## 9. Analisi dello Stato Attuale — EftiNativeGateway

### 9.1 Cosa è ora implementato ✅

| Componente | File | Stato |
|---|---|---|
| Interfaccia gateway | `EftiNativeGateway.cs` | ✅ Completo — inietta `IOptions<EftiNativeOptions>` |
| Client Refit | `IEftiGateClient.cs` | ✅ Completo — CRUD + status polling + callback registration |
| OAuth2 handler | `EftiOAuth2Handler.cs` | ✅ Completo — `client_secret` + `client_assertion` JWT (RFC 7523) |
| Token cache Redis | `EftiTokenCache.cs` | ✅ Completo |
| Caricamento X.509 | `X509CertificateLoader.cs` | ✅ Completo |
| mTLS sull'HttpClient | `EftiNativeGatewayExtensions.cs` | ✅ **Aggiunto** — `ConfigurePrimaryHttpMessageHandler` |
| Generatore UID | `EftiUidGenerator.cs` | ✅ **Creato** — formato `CC.PlatformId.DatasetType.UniqueId` |
| Mapper EN 17532 | `EcmrPayloadToEftiMapper.cs` | ✅ **Aggiornato** — usa `EftiUidGenerator` |
| Modello status polling | `EftiDatasetStatus.cs` | ✅ **Creato** — stati `RECEIVED/PROCESSING/.../ACKNOWLEDGED` |
| Configurazione | `EftiNativeOptions.cs` | ✅ **Aggiornato** — tutti i campi presenti |
| File vuoto duplicato | `Models/EftiEcmrDataset.cs` | ✅ **Eliminato** |
| `GatewaySelector` | `GatewaySelector.cs` | ✅ **Aggiornato** — inietta `IOptions<EftiNativeOptions>` |

---

## 10. Gap Analysis — Cosa Rimane da Fare

### Priorità Alta 🔴

#### 10.1 Endpoint Inbound per Notifiche Callback del Gate

Il Gate invia una `POST` al `WebhookCallbackUrl` quando lo stato del dataset cambia a `ACKNOWLEDGED` o `ERROR`. Nessun endpoint riceve ancora questa notifica.

**File da creare:**
```
src/Services/ilp_efti_connector.EftiGatewayService/Controllers/EftiCallbackController.cs
src/Shared/ilp_efti_connector.Shared.Contracts/Events/EftiAcknowledgeReceivedEvent.cs
```

**Cambio architetturale:** `EftiGatewayService` deve esporre Kestrel HTTP (aggiungere `WebApplication` builder in `Program.cs`) per ricevere i callback.

#### 10.2 Consumer `EftiAcknowledgeReceivedEvent` in `ResponseHandlerService`

Quando il callback arriva, deve essere pubblicato su RabbitMQ e consumato da `ResponseHandlerService` per aggiornare `TransportOperation.Status = ACKNOWLEDGED`.

---

### Priorità Media 🟡

#### 10.3 Polling di Stato come Fallback

Se il callback non arriva entro N minuti, il `RetryService` deve fare polling su `GET /datasets/{id}/status`. L'endpoint `GetDatasetStatusAsync` è già in `IEftiGateClient`.

#### 10.4 Registrazione Callback URL al Boot

Al boot di `EftiGatewayService` (o nel `GatewayHealthMonitor`), chiamare `RegisterCallbackAsync` per registrare `WebhookCallbackUrl` presso il Gate. Dipende dalla conferma da parte del MIT se questo endpoint è implementato nel gate italiano.

---

### Priorità Bassa 🟢

#### 10.5 Registry of Identifiers Lookup (Fase 2 avanzata)

Prima di inviare un dataset, verificare che l'UID non sia già presente nel Registry. Eventuale nuovo `IEftiRegistryClient`.

#### 10.6 Supporto XML (XSD) opzionale

Se il gate italiano richiede XML invece di JSON, aggiungere serializer XML configurabile via `EftiNativeOptions.UseXml`.

---

## 11. Piano di Implementazione

### Sprint 1 — Completato ✅ (questo sprint)

| Task | Stato |
|---|---|
| Eliminare file vuoto duplicato `Models/EftiEcmrDataset.cs` | ✅ |
| Aggiungere campi mancanti a `EftiNativeOptions` | ✅ |
| Creare `EftiUidGenerator` con formato `CC.PlatformId.DatasetType.UniqueId` | ✅ |
| Aggiornare mapper per usare l'UID generato | ✅ |
| Aggiungere mTLS all'`HttpClientHandler` | ✅ |
| Aggiungere `client_assertion` JWT a `EftiOAuth2Handler` (RFC 7523, BCL puro) | ✅ |
| Aggiungere `GetDatasetStatusAsync` a `IEftiGateClient` | ✅ |
| Aggiungere `RegisterCallbackAsync` a `IEftiGateClient` | ✅ |
| Creare `EftiDatasetStatus` model | ✅ |
| Fix `GatewaySelector` + test con nuovo costruttore | ✅ |

### Sprint 2 — Callback Asincrono (2-3 settimane)

| Task | Priorità |
|---|---|
| `EftiCallbackController` in `EftiGatewayService` (endpoint inbound) | 🔴 |
| `EftiAcknowledgeReceivedEvent` in `Shared.Contracts` | 🔴 |
| Consumer `EftiAcknowledgeReceivedConsumer` in `ResponseHandlerService` | 🔴 |
| Transizione stato `ACKNOWLEDGED` su `TransportOperation` | 🔴 |
| Registrazione callback URL al boot (`GatewayHealthMonitor`) | 🟡 |

### Sprint 3 — Produzione e Integrazione MIT (3-4 settimane)

| Task | Priorità |
|---|---|
| Polling fallback in `RetryService` (usa `GetDatasetStatusAsync`) | 🟡 |
| Test end-to-end con simulatore eFTI4EU via Testcontainers | 🟡 |
| Richiesta accreditamento TFP al MIT | 🔴 |
| Test contro sandbox MIT (quando disponibile) | 🔴 |

---

## 12. Impatti su Altri Layer del Sistema

### 12.1 Nuovo Evento `EftiAcknowledgeReceivedEvent` (`Shared.Contracts`)

```csharp
public record EftiAcknowledgeReceivedEvent(
    Guid   EftiMessageId,
    Guid   TransportOperationId,
    string CorrelationId,
    string GatewayProvider,     // "EFTI_NATIVE"
    string EftiUid,             // "IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123"
    string GateStatus,          // "ACKNOWLEDGED" | "ERROR"
    string? ErrorMessage,
    DateTime ReceivedAt
);
```

### 12.2 `EftiGatewayService` — Da Worker a Web API + Worker

```csharp
// Program.cs — cambia da:
var host = Host.CreateDefaultBuilder(args) ...
// a:
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();         // per EftiCallbackController
builder.Services.AddHostedService<...>(); // worker rimane
// ...
var app = builder.Build();
app.MapControllers();
app.Run();
```

Porta dedicata ai callback (es. `5090`) separata dall'API gateway (porta `5052`).

### 12.3 `ResponseHandlerService` — Nuova Transizione `ACKNOWLEDGED`

```csharp
// Nuovo consumer per EftiAcknowledgeReceivedEvent:
// 1. Carica EftiMessage per EftiMessageId
// 2. message.Status = ACKNOWLEDGED; message.AcknowledgedAt = evt.ReceivedAt
// 3. operation.Status = ACKNOWLEDGED
// 4. Pubblica SourceNotificationRequiredEvent con status=ACKNOWLEDGED
```

### 12.4 Schema DB — Nessuna Migration Necessaria

`efti_messages.external_id` (`VARCHAR 100`) è sufficiente per `IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123` (~50 char). La colonna `acknowledged_at` è già presente.

---

## 13. Configurazione Aggiornata

### 13.1 Tutti i Campi di `EftiNativeOptions` (aggiornati)

```json
"EftiGateway": {
  "Provider": "EftiNative",
  "EftiNative": {
    "BaseUrl":            "https://efti-gate.mit.gov.it/api/",
    "TokenEndpoint":      "https://auth.efti.mit.gov.it/oauth2/token",
    "ClientId":           "<assegnato dal MIT>",
    "ClientSecret":       null,
    "UseClientAssertion": true,
    "CertificatePath":    "<da vault>",
    "CertificatePassword": "<da vault>",
    "Scope":              "efti",
    "CountryCode":        "IT",
    "PlatformId":         "<assegnato dal MIT — es. EFTI_CONNECTOR>",
    "WebhookCallbackUrl": "https://connector.azienda.it/api/efti/callback",
    "TimeoutSeconds":     30
  }
}
```

### 13.2 Variabili da Ottenere dal MIT

| Variabile | Come ottenerla |
|---|---|
| `ClientId` | Modulo di accreditamento TFP MIT |
| `PlatformId` | Assegnato dal MIT al momento della registrazione |
| `BaseUrl` | Pubblicata sul portale MIT (attesa 2026) |
| `TokenEndpoint` | Pubblicata sul portale MIT |
| Certificato X.509 | CA accreditata AgID (Infocert, Aruba, Namirial, ecc.) |

---

## 14. Test e Validazione

### 14.1 Simulatore Locale (Disponibile Subito)

```bash
# Avvia il simulatore dal reference implementation
git clone https://github.com/EFTI4EU/reference-implementation.git
cd reference-implementation/deploy/local/efti-gate
docker compose up -d
```

### 14.2 Checklist Pre-Go-Live Fase 2

```
[ ] Accreditamento TFP completato — PlatformId e ClientId ottenuti dal MIT
[ ] Certificato X.509 qualificato ottenuto da CA accreditata AgID
[ ] mTLS verificato: GET /health risponde 200 con certificato client
[ ] client_assertion JWT testato: token ottenuto con X.509 signing
[ ] Formato UID verificato: IT.<PlatformId>.ECMR.<OperationCode>
[ ] WebhookCallbackUrl raggiungibile dal Gate (porta 5090, firewall in ingresso)
[ ] EftiCallbackController implementato e funzionante
[ ] Callback registration verificata (se supportato dal gate MIT)
[ ] Test end-to-end: POST /datasets → 202 → callback ACKNOWLEDGED ricevuto
[ ] Polling fallback: GET /datasets/{id}/status → ACKNOWLEDGED
[ ] Stato ACKNOWLEDGED propagato: transport_operations + efti_messages
[ ] Notifica webhook al TMS sorgente con status=ACKNOWLEDGED
[ ] GatewayHealthMonitor: log HEALTHY per EFTI_NATIVE in Seq
[ ] Regression test MILOS: path Fase 1 ancora funzionante come fallback
```

---

## Appendice — Differenze Chiave tra Fase 1 (MILOS) e Fase 2 (EFTI Gate IT)

| Aspetto | Fase 1 — MILOS | Fase 2 — EFTI Gate Italiano |
|---|---|---|
| **Autenticazione** | API Key (`X-API-Key` header) | OAuth2 client_credentials + mTLS X.509 |
| **Payload** | `ECMRRequest` JSON custom MILOS | `EftiEcmrDataset` EN 17532 JSON |
| **Identificatore** | `eCMRID` generato da MILOS | UID `IT.PlatformId.ECMR.UniqueId` |
| **Risposta invio** | Sincrona: `200 OK` + `eCMRID` | Asincrona: `202 Accepted` + callback |
| **QR Code** | Generato da MILOS | Generato dal Gate MIT |
| **Hash integrità** | SHA-256 in `hashcodeDetails.json` | SHA-256 in `hashcode.value` (EN 17532) |
| **Interoperabilità** | Limitata (solo paesi MILOS) | Multi-stato EU via AS4 inter-gate |
| **Autorità** | Interrogano MILOS | Interrogano il Gate MIT direttamente |
| **Costi ricorrenti** | Licenza commerciale Circle SpA | Zero (infrastruttura pubblica MIT) |

---

*Documento creato: Febbraio 2026 — v1.0*  
*Aggiornato: Febbraio 2026 — v1.1: Sezione 7 aggiunta (Italia e eFTI Gate), Sezione 8 aggiunta (guida connessione gate IT), sprint 1 completato (mTLS, UID generator, client_assertion JWT, status polling, gap risolti)*  
*Fonte: [EFTI4EU/reference-implementation](https://github.com/EFTI4EU/reference-implementation) — Release v0.5.0*  
*Aggiornare le URL del Gate italiano quando pubblicate ufficialmente dal MIT.*


> **Versione:** 1.0 · **Data:** Febbraio 2026  
> **Riferimento:** [EFTI4EU/reference-implementation](https://github.com/EFTI4EU/reference-implementation) · Release `v0.5.0` (7 Gennaio 2025)  
> **Scopo:** Documento di analisi dell'implementazione di riferimento eFTI4EU e mappatura degli impatti sul codebase per il completamento di `EftiNativeGateway` (Fase 2).

---

## Indice

- [1. Il Repository di Riferimento eFTI4EU](#1-il-repository-di-riferimento-efti4eu)
- [2. Architettura del Reference Implementation](#2-architettura-del-reference-implementation)
- [3. Stack Tecnologico e Componenti](#3-stack-tecnologico-e-componenti)
- [4. Protocolli e Standard Tecnici](#4-protocolli-e-standard-tecnici)
- [5. Schema Dati EN 17532](#5-schema-dati-en-17532)
- [6. Flusso di Comunicazione Gate-to-Gate](#6-flusso-di-comunicazione-gate-to-gate)
- [7. Analisi dello Stato Attuale — EftiNativeGateway](#7-analisi-dello-stato-attuale--eftinativegateway)
- [8. Gap Analysis — Cosa Manca](#8-gap-analysis--cosa-manca)
- [9. Piano di Implementazione](#9-piano-di-implementazione)
- [10. Impatti su Altri Layer del Sistema](#10-impatti-su-altri-layer-del-sistema)
- [11. Configurazione e Certificati](#11-configurazione-e-certificati)
- [12. Test e Validazione](#12-test-e-validazione)

---

## 1. Il Repository di Riferimento eFTI4EU

Il repository [EFTI4EU/reference-implementation](https://github.com/EFTI4EU/reference-implementation) è il **codice sorgente ufficiale** dell'implementazione di riferimento per il Regolamento EU 2020/1056 (eFTI). È sviluppato e mantenuto da diversi Stati Membri partecipanti al progetto eFTI4EU ed è distribuito sotto licenza **Apache-2.0**.

### 1.1 Scopo e Limiti Dichiarati

| Caratteristica | Dettaglio |
|---|---|
| **Release attuale** | `v0.5.0` (7 Gennaio 2025) — ancora in progress |
| **Uso dichiarato** | Base di codice per implementazioni nazionali, riferimento architetturale, test di interoperabilità |
| **Uso vietato** | Produzione diretta senza adattamenti nazionali |
| **Linguaggio primario** | Java 90%, TypeScript 5%, HTML 2.7%, XSLT 1.5% |
| **Autenticazione** | OpenID Connect (limitata — da adattare per produzione) |

> ⚠️ **Nota critica:** Il reference implementation usa **Java** (Spring Boot, presunto) e **Domibus** come access point AS4. Per il nostro Connector Hub in **.NET 9** sarà necessario adattare i protocolli e non copiare il codice direttamente.

### 1.2 Struttura del Repository

```
reference-implementation/
├── implementation/
│   ├── gate/                     ← Servizio principale EFTI Gate (Spring Boot Java)
│   ├── registry-of-identifiers/  ← Registro UUID dei documenti eFTI
│   ├── edelivery-ap-connector/   ← Connettore AS4 via Domibus
│   ├── efti-ws-plugin/           ← Web Service Client (interfaccia per TFP/Piattaforme)
│   ├── commons/                  ← Libreria condivisa (modelli, utilities)
│   ├── efti-logger/              ← Logging centralizzato
│   ├── platform-gate-simulator/  ← Simulatore piattaforma/gate per testing
│   └── test-support/             ← Utilities per test di integrazione
│
├── schema/
│   ├── xsd/                      ← Data models EN 17532 (XSD)
│   └── api-schemas/              ← Definizioni REST API (OpenAPI)
│
├── deploy/
│   ├── local/
│   │   ├── efti-gate/            ← Docker Compose per il gate locale
│   │   └── domibus/              ← Configurazione Domibus AS4
│
└── utils/
    └── postman/                  ← Collection Postman per test
```

---

## 2. Architettura del Reference Implementation

### 2.1 Topologia Multi-Gate

Il modello eFTI prevede una **rete di Gate nazionali** interconnessi. Ogni Stato Membro gestisce almeno un EFTI Gate. Le comunicazioni tra gate avvengono via AS4 (eDelivery).

```
┌───────────────────────────────────────────────────────────────────────┐
│  EFTI Network                                                         │
│                                                                       │
│  ┌──────────────┐   AS4 / eDelivery   ┌──────────────┐               │
│  │  Gate IT     │◄──────────────────►│  Gate DE     │               │
│  │  (EFTI Gate  │                     │  (EFTI Gate  │               │
│  │   Italiano)  │◄──────────────────►│   Tedesco)   │               │
│  └──────┬───────┘   AS4 / eDelivery   └──────┬───────┘               │
│         │ REST / WS                          │ REST / WS             │
│         ▼                                    ▼                       │
│  ┌──────────────┐                    ┌──────────────┐                │
│  │  TFP/        │                    │  TFP/        │                │
│  │  Piattaforma │                    │  Piattaforma │                │
│  │  (es. ns.    │                    │  (es. MILOS) │                │
│  │   Connector) │                    │              │                │
│  └──────────────┘                    └──────────────┘                │
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐    │
│  │  Registry of Identifiers  (UID lookup condiviso tra gate)    │    │
│  └──────────────────────────────────────────────────────────────┘    │
└───────────────────────────────────────────────────────────────────────┘
```

### 2.2 Due Layer di Comunicazione Distinti

Il reference implementation distingue nettamente due layer di comunicazione:

| Layer | Direzione | Protocollo | Chi comunica |
|---|---|---|---|
| **Piattaforma → Gate** | TFP → Gate nazionale | REST/HTTP o WS SOAP (webservice plugin) | Il nostro Connector → Gate IT |
| **Gate → Gate** | Gate IT → Gate DE | eDelivery / AS4 (Domibus) | Gate IT → Gate DE |

> ✅ **Implicazione diretta per noi:** Il nostro `EftiNativeGateway` implementa il layer **Piattaforma → Gate**, non il Gate → Gate. Il protocollo AS4 è **interno al Gate** e non è nostra responsabilità. Dobbiamo implementare l'API REST del Gate (o il WS Plugin), **non** AS4 direttamente.

### 2.3 Componenti Principali del Gate Core

Il **gate core** (`implementation/gate/`) espone:

1. **API REST per piattaforme TFP**: Endpoint per submit/update/delete/query di dataset eFTI.
2. **Gestione asincrona**: Il gate risponde `202 Accepted` e processa il dataset in modo asincrono.
3. **Registry Lookup**: Recupera gli identificatori eFTI dal registry condiviso.
4. **Stato dei dataset**: Tracking del ciclo di vita `RECEIVED → PROCESSING → VALIDATED → FORWARDED`.
5. **Notifiche callback**: Invia notifiche HTTP alla piattaforma sorgente quando lo stato cambia.

---

## 3. Stack Tecnologico e Componenti

### 3.1 Reference Implementation (Java)

| Componente | Tecnologia | Equivalente .NET nel nostro progetto |
|---|---|---|
| Gate Core | Spring Boot 3.x | `EftiNativeGateway` + nuovi servizi |
| eDelivery Connector | Domibus 5.x (AS4) | **Non necessario** — il gate gestisce AS4 internamente |
| Registry | Spring Boot + DB | Client REST per chiamate al registry |
| WS Plugin | Apache CXF (SOAP) | `IEftiGateClient` (Refit REST) oppure nuovo client WS |
| Data Models | XSD (EN 17532) | `EftiEcmrDataset` (già presente, da completare) |
| Auth | OpenID Connect | `EftiOAuth2Handler` (già presente, da completare) |
| AS4 Transport | Domibus | **Non riguarda il Connector Hub** |

### 3.2 Interfacce API Esposte dal Gate (schema/api-schemas/)

Dall'analisi del reference implementation, il Gate espone le seguenti API REST:

```
POST   /datasets                       ← Invia nuovo dataset eFTI
PUT    /datasets/{id}                  ← Aggiorna dataset esistente  
DELETE /datasets/{id}                  ← Cancella dataset
GET    /datasets/{id}                  ← Recupera dataset
GET    /datasets/{id}/status           ← Stato del dataset
GET    /datasets?query=...             ← Ricerca dataset (per autorità)
POST   /notifications/register         ← Registra callback URL per notifiche
GET    /health                         ← Health check
GET    /identifiers/{uid}              ← Lookup nel Registry of Identifiers
```

> **Nota:** L'API REST effettiva potrebbe variare nella versione definitiva del gate nazionale italiano. Le specifiche ufficiali saranno pubblicate dal Ministero competente. Usare il reference implementation come **base di riferimento**.

---

## 4. Protocolli e Standard Tecnici

### 4.1 Formato Payload — EN 17532

Lo standard **EN 17532** definisce il modello dati per i documenti di trasporto eFTI. Il reference implementation fornisce:

- **Schemi XSD** (`schema/xsd/`) — definizione formale dei campi
- **Trasformazioni XSLT** — conversione tra formati
- Il gate accetta sia **XML** (conforme XSD) che **JSON** (schema equivalente)

I dataset type supportati dalla normativa:

| Dataset Type | Documento | Campo `typeCode` |
|---|---|---|
| eCMR | Lettera di Vettura Elettronica (Strada) | `ECMR` |
| eAWB | Air Waybill Elettronico (Aereo) | `EAWB` |
| eBL | Bill of Lading Elettronico (Mare) | `EBL` |
| eRSD | Rail Supplementary Document (Ferrovia) | `ERSD` |
| eDAD | Dispatching Advice Document | `EDAD` |

### 4.2 Formato UID (Identificatore Univoco eFTI)

Il reference implementation definisce un formato specifico per gli identificatori:

```
{countryCode}.{platformId}.{datasetType}.{uniqueId}

Esempio: IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123
```

| Parte | Descrizione | Esempio |
|---|---|---|
| `countryCode` | ISO 3166-1 alpha-2 | `IT` |
| `platformId` | Identificatore della piattaforma TFP | `EFTI_CONNECTOR` |
| `datasetType` | Tipo documento | `ECMR` |
| `uniqueId` | ID univoco della piattaforma | `CMR-2026-00123` |

> **Impatto:** Il campo `Id` del dataset (`EftiEcmrDataset.Id`) deve rispettare questo formato. Attualmente inviamo solo `OperationCode`. Serve un generatore di UID.

### 4.3 Autenticazione OAuth2 + eIDAS

Il reference implementation usa OpenID Connect con due possibili modalità per il client:

| Modalità | Quando | Come |
|---|---|---|
| **client_credentials + client_secret** | Ambiente di sviluppo / test | `grant_type=client_credentials&client_id=X&client_secret=Y` |
| **client_credentials + client_assertion** | Produzione (eIDAS) | JWT firmato con chiave privata del certificato X.509 (`client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer`) |

> **Impatto:** `EftiOAuth2Handler` gestisce già il caso `client_secret`. Manca il supporto `client_assertion` con JWT firmato da X.509 (obbligatorio in produzione).

### 4.4 Mutual TLS (mTLS)

Per il canale HTTPS verso il Gate è richiesto **mTLS**:
- Il client (nostro Connector) presenta il proprio certificato X.509 durante l'handshake TLS.
- Il Gate verifica il certificato tramite la PKI nazionale.

> **Impatto:** `X509CertificateLoader` esiste già. Il `HttpClient` configurato in `EftiNativeGatewayExtensions` deve essere arricchito per aggiungere il certificato client all'`HttpClientHandler`.

### 4.5 Comunicazione Asincrona e Callbacks

Il Gate risponde con `202 Accepted` all'invio di un dataset. L'esito effettivo (VALIDATED / ERROR) viene comunicato in modo **asincrono** tramite:

1. **Callback HTTP (Webhook)**: Il Gate chiama un endpoint del nostro Connector con il nuovo stato.
2. **Polling**: `GET /datasets/{id}/status` per verificare lo stato (alternativo al callback).

Questo è un **cambiamento architetturale significativo** rispetto alla Fase 1 (MILOS risponde in modo sincrono con eCMRID).

---

## 5. Schema Dati EN 17532

### 5.1 Struttura del Dataset eCMR (Campi Principali)

Dalla definizione XSD del reference implementation:

```
EftiDataset
├── id                (UID format: CC.PlatformId.Type.UniqueId)
├── typeCode          (ECMR / EAWB / EBL / ERSD / EDAD)
├── issueDateTime     (ISO 8601)
├── consignor         (mittente)
│   ├── name
│   ├── taxId         (P.IVA)
│   ├── eoriCode      (EORI)
│   └── address       (streetName, postCode, cityName, countryCode)
├── consignee         (destinatario)
│   ├── name, taxId, eoriCode, address
│   └── playerType
├── carriers[]        (lista vettori)
│   ├── name, taxId, eoriCode, address
│   ├── tractorPlate  (OBBLIGATORIO per eCMR)
│   └── equipmentCategory
├── acceptanceLocation (luogo presa in carico)
│   ├── address
│   └── dateTime
├── deliveryLocation  (luogo consegna)
│   └── address
├── consignmentItems  (merci)
│   ├── totalWeight   (kg)
│   ├── totalVolume   (m³)
│   ├── totalQuantity
│   └── packages[]
│       ├── typeCode, quantity, grossWeight, grossVolume
│       └── shippingMarks
├── transportDetails
│   ├── cargoType     (FTL / LTL / GROUPAGE)
│   └── incoterms     (EXW / DAP / DDP / ...)
└── hashcode
    ├── value         (SHA-256 hex)
    └── algorithm     ("SHA-256")
```

### 5.2 Confronto con il Modello Attuale `EftiEcmrDataset`

| Campo EN 17532 | In `EftiEcmrDataset` | Stato |
|---|---|---|
| `id` (formato UID) | `Id` (solo `OperationCode`) | ⚠️ Formato non conforme |
| `typeCode` | `TypeCode` | ✅ |
| `issueDateTime` | `IssueDateTime` | ✅ |
| `consignor` | `Consignor` (EftiConsignor) | ✅ |
| `consignee` | `Consignee` (EftiConsignee) | ✅ |
| `carriers[]` | `Carriers` | ✅ (verifica `tractorPlate`) |
| `acceptanceLocation` | `AcceptanceLocation` | ✅ |
| `deliveryLocation` | `DeliveryLocation` | ✅ |
| `consignmentItems` + `packages[]` | `ConsignmentItems` (EftiGoods) | ⚠️ `EftiGoods` mancante nel file visto |
| `transportDetails` | `TransportDetails` | ✅ |
| `hashcode` | `Hashcode` | ✅ |
| **`status`** | assente | ❌ Mancante (richiesto per GET) |
| **`registryId`** | assente | ❌ Mancante (richiesto dal Registry) |

---

## 6. Flusso di Comunicazione Gate-to-Gate

### 6.1 Flusso Completo Fase 2 (con notifiche asincrone)

```
Connector Hub                    EFTI Gate IT              EFTI Gate DE
     │                                │                          │
     │  POST /datasets                │                          │
     │  (EftiEcmrDataset JSON/XML)    │                          │
     │──────────────────────────────►│                          │
     │                                │                          │
     │  202 Accepted                  │                          │
     │  { messageId, status:RECEIVED }│                          │
     │◄──────────────────────────────│                          │
     │                                │                          │
     │                                │ Schema validation        │
     │                                │ UID registration         │
     │                                │ (Registry of IDs)        │
     │                                │                          │
     │                                │  AS4 message (Domibus)   │
     │                                │─────────────────────────►│
     │                                │                          │
     │                                │  AS4 ACK                 │
     │                                │◄─────────────────────────│
     │                                │                          │
     │  POST /webhook                 │                          │
     │  { status: ACKNOWLEDGED }      │                          │
     │◄──────────────────────────────│                          │
     │                                │                          │
```

### 6.2 Stati del Dataset nel Gate

```
RECEIVED    → Il Gate ha ricevuto il dataset (202 Accepted)
PROCESSING  → Il Gate sta validando lo schema EN 17532
VALIDATED   → Validazione OK, dataset accettato
FORWARDED   → Inviato via AS4 al gate dello Stato destinatario
ACKNOWLEDGED → Gate destinatario ha confermato la ricezione
ERROR       → Errore di validazione o routing
```

> **Impatto:** Nella Fase 1 (MILOS), il mapping era diretto: risposta HTTP = esito finale. In Fase 2, lo stato `ACKNOWLEDGED` corrisponde alla ricezione AS4, non alla risposta REST. Serve un **endpoint inbound** nel nostro Connector per ricevere le notifiche callback dal Gate.

---

## 7. Analisi dello Stato Attuale — EftiNativeGateway

### 7.1 Cosa è già implementato ✅

| Componente | File | Stato | Note |
|---|---|---|---|
| Interfaccia gateway | `EftiNativeGateway.cs` | ✅ Completo | Implementa `IEftiGateway` |
| Client Refit | `IEftiGateClient.cs` | ✅ Struttura base | Endpoints CRUD presenti |
| OAuth2 handler | `EftiOAuth2Handler.cs` | ⚠️ Parziale | Manca `client_assertion` JWT |
| Token cache Redis | `EftiTokenCache.cs` | ✅ Completo | TTL automatico |
| Caricamento X.509 | `X509CertificateLoader.cs` | ✅ Completo | Supporta `.pfx`/`.p12` |
| Mapper EN 17532 | `EcmrPayloadToEftiMapper.cs` | ⚠️ Parziale | Manca UID format, `EftiGoods` |
| Modello dataset | `EftiEcmrDataset.cs` | ⚠️ Parziale | Manca `status`, `registryId` |
| Modelli consignor/ee | `EftiConsignor.cs`, `EftiConsignee.cs` | ✅ | Da verificare campi |
| Configurazione | `EftiNativeOptions.cs` | ⚠️ Parziale | Manca `ClientSecret`, `WebhookCallbackUrl` |
| DI Extensions | `EftiNativeGatewayExtensions.cs` | ⚠️ Parziale | Manca configurazione mTLS sull'HttpClientHandler |

### 7.2 File Presenti ma Non Analizzati

```
Models\EftiEcmrDataset.cs         ← (diverso da Models\EN17532\EftiEcmrDataset.cs — potenziale duplicato)
Auth\EftiTokenResponse.cs         ← modello risposta token
```

> ⚠️ **Da verificare:** Esistono due `EftiEcmrDataset.cs` in percorsi diversi (`Models\` e `Models\EN17532\`). Verificare se uno è obsoleto e rimuoverlo per evitare ambiguità.

---

## 8. Gap Analysis — Cosa Manca

### Priorità Alta 🔴

#### 8.1 Endpoint Inbound per Notifiche Callback del Gate
**Problema:** Quando il Gate porta il dataset a stato `ACKNOWLEDGED`, invia una `POST` HTTP al nostro Connector (webhook). Attualmente non esiste nessun endpoint che riceva questa notifica.

**Impatto:**
- Nuovo controller/endpoint in `FormInputService` o in un nuovo `EftiCallbackService`
- Nuovo consumer MassTransit / nuovo evento `EftiAcknowledgeReceivedEvent`
- Aggiornamento dello stato `TransportOperation` a `ACKNOWLEDGED`

**File da creare:**
```
src/Services/ilp_efti_connector.EftiGatewayService/Controllers/EftiCallbackController.cs
src/Shared/ilp_efti_connector.Shared.Contracts/Events/EftiAcknowledgeReceivedEvent.cs
```

#### 8.2 Generatore di UID Conforme al Formato eFTI
**Problema:** Il campo `Id` del dataset viene popolato con `OperationCode` grezzo. Il Gate si aspetta il formato `{CC}.{PlatformId}.{DatasetType}.{UniqueId}`.

**Impatto:**
- Nuova classe `EftiUidGenerator` in `Gateway.EftiNative`
- Modifica a `EcmrPayloadToEftiMapper.Map()` per usare il generatore
- Nuovo campo di configurazione `PlatformId` in `EftiNativeOptions`
- Aggiornamento colonna `external_id` in `efti_messages` per memorizzare il formato UID

**File da creare/modificare:**
```
src/Gateway/ilp_efti_connector.Gateway.EftiNative/EftiUidGenerator.cs           ← NUOVO
src/Gateway/ilp_efti_connector.Gateway.EftiNative/Mapping/EcmrPayloadToEftiMapper.cs ← MODIFICA
src/Gateway/ilp_efti_connector.Gateway.EftiNative/EftiNativeOptions.cs          ← MODIFICA
```

#### 8.3 mTLS sull'HttpClient verso il Gate
**Problema:** `EftiNativeGatewayExtensions` configura l'`HttpClient` Refit ma **non aggiunge il certificato X.509 client** per mutual TLS. `X509CertificateLoader` esiste ma non è integrato nel pipeline HTTP.

**Impatto:** Modifica a `EftiNativeGatewayExtensions.cs`.

```csharp
// Aggiungere in ConfigureHttpClient:
var handler = new HttpClientHandler();
if (!string.IsNullOrEmpty(opts.CertificatePath))
{
    var cert = X509CertificateLoader.Load(opts.CertificatePath, opts.CertificatePassword);
    handler.ClientCertificates.Add(cert);
}
```

---

### Priorità Media 🟡

#### 8.4 client_assertion JWT per OAuth2 in Produzione
**Problema:** `EftiOAuth2Handler` usa solo `client_secret`. In produzione eIDAS, l'autenticazione richiede un JWT firmato con la chiave privata del certificato X.509 (`client_assertion`).

**Standard:** RFC 7523 — JSON Web Token (JWT) Profile for OAuth 2.0 Client Authentication.

```
grant_type=client_credentials
&client_id=<clientId>
&client_assertion_type=urn:ietf:params:oauth:client-assertion-type:jwt-bearer
&client_assertion=<JWT firmato con X.509>
&scope=efti
```

**Impatto:** Modifica a `EftiOAuth2Handler.FetchTokenAsync()` — aggiungere la costruzione del JWT usando `System.IdentityModel.Tokens.Jwt` + il certificato X.509 caricato da `X509CertificateLoader`.

**Dipendenze NuGet da aggiungere:**
```
Microsoft.IdentityModel.Tokens
System.IdentityModel.Tokens.Jwt
```

#### 8.5 Polling di Stato come Fallback al Callback
**Problema:** Se il callback del Gate non arriva (connettività, timeout), il sistema rimane in stato `SENDING` indefinitamente. Serve un meccanismo di polling periodico su `GET /datasets/{id}/status`.

**Impatto:**
- Nuovo metodo `GetDatasetStatusAsync` in `IEftiGateClient`
- Modifica al `RetryService`: oltre ai retry di invio, aggiungere un poll di stato per messaggi in `SENDING` da più di N minuti

#### 8.6 Supporto Callback URL Registrazione sul Gate
**Problema:** Il Gate deve sapere dove inviare le notifiche di stato. L'endpoint `POST /notifications/register` del Gate deve essere chiamato al boot con il nostro callback URL.

**Impatto:**
- Nuovo metodo `RegisterCallbackAsync` in `IEftiGateClient`
- Chiamata a `RegisterCallbackAsync` nel `GatewayHealthMonitor` o all'avvio di `EftiGatewayService`
- Nuovo campo `WebhookCallbackUrl` in `EftiNativeOptions`

#### 8.7 Modello EftiGoods / EftiPackage Incompleto
**Problema:** Dall'analisi di `EftiEcmrDataset.cs`, la classe `EftiGoods` è referenziata ma il suo file non è visibile tra i file del progetto — potrebbe mancare o avere campi incompleti per la struttura packages[].

**Verifica richiesta:** Controllare che `EftiGoods` includa la lista `Packages` con i campi `typeCode`, `quantity`, `grossWeight`, `grossVolume`, `shippingMarks`.

#### 8.8 Duplicato EftiEcmrDataset
**Problema:** Esistono due file con lo stesso nome:
- `Models/EftiEcmrDataset.cs`
- `Models/EN17532/EftiEcmrDataset.cs`

**Azione:** Verificare quale è quello attivo, rimuovere l'obsoleto, aggiornare le `using` di conseguenza.

---

### Priorità Bassa 🟢

#### 8.9 Supporto XML (XSD) oltre a JSON
**Problema:** Il reference implementation usa schemi XSD. Alcuni gate nazionali potrebbero richiedere XML invece di JSON.

**Impatto:** Eventuale aggiunta di un serializer XML su `EftiNativeGatewayExtensions` — configurabile tramite `EftiNativeOptions.UseXml`.

#### 8.10 Registry of Identifiers Lookup
**Problema:** Il reference implementation prevede un `Registry of Identifiers` condiviso tra i gate. Prima di inviare un dataset, è necessario verificare che l'UID non sia già registrato (idempotenza).

**Impatto:** Eventuale nuovo client `IEftiRegistryClient` con `GET /identifiers/{uid}`.

#### 8.11 Metriche Specifiche Fase 2
**Problema:** `IlpEftiMetrics` non include contatori specifici per EFTI_NATIVE (ACK ricevuti, callback falliti, polling effettuati).

---

## 9. Piano di Implementazione

### Sprint 1 — Fondamenta (1-2 settimane)
**Obiettivo:** Gateway funzionante in un ambiente sandbox con Gate reale (o simulatore).

| Task | File/Componente | Priorità |
|---|---|---|
| Correggere formato UID | `EftiUidGenerator.cs` + mapper | 🔴 |
| Aggiungere mTLS all'HttpClient | `EftiNativeGatewayExtensions.cs` | 🔴 |
| Rimuovere duplicato `EftiEcmrDataset` | file da eliminare | 🟡 |
| Completare `EftiGoods` + `EftiPackage` | `Models/EN17532/` | 🟡 |
| Aggiungere `ClientSecret` + `WebhookCallbackUrl` a `EftiNativeOptions` | config | 🟡 |

### Sprint 2 — Callback e Asincronia (2-3 settimane)
**Obiettivo:** Ciclo di vita asincrono completo con ACKNOWLEDGED.

| Task | File/Componente | Priorità |
|---|---|---|
| Controller inbound callback | `EftiCallbackController.cs` | 🔴 |
| Evento `EftiAcknowledgeReceivedEvent` | `Shared.Contracts` | 🔴 |
| Consumer `EftiAcknowledgeReceivedConsumer` in `ResponseHandlerService` | nuovo consumer | 🔴 |
| Transizione stato `ACKNOWLEDGED` in `TransportOperation` | `ResponseHandlerService` | 🔴 |
| Registrazione callback URL al boot | `GatewayHealthMonitor` o startup | 🟡 |

### Sprint 3 — Produzione (2-3 settimane)
**Obiettivo:** Autenticazione production-grade, polling di fallback.

| Task | File/Componente | Priorità |
|---|---|---|
| `client_assertion` JWT OAuth2 | `EftiOAuth2Handler.cs` | 🟡 |
| Polling stato fallback in `RetryService` | modifica `RetryService` | 🟡 |
| Supporto XML (opzionale) | `EftiNativeGatewayExtensions.cs` | 🟢 |
| Registry lookup | `IEftiRegistryClient.cs` | 🟢 |

---

## 10. Impatti su Altri Layer del Sistema

### 10.1 `Shared.Contracts` — Nuovi Eventi

```csharp
// NUOVO: evento di ACK dal Gate EFTI nativo
public record EftiAcknowledgeReceivedEvent(
    Guid   EftiMessageId,
    Guid   TransportOperationId,
    string CorrelationId,
    string GatewayProvider,
    string EftiUid,          // UID formato: IT.EFTI_CONNECTOR.ECMR.xxx
    string GateStatus,       // ACKNOWLEDGED | ERROR
    string? ErrorMessage,
    DateTime ReceivedAt
);
```

### 10.2 `ResponseHandlerService` — Nuova Transizione di Stato

Aggiungere un nuovo consumer `EftiAcknowledgeReceivedConsumer` che:
1. Carica l'`EftiMessage` per `EftiMessageId`.
2. Imposta `Status = ACKNOWLEDGED`, `AcknowledgedAt = ReceivedAt`.
3. Imposta `TransportOperation.Status = ACKNOWLEDGED`.
4. Pubblica `SourceNotificationRequiredEvent` con stato `ACKNOWLEDGED`.

### 10.3 `NormalizationService` — Generazione UID

Il `NormalizationService` (o il mapper) deve generare il corretto formato UID. La logica di generazione UID deve essere disponibile prima della pubblicazione di `EftiSendRequestedEvent`, in modo che l'UID sia determinisico e idempotente (non generato ad ogni retry).

**Proposta:** Generare l'UID in `SubmitTransportOperationCommandHandler` e memorizzarlo in `EftiMessage.ExternalId` già al momento della creazione (non più al momento dell'invio).

### 10.4 `EftiGatewayService` — Nuovo Endpoint HTTP

`EftiGatewayService` è attualmente un puro Worker Service (senza endpoint HTTP). Per ricevere i callback del Gate serve:
- **Opzione A:** Aggiungere un controller HTTP all'`EftiGatewayService` (trasformarlo in Web API + Worker)
- **Opzione B:** Creare un nuovo microservizio `EftiCallbackService` dedicato alla ricezione dei callback
- **Opzione C:** Usare `FormInputService` o `QueryProxyService` come proxy per i callback

**Raccomandazione:** Opzione A — aggiungere Kestrel all'`EftiGatewayService` con un controller leggero. Questo mantiene la coerenza: tutto ciò che riguarda la comunicazione con il Gate è nello stesso servizio.

### 10.5 Schema DB — Nessuna Migration Necessaria

La colonna `efti_messages.external_id` (`VARCHAR(100)`) è già sufficiente per memorizzare il formato UID (`IT.EFTI_CONNECTOR.ECMR.CMR-2026-00123` = max ~60 caratteri). Nessuna migration richiesta.

---

## 11. Configurazione e Certificati

### 11.1 Nuova Sezione `appsettings.json` per Fase 2

```json
"EftiGateway": {
  "Provider": "EftiNative",
  "EftiNative": {
    "BaseUrl":            "https://efti-gate.gov.it/api/",
    "TokenEndpoint":      "https://auth.efti.gov.it/oauth2/token",
    "ClientId":           "<registrato presso il gate nazionale>",
    "ClientSecret":       null,
    "CertificatePath":    "certs/efti-client-prod.pfx",
    "CertificatePassword":"<da vault>",
    "Scope":              "efti",
    "PlatformId":         "EFTI_CONNECTOR",
    "CountryCode":        "IT",
    "WebhookCallbackUrl": "https://connector.azienda.it/api/efti/callback",
    "UseClientAssertion": true,
    "TimeoutSeconds":     30
  }
}
```

### 11.2 Nuovi Campi da Aggiungere a `EftiNativeOptions`

```csharp
/// <summary>ID della piattaforma per la generazione dell'UID EFTI (es. EFTI_CONNECTOR).</summary>
public string PlatformId { get; set; } = "EFTI_CONNECTOR";

/// <summary>Codice paese per la generazione dell'UID EFTI (es. IT).</summary>
public string CountryCode { get; set; } = "IT";

/// <summary>URL pubblico del nostro Connector per ricevere i callback dal Gate.</summary>
public string? WebhookCallbackUrl { get; set; }

/// <summary>Se true, usa client_assertion JWT (X.509) invece di client_secret.</summary>
public bool UseClientAssertion { get; set; } = false;
```

### 11.3 Gestione Certificati in Produzione

```
Sviluppo locale:    certs/dev-efti.pfx  (auto-signed, solo per test)
Staging:            Kubernetes Secret   (creato da Vault Agent)
Produzione:         X.509 qualificato EU, ottenuto da CA accreditata eIDAS
                    Scadenza: monitorata da Grafana alert (sezione 13.4 architettura)
```

---

## 12. Test e Validazione

### 12.1 Simulator del Reference Implementation

Il reference implementation include un **Platform and Gate Simulator** (`implementation/platform-gate-simulator/`). Questo simulatore può essere usato per:

1. **Sviluppo offline**: Testare il nostro `EftiNativeGateway` senza accesso al Gate reale.
2. **Test automatici**: Integrare il simulatore nei `IntegrationTests` via Testcontainers (immagine Docker disponibile nel `deploy/` folder).

**Configurazione locale con Docker:**

```bash
# Avvio del simulatore dal reference implementation
cd reference-implementation/deploy/local
docker compose up -d

# Il simulatore espone:
# Gate API:  http://localhost:8080
# Auth:      http://localhost:8090
```

### 12.2 Test da Implementare nel Progetto

| Progetto | Suite | Cosa testare |
|---|---|---|
| `Gateway.EftiNative.Tests` | Unit | `EcmrPayloadToEftiMapper`, `EftiUidGenerator`, `EftiOAuth2Handler` (mock) |
| `IntegrationTests` | Integration | Flusso completo con Gate simulator via Testcontainers |
| `IntegrationTests` | Integration | Ricezione callback → transizione ACKNOWLEDGED |

### 12.3 Collection Postman del Reference Implementation

Il folder `utils/postman/` del reference implementation contiene collection Postman per testare manualmente tutte le API del Gate. Importare queste collection in Postman per:

- Validare il formato del payload inviato
- Verificare i token OAuth2
- Testare il callback manuale

### 12.4 Checklist Pre-Go-Live Fase 2 (aggiornamento alla Sezione 13.5 dell'architettura)

```
[ ] Certificato X.509 EU qualificato ottenuto e caricato in Vault
[ ] PlatformId registrato presso il Gate nazionale italiano
[ ] ClientId OAuth2 registrato presso l'Identity Provider del Gate
[ ] Formato UID verificato: IT.<PlatformId>.ECMR.<OperationCode>
[ ] mTLS handshake verificato: GET /health risponde 200 con certificato client
[ ] client_assertion JWT testato: token ottenuto con X.509 signing
[ ] WebhookCallbackUrl raggiungibile dal Gate (firewall aperto in ingresso)
[ ] Callback registration verificata: POST /notifications/register → 200 OK
[ ] Test end-to-end: POST /datasets → 202 → callback ACKNOWLEDGED ricevuto
[ ] Polling fallback testato: GET /datasets/{id}/status → ACKNOWLEDGED
[ ] Stato ACKNOWLEDGED propagato in DB: transport_operations, efti_messages
[ ] Notifica webhook al TMS sorgente con status=ACKNOWLEDGED
[ ] GatewayHealthMonitor: log HEALTHY per EFTI_NATIVE in Seq
[ ] Metriche efti_messages_acknowledged_total > 0 su Grafana
[ ] Regression test MILOS: path Fase 1 ancora funzionante come fallback
```

---

## Appendice — Differenze Chiave tra Fase 1 (MILOS) e Fase 2 (EFTI Gate)

| Aspetto | Fase 1 — MILOS | Fase 2 — EFTI Gate Nativo |
|---|---|---|
| **Autenticazione** | API Key (`X-API-Key` header) | OAuth2 client_credentials + X.509 mTLS |
| **Payload** | `ECMRRequest` (JSON custom MILOS) | `EftiEcmrDataset` (EN 17532 JSON/XML) |
| **Identificatore** | `eCMRID` generato da MILOS | UID formato `CC.PlatformId.Type.UniqueId` |
| **Risposta invio** | Sincrona: `200 OK` + `eCMRID` | Asincrona: `202 Accepted` + callback |
| **QR Code** | Generato da MILOS | Generato dal Gate nazionale |
| **Hash integrità** | SHA-256 in `hashcodeDetails.json` | SHA-256 in `hashcode.value` |
| **Interoperabilità** | Limitata (solo paesi MILOS) | Multi-stato EU (via AS4 inter-gate) |
| **SLA / Uptime** | Dipende da Circle SpA | Dipende dallo Stato italiano |
| **Costi** | Licenza commerciale MILOS | Zero (gate pubblico) |
| **Autorità stradali** | Interrogano MILOS | Interrogano il Gate IT direttamente |

---

*Documento creato: Febbraio 2026*  
*Fonte: [EFTI4EU/reference-implementation](https://github.com/EFTI4EU/reference-implementation) — Release v0.5.0*  
*Aggiornare questo documento quando il gate nazionale italiano pubblicherà le specifiche ufficiali API.*
