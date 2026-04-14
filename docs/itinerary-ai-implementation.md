# Itinerary AI Implementation

## Added

* AI-assisted product recommendation flow for itinerary planning
* AI-assisted draft itinerary generation flow
* Explicit draft approval flow before itinerary persistence
* Persisted itinerary draft tables for auditability and operator review
* Unit tests for main itinerary AI service paths

## Key Decisions

* Deterministic catalog filtering runs first
* AI only ranks shortlisted products and drafts against real product ids
* Invalid AI product ids are ignored
* Unmapped draft steps become unresolved placeholders
* Draft generation does not persist a live itinerary
* Approval is explicit and creates human-approval audit records before save
* LLM audit logging uses the existing generic LLM service path
* Draft artifacts are stored because review + approve is a multi-step workflow

## Endpoints

* `POST /api/itineraries/ai/product-assist`
* `POST /api/itineraries/ai/draft`
* `POST /api/itineraries/ai/drafts/{draftId}/approve`
* Existing `POST /api/itineraries` now uses application models instead of domain entities in the controller

## Data Model Changes

* Added `ItineraryDraftStatus`
* Added `ItineraryDraft`
* Added `ItineraryDraftItem`
* Added EF migration `20260414130041_AddItineraryAiDrafts`

## Flow Summary

### Product Assist

* accept structured trip criteria + optional free-text brief
* filter catalog deterministically with product, supplier, rate, and metadata signals
* score candidates
* ask LLM to rank/explain shortlisted candidates only
* return structured matches, warnings, assumptions, and missing data

### Draft Generation

* accept trip brief + trip parameters
* shortlist real catalog products
* ask LLM for structured draft items only against shortlist ids
* convert unknown ids to unresolved placeholders
* persist draft artifact for review
* return assumptions, caveats, data gaps, and item-level confidence

### Draft Approval

* operator can approve stored resolved items
* operator can override items before save
* unresolved items block approval unless operator supplies real product overrides
* approval creates a human-approval record
* approved draft becomes a normal persisted itinerary

## Follow-Up For UI

* add itinerary AI builder screen for brief capture
* show recommendation reasons, warnings, and missing data inline
* show unresolved draft items with required operator action
* allow operator edits before approve
* surface draft status and linked persisted itinerary id after approval
