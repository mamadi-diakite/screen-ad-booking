# Roadmap

This is the working plan for screen-ad-booking. Stories are grouped by epic and tagged with the milestone they belong to.

## Scope

In scope:
- Venue network with search by place, venue type or distance from an address, shown on a map.
- Fictional credits, video upload with agency review, campaigns and slot booking.
- Agency back office for slots and prices, simulated broadcasts and estimated stats.

Out of scope:
- No real payment. Credits are fictional and topped up for free.
- No real screens. Broadcasts are simulated from bookings.
- Views are estimates (plays × average audience of the venue), not measured audience.
- No real brands or businesses. Venues are fictional; countries, regions and cities are real place names.

## Roles

| Role | Can do |
|------|--------|
| Agency admin | Manage the venue network, time slots, prices and demand rates; review videos; see all bookings |
| Advertiser | Manage the company profile, credits, videos and campaigns; see own stats |

## Glossary

| Term | Meaning |
|------|---------|
| Venue | A place with one or more screens, e.g. a supermarket or a pharmacy |
| Venue type | Business sector of a venue: supermarket, pharmacy, gym, bakery, mall, station… |
| Footfall | Visitor level of a venue: low, medium, high |
| Average audience | Average number of people who see one play at a venue, set by the agency |
| Time block | A daily time range, e.g. 08:00–10:00 |
| Slot | A time block at one venue on one date. It has a fixed number of spots |
| Spot | One advertiser's place in a slot |
| Booking | A spot taken by a campaign |
| Play | One showing of a video during a booked slot |
| Credit | Fictional currency used to pay for bookings |

## Milestones

| Milestone | Goal |
|-----------|------|
| M0 Foundation | Runnable app: accounts with roles, fictional venue network with filters, credits wallet, video upload |
| M1 Map and booking | Venues on a map, free slots, campaign creation and booking with credits |
| M2 Agency back office | Network, slots, prices and video review managed by the agency |
| M3 Broadcast and stats | Simulated broadcasts, plays, estimated views and reports |
| M4 Quality | CI, tests, accessibility, docs |

---

## EPIC 1 — Project setup

| ID | Story | Milestone |
|----|-------|-----------|
| SETUP-1 | Repository with README, LICENSE (MIT), .gitignore, .editorconfig | M0 |
| SETUP-2 | .NET solution: Api, Domain, Application, Infrastructure and Tests projects | M0 |
| SETUP-3 | SQLite + EF Core, first migration, database created on startup in dev | M0 |
| SETUP-4 | Angular workspace with the main app and shared libraries | M0 |
| SETUP-5 | GitHub Actions: build and test both API and front end on each push | M4 |

## EPIC 2 — Accounts

| ID | Story | Milestone |
|----|-------|-----------|
| AUTH-1 | As a visitor, I create an advertiser account with my company name, my name, email and password | M0 |
| AUTH-2 | As a user, I log in and get a token that carries my role | M0 |
| AUTH-3 | As a user, I log out, and I am sent back to login when my session expires | M0 |
| AUTH-4 | As a user, I change my password with current password, new password and confirmation | M0 |
| AUTH-5 | Demo accounts are seeded: one agency admin and two advertisers, with a documented demo password | M0 |
| AUTH-6 | As an advertiser, I edit my company profile: name, sector, address, contact email | M1 |

## EPIC 3 — Venue network

| ID | Story | Milestone |
|----|-------|-----------|
| NET-1 | The network is organized as country → region → city → venue | M0 |
| NET-2 | A venue has a name, venue type, footfall, location type (city centre, mall, station, suburb), address, coordinates, number of screens and average audience | M0 |
| NET-3 | A fictional demo network is seeded in France and Belgium, with venues spread over several regions and cities | M0 |
| NET-4 | As an advertiser, I list venues and filter by country, region, city, venue type, footfall and location type | M0 |
| NET-5 | As an advertiser, I find venues within a radius of an address (geocoding with OpenStreetMap Nominatim, distance computed by the API) | M1 |
| NET-6 | As an agency admin, I create, edit and deactivate venues | M2 |
| NET-7 | As an agency admin, I import venues from a CSV file | M2 |

## EPIC 4 — Map

| ID | Story | Milestone |
|----|-------|-----------|
| MAP-1 | As an advertiser, I see the filtered venues on a map (Leaflet, OpenStreetMap tiles) | M1 |
| MAP-2 | Venue markers show their state: free slots, fully booked, booked by me, playing for me now | M1 |
| MAP-3 | Close markers are grouped and split when I zoom in | M1 |
| MAP-4 | I click a marker to see the venue details and its free slots | M1 |
| MAP-5 | The map and the list stay in sync with the same filters | M1 |

