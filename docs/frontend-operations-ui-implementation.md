# Frontend Operations UI Implementation

## Routes And Pages

* `/itineraries`
  * manual itinerary create
  * persisted itinerary lookup
* `/itineraries/ai`
  * AI product assist workspace
  * AI draft itinerary review and save flow
* `/invoices`
  * invoice list
  * invoice intake / test ingest form
* `/invoices/:invoiceId`
  * invoice detail
  * status, relink, payment-prep workflow
* `/customers`
  * customer list
  * create customer workflow
* `/customers/:customerId`
  * profile
  * preferences
  * KYC
  * relationship management

## Major Components

* `Badge`
  * shared status / AI / warning chip
* `MultiValueEditor`
  * structured list editing for interests, issues, preferences
* invoice status presentation
* customer verification presentation

## API Integrations Used

* itinerary
  * `POST /itineraries`
  * `GET /itineraries/{id}`
  * `POST /itineraries/ai/product-assist`
  * `POST /itineraries/ai/draft`
  * `POST /itineraries/ai/drafts/{draftId}/approve`
* invoices
  * `POST /invoices/ingest`
  * `GET /invoices`
  * `GET /invoices/{id}`
  * `PATCH /invoices/{id}/status`
  * `PATCH /invoices/{id}/links`
  * `POST /invoices/{id}/payments`
  * `POST /invoices/{id}/rebate/apply`
* customers
  * `POST /customers`
  * `GET /customers`
  * `GET /customers/{id}`
  * `PUT /customers/{id}`
  * `PUT /customers/{id}/kyc`
  * `PUT /customers/{id}/preferences`
  * `POST /customers/{id}/quotes/{quoteId}`
  * `POST /customers/{id}/itineraries/{itineraryId}`
  * `POST /customers/{id}/bookings/{bookingId}`
  * `PUT /customers/{id}/bookings/{bookingId}/traveller`
  * `DELETE /customers/{id}/bookings/{bookingId}/traveller`

## UI Workflow Decisions

* AI outputs always marked with AI badges
* AI draft kept separate from saved itinerary
* unresolved itinerary items block save until mapped or removed
* invoice list favors triage first
* invoice detail groups review, linking, and payment prep
* customer list hides KYC-sensitive values
* customer detail splits profile, preferences, KYC, relationships

## Follow-Up Items

* add backend list/search endpoints for quotes and itineraries to improve link pickers
* expose invoice raw extraction snapshot and audit history for richer review screen
* add server-side invoice search, supplier-name filter, and currency filter for scale
* add frontend field-level permissions for sensitive customer/KYC fields
* split routes with lazy loading if bundle size becomes a concern
