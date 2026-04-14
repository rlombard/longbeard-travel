# Invoice Operations Implementation

## Data Model

* `Invoice`
  * source system + external source ref
  * invoice number
  * supplier / booking / booking item / quote / email thread links
  * invoice date / due date
  * subtotal / tax / total / rebate
  * extraction confidence
  * extraction issues
  * unresolved fields
  * raw payload + normalized snapshot
  * review task link
  * status + timestamps
* `InvoiceLineItem`
  * description
  * optional booking item link
  * qty / unit price / tax / total
* `InvoiceAttachment`
  * external file ref
  * file name / content type / source url
* `PaymentRecord`
  * amount / currency
  * paid at
  * external payment ref
  * method / notes / metadata

## Endpoints

* `POST /api/invoices/ingest`
* `GET /api/invoices/{id}`
* `GET /api/invoices`
* `PATCH /api/invoices/{id}/status`
* `PATCH /api/invoices/{id}/links`
* `POST /api/invoices/{id}/payments`
* `POST /api/invoices/{id}/rebate/apply`

## Statuses

* `Draft`
* `Received`
* `Matched`
* `Unmatched`
* `PendingReview`
* `Approved`
* `Rejected`
* `Unpaid`
* `PartiallyPaid`
* `Paid`
* `Overdue`
* `RebatePending`
* `RebateApplied`
* `Cancelled`

## Workflow

* AI Forged pushes structured invoice payload
* TourOps validates and stores payload
* deterministic matching runs first
* invoice lands as `Matched`, `Received`, `PendingReview`, or `Unmatched`
* review-required invoices can create operational tasks when booking context exists
* operator can relink
* operator can approve / reject
* operator can record payments
* operator can apply rebates

## Matching Rules

* supplier by `SupplierId`
* supplier by parseable supplier ref guid
* supplier by exact normalized supplier name
* booking by `BookingId` or parseable booking ref guid
* booking item by `BookingItemId` or parseable booking item ref guid
* quote by `QuoteId` or parseable quote ref guid
* email thread by `EmailThreadId`
* booking inferred from booking item
* booking inferred from quote when booking exists for quote
* supplier inferred from booking item when needed
* conflicts become unresolved fields + warnings

## Future Extensions

* scheduled overdue transitions
* partner payment orchestration
* outbound payment connectors
* payment approval rules
* richer invoice-to-email lineage and attachment retrieval
