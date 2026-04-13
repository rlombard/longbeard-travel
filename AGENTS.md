# AGENTS.md

## Purpose
Defines how AI agents (Codex / automation) must interact with this codebase.

---

## Core Architecture (MANDATORY)

All implementations MUST follow:

Controller → Service → Repository → DbContext

### Rules

- Controllers:
  - Use Request/Response DTOs ONLY
  - No business logic
  - No EF usage

- Services:
  - Contain ALL business logic
  - Coordinate repositories
  - Handle validation

- Repositories:
  - Data access only
  - No business logic
  - Return domain entities internally only

- Entities:
  - NEVER exposed via API
  - Only used inside service/repository layer

Violation of this structure is not allowed.

---

## Backend Stack

- .NET 8 Web API
- Entity Framework Core
- PostgreSQL

---

## Frontend Stack

- React
- Keycloak authentication (OIDC)

---

## Domain Scope (MVP 1)

Entities:
- Supplier
- Product
- Rate
- Itinerary
- ItineraryItem
- Quote
- QuoteLineItem

---

## Pricing Engine Rules

- Deterministic only (NO AI decision making)
- Supported models:
  - PerPerson
  - PerGroup
  - PerUnit
- Must:
  - Fail if no rate found
  - Fail if currency missing
  - Apply markup AFTER cost

---

## AI Forged Integration

- AI Forged sends structured JSON
- This system:
  - Validates
  - Maps
  - Persists

AI NEVER writes directly to DB.

---

## Coding Standards

- Use async/await
- Use dependency injection
- No static state
- All services must be testable

---

## Folder Structure (Required)
/src
    /Api
    /Controllers
    /Models (DTOs)
    /Application
    /Services
    /Interfaces
    /Domain
    /Entities
    /Enums
    /Infrastructure
    /Repositories
    /Data (DbContext)


---

## What Agents SHOULD Do

- Scaffold controllers/services/repositories correctly
- Generate DTOs separate from entities
- Implement pricing engine in service layer
- Respect boundaries

---

## What Agents MUST NOT Do

- Use DbContext in controllers
- Return EF entities in API responses
- Put logic in repositories
- Introduce AI logic into pricing

---

## MVP Goal

Enable:
- ingestion → itinerary → quote

NOT:
- full booking system
- enterprise features

---

## Future Expansion

System must remain extensible for:
- bookings
- CRM
- distribution APIs