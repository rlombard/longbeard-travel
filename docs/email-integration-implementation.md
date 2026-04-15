# Client Email Integration Implementation

## What Added

* client mailbox/provider integration layer in API
* user-owned `EmailProviderConnection` records
* synced-message dedupe via `EmailProviderMessageLink`
* secure secret/token protection before persistence
* provider adapter abstraction for OAuth, send, sync, and webhook capability
* background sync worker for due mailbox polling

## Endpoints

* `GET /api/email-integrations/connections`
* `GET /api/email-integrations/connections/{connectionId}`
* `POST /api/email-integrations/connections`
* `POST /api/email-integrations/oauth/start`
* `GET /api/email-integrations/oauth/callback/{providerType}`
* `POST /api/email-integrations/connections/{connectionId}/test`
* `POST /api/email-integrations/connections/{connectionId}/sync`
* `POST /api/email-integrations/connections/{connectionId}/send`
* `DELETE /api/email-integrations/connections/{connectionId}`
* `GET /api/email-integrations/webhooks/microsoft-graph`
* `POST /api/email-integrations/webhooks/microsoft-graph`

## Data Model

* `EmailProviderConnection`
  * provider type
  * auth method
  * owner user id
  * mailbox identity
  * send/sync flags
  * encrypted secrets
  * OAuth state
  * sync cursor
  * webhook subscription state
  * operational status and timestamps
* `EmailProviderMessageLink`
  * provider message id
  * provider thread id
  * linked `EmailThread`
  * linked `EmailMessage`
  * folder + received time

## Provider Support

* `Microsoft365`
  * OAuth2
  * Graph send
  * Graph sync
  * webhook endpoint scaffolded, polling remains primary path now
* `Gmail`
  * OAuth2
  * Gmail API send
  * Gmail API sync
* `SendGrid`
  * outbound send only
* `Mailcow`
  * IMAP sync
  * SMTP send
* `GenericImapSmtp`
  * IMAP sync
  * SMTP send
* `SmtpDirect`
  * SMTP send only

## Design Notes

* this is integration support, not mailbox hosting
* Keycloak stays auth boundary for TourOps users
* mailbox connections bind to current authenticated TourOps user because repo has no tenant/org model yet
* browser never gets provider secrets
* connected-provider send path now backs existing draft sending when a default active connection exists
* polling uses sync cursor + provider ids to avoid blind full re-fetch

## Config

* `EmailIntegration__Enabled`
* `EmailIntegration__EncryptionKey`
* `EmailIntegration__SyncWorkerIntervalSeconds`
* `EmailProviders__Microsoft365__*`
* `EmailProviders__Gmail__*`
* `EmailProviders__SendGrid__*`

## Follow-Up

* add tenant/org binding when repo gets tenant model
* harden Microsoft webhook subscription lifecycle if webhook mode becomes required
* add inbound webhook validation/signing for providers that need it
* move encryption key to managed secret store outside local/dev config
