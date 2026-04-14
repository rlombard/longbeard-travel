# AGENTS.md

## Purpose

Defines how AI agents (Codex / automation) must interact with this codebase.

---

## Token Efficiency Mode (MANDATORY)

Agents MUST:

* Use **caveman style language**
* Short sentences
* No filler
* No repetition
* Prefer lists over paragraphs

### Output Rules

* Say less, mean same
* No long explanations
* No storytelling
* No redundant phrasing

### Example

GOOD:

* create service
* validate input
* return dto

BAD:

* long explanations
* unnecessary commentary

---

## Core Architecture (MANDATORY)

All implementations MUST follow:

Controller → Service → Repository → DbContext

### Rules

* Controllers:

  * DTOs ONLY
  * No business logic
  * No EF usage

* Services:

  * ALL business logic
  * Coordinate repositories
  * Validation here

* Repositories:

  * Data access only
  * No business logic

* Entities:

  * NEVER exposed via API
  * Internal use only

Violation NOT allowed.

---

## Backend Stack

* .NET 8 Web API
* Entity Framework Core
* PostgreSQL

---

## Frontend Stack

* React
* Keycloak (OIDC)

---

## Domain Scope

### Core

* Supplier
* Product
* Rate
* Itinerary
* ItineraryItem
* Quote
* QuoteLineItem

### Phase 2

* Booking
* BookingItem

### Phase 3

* Task (Operational Task system)

### Phase 4 (AI + Email)

* EmailThread
* EmailMessage
* EmailDraft
* SuggestedTask (AI output)

---

## Domain Rules (CRITICAL)

### Product

Product = reusable supplier offering

NOT:

* booking
* quote line
* itinerary instance

---

### Rate

* belongs to Product
* defines pricing rules

---

### ItineraryItem

* references Product
* stores usage (day, qty, notes)

---

### QuoteLineItem

* generated output only
* not source of truth

---

### Booking

* execution layer
* derived from Quote

---

### Task

* operational action
* linked to Booking or BookingItem

---

## Pricing Engine Rules

* Deterministic ONLY
* NO AI logic

Supported:

* PerPerson
* PerGroup
* PerUnit

Must:

* fail if no rate
* fail if currency missing
* apply markup AFTER cost

---

## AI Rules (CRITICAL)

AI is ASSIST ONLY.

Allowed:

* suggest tasks
* summarize emails
* classify emails
* draft replies

Forbidden:

* auto confirm bookings
* auto send emails
* auto change pricing
* auto make decisions

Human ALWAYS decides.

---

## AI Forged Integration

Primary AI path:

Flow:
Email → PDF → AI Forged → JSON → System

System:

* generate PDF
* send to AI Forged
* parse JSON
* map to SuggestedTasks / insights

AI NEVER writes DB directly.

---

## LLM Architecture

Must exist:

* Generic LLM interface
* Provider-specific implementations

Providers:

* OpenAI
* Azure OpenAI
* Anthropic

Rules:

* No provider logic in services
* Use DI
* Config via appsettings/env

---

## Email System

Built-in lightweight client.

Entities:

* EmailThread
* EmailMessage
* EmailDraft

Rules:

* no auto-send
* drafts require approval
* AI can suggest only

---

## Task System

Operational Tasks:

Statuses:

* ToDo
* Waiting
* FollowUp
* Blocked
* Done

Features:

* assign user
* reassign
* kanban + list UI

---

## Coding Standards

* async/await everywhere
* dependency injection
* no static state
* services testable
* no shortcuts

---

## Folder Structure (Required)

/src
/Api
/Controllers
/Models
/Application
/Services
/Interfaces
/Domain
/Entities
/Enums
/Infrastructure
/Repositories
/Data

---

## What Agents SHOULD Do

* follow architecture strictly
* generate DTOs separate
* keep services clean
* enforce domain rules
* integrate AI via abstraction only

---

## What Agents MUST NOT Do

* use DbContext in controllers
* return entities in API
* put logic in repositories
* mix AI into pricing
* auto-execute AI decisions
* hardcode providers
* break layering
1§  qwe[]
---

## MVP Goal

ingestion → itinerary → quote → booking → tasks → email assist

---

## Keycloak User Admin Rules

- Keycloak is identity backend
- Frontend never calls Keycloak admin API directly
- Flow is: UI -> API -> Keycloak Admin API
- Only `admin` role can create/view/manage users
- UI may:
  - create users
  - assign/de-assign existing roles
  - assign/de-assign existing groups
  - enable/disable users
  - reset temp passwords
- UI may NOT:
  - create roles
  - create groups
  - create permissions
- New user must get temporary password
- New user must be forced to change password on first login
- Keycloak remains source of truth

---

## Future Direction

System must support:

* AI-assisted operations
* time-zone gap reduction
* email understanding
* human-in-the-loop decisions
* agentic assist (later phase)

---

## Final Rule

Keep system:

* clean
* modular
* extensible
* human-controlled

AI assists. Human decides.