## EPIC 5 — Credits

| ID | Story | Milestone |
|----|-------|-----------|
| CRED-1 | As an advertiser, I top up my fictional credits by choosing a pack; no payment is involved | M0 |
| CRED-2 | As an advertiser, I see my balance and every movement: top-up, hold, charge, release | M0 |
| CRED-3 | Credits are held when I book and charged when the slot is played; a cancelled booking releases them | M1 |
| CRED-4 | I cannot book more than my available balance | M1 |

## EPIC 6 — Videos

| ID | Story | Milestone |
|----|-------|-----------|
| VID-1 | As an advertiser, I upload a video of 10 MB max in MP4 or WebM | M0 |
| VID-2 | The API checks the real file type from its content, not only the extension, and rejects other files | M0 |
| VID-3 | As an advertiser, I see my video library with name, size, upload date and review status, and preview a video | M0 |
| VID-4 | As an advertiser, I rename or delete a video not used by an active booking | M1 |
| VID-5 | As an agency admin, I approve or reject a video with a reason; only approved videos can be broadcast | M2 |
| VID-6 | New videos start as "pending review"; the advertiser sees the status and the rejection reason | M0 |

## EPIC 7 — Campaigns and booking

| ID | Story | Milestone |
|----|-------|-----------|
| CAMP-1 | As an advertiser, I create a campaign with a name, start and end dates and one or more videos | M1 |
| CAMP-2 | I set the campaign targets: places (filters or a list of venues) and time blocks | M1 |
| CAMP-3 | I see the free slots matching my targets, with their price, and the total cost | M1 |
| CAMP-4 | I book the selected slots; each booking says which video plays, or videos rotate | M1 |
| CAMP-5 | Only free spots can be booked; two advertisers cannot take the last spot at the same time | M1 |
| CAMP-6 | I cancel a future booking and get my credits back | M1 |
| CAMP-7 | I see my campaigns with their status: draft, booked, running, finished | M1 |

## EPIC 8 — Agency back office

| ID | Story | Milestone |
|----|-------|-----------|
| ADM-1 | As an agency admin, I define the daily time blocks and the number of spots per slot | M2 |
| ADM-2 | As an agency admin, I set a base price per time block | M2 |
| ADM-3 | As an agency admin, I set demand rates by period, time block, venue type or venue; the slot price is base price × rate | M2 |
| ADM-4 | Price changes apply to new bookings only | M2 |
| ADM-5 | As an agency admin, I block slots (closure, maintenance) | M2 |
| ADM-6 | As an agency admin, I see slot occupancy by venue and period | M2 |

## EPIC 9 — Broadcast simulation and stats

| ID | Story | Milestone |
|----|-------|-----------|
| SIM-1 | A simulator turns past booked slots into plays, with a set number of plays per spot and slot | M3 |
| SIM-2 | In demo mode, an admin runs the simulator up to a chosen date | M3 |
| SIM-3 | Estimated views = plays × average audience of the venue | M3 |
| STAT-1 | As an advertiser, I see plays, broadcast time and estimated views per campaign | M3 |
| STAT-2 | I break the figures down by venue, venue type, city, region, country and period | M3 |
| STAT-3 | I see the exact list of venues where my videos ran | M3 |
| STAT-4 | I export my stats to CSV | M3 |
| STAT-5 | Estimated figures are clearly labelled as estimates | M3 |

## EPIC 10 — Quality and docs

| ID | Story | Milestone |
|----|-------|-----------|
| QA-1 | API integration tests for booking, credits and pricing | M4 |
| QA-2 | Concurrency test: two bookings on the last spot, only one succeeds | M4 |
| QA-3 | Unit tests for key Angular components and services | M4 |
| QA-4 | End-to-end test: log in, filter venues, book a slot, run the simulator, check stats (Playwright) | M4 |
| QA-5 | Accessibility check of the main pages, including a list view as an alternative to the map | M4 |
| QA-6 | Docs: setup, demo accounts, how prices and estimates are computed | M4 |

---

## Later ideas

- Automatic demand pricing from occupancy
- Google Maps as an alternative map provider
- Real payment for credits
- Several users per company
- Email notifications
- Screen player app that pulls its playlist from the API
