# Customer And KYC Implementation

## Customer Model

* `Customer`
  * core identity and contact fields
  * nationality
  * country of residence
  * date of birth
  * preferred contact method
  * operator notes
* lead customer linkage added to:
  * `Itinerary`
  * `Quote`
  * `Booking`
* `BookingTraveller`
  * supports additional travellers / party members per booking
  * keeps relationship-to-lead and notes

## KYC Model

* `CustomerKycProfile`
  * separate table from core customer
  * passport number
  * document reference
  * passport expiry
  * issuing country
  * visa notes
  * emergency contact
  * verification status
  * verification notes
  * profile-data and KYC-data consent flags

## Preference Model

* `CustomerPreferenceProfile`
  * separate structured table
  * budget band
  * accommodation preference
  * room preference
  * pace of travel
  * luxury vs value leaning
  * operator notes
  * structured JSON arrays for:
    * dietary requirements
    * activity preferences
    * accessibility requirements
    * transport preferences
    * special occasions
    * disliked experiences
    * preferred destinations
    * avoided destinations

## Linkages

* one lead customer on itinerary
* one lead customer on quote
* one lead customer on booking
* many additional booking travellers via `BookingTraveller`
* quote generation now carries lead customer from itinerary
* booking creation now carries lead customer from quote

## Privacy

* KYC data separated from lightweight customer list data
* list endpoint excludes sensitive KYC fields
* KYC and preference updates write `CustomerAuditLog`
* audit payloads store changed-field summaries, not raw full snapshots by default
* design leaves room for later masking/redaction on detail endpoints

## AI Readiness

* preference profile is structured, not only notes
* customer history can be read across itinerary / quote / booking links
* destination and experience likes/dislikes are machine-readable
* future AI can safely propose preference summaries and trip recommendations from stored structure plus operator notes
