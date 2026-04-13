# AI Forged Tour Ops – MVP 1

## Overview
This is a lightweight AI-native tour operator platform.

AI Forged handles ingestion.
This system handles:
- Products
- Rates
- Itineraries
- Pricing
- Quotes

## Architecture
- Backend: .NET 8 Web API
- DB: PostgreSQL (EF Core)
- Frontend: React
- Auth: Keycloak

## Layering (STRICT)
Controller → Service → Repository → DbContext

Rules:
- Controllers use DTOs only
- No entity exposure outside service/repo
- No business logic in controllers
- No direct DB access outside repositories

## MVP Scope
- Supplier ingestion
- Product + rate management
- Itinerary builder
- Pricing engine
- Quote generation

## Not Included
- Bookings
- Payments
- CRM

## Goal
Convert supplier data → itinerary → priced quote in minutes.

## Docker Quick Start
Run the full platform (frontend + api + postgres + keycloak):

```bash
docker-compose up --build
```

### Service Endpoints
- Frontend: http://localhost:3000
- API: http://localhost:5000
- Keycloak: http://localhost:8080
- PostgreSQL: localhost:5432

### Default Credentials
- Keycloak admin console: `admin / admin`
- Default realm user: `admin / admin`

### Notes
- Keycloak realm auto-import file: `keycloak/realm-tourops.json`
- API uses `ConnectionStrings__DefaultConnection` and points to `postgres` service inside Docker network.
