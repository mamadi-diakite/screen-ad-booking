# screen-ad-booking

A web app for an advertising agency that sells video ad slots on screens placed in shops and public venues.

Companies create an account, top up fictional credits, upload short videos and build campaigns: they choose where (countries, regions, cities, venue types, or venues around an address on a map), when (periods and time slots) and which videos to show. Slots are limited and priced by the agency, and every video is reviewed by the agency before it can be broadcast. Broadcasts are simulated, and each company sees where and when its videos ran, with an estimated number of views.

## Who it is for

Agencies that run a network of screens in shops and public venues, and the companies that want to advertise on them.

## What it is not

- No real payment: credits are fictional.
- No real screens: broadcasts are simulated from bookings.
- Views are estimates, not measured audience.
- Demo venues and brands are fictional.

## Stack

- Front end: Angular workspace (app + libraries), Leaflet with OpenStreetMap tiles
- Back end: ASP.NET Core Web API (C#)
- Database: SQLite with EF Core

## Status

Early development (milestone M0). What works today:

- Create an advertiser account together with its company, log in, change your password. Sessions expire after 60 minutes.
- Browse the venue network and filter it by country, region, city, venue type, footfall and location type.
- Top up fictional credits by choosing a pack, and see the balance and every movement.
- Upload MP4 or WebM videos (10 MB max, the real file type is checked from its content), see the library with each video's review status, and preview a video.

Campaigns, bookings, the map, video review by the agency and simulated broadcasts are planned (see [ROADMAP.md](ROADMAP.md)).

## Getting started

### Prerequisites

- [.NET SDK 10.0.403](https://dotnet.microsoft.com/download) or a later 10.0 feature band
- [Node.js 24](https://nodejs.org/) with npm 11

### Run the API

```bash
cd src/backend
dotnet run --project src/ScreenAdBooking.Api --launch-profile http
```

The API listens on http://localhost:5084. In Development it creates the SQLite database in `src/backend/src/ScreenAdBooking.Api/App_Data/`, applies the migrations, and adds the demo data below when the database has no users. Uploaded videos are stored in `App_Data/videos`.

The access tokens are signed with a key that is never stored in the repository. In Development, a random key is generated at startup, so you need to log in again after each restart. In any other environment, set a base64 key of at least 32 bytes:

```bash
export Jwt__SigningKey="$(openssl rand -base64 32)"
```

### Run the web app

```bash
cd src/frontend
npm install
npm start
```

Open http://localhost:4200. Calls to `/api` are sent to the API on port 5084.

### Demo data

All demo accounts use the password `TestDemo123!`. They are fictional and only exist in Development.

| Name | Email | Role | Company | Starting credits |
|------|-------|------|---------|------------------|
| Paul Mercier | paul.mercier@example.com | Advertiser | Lumen Bakery | 2,000 (Standard pack) |
| Lina Chevalier | lina.chevalier@example.com | Advertiser | Urban Cycle Shop | 500 (Starter pack) |
| Nora Vidal | nora.vidal@example.com | Agency admin | — | — |

The agency admin can browse the venues; the credits and video pages belong to advertisers. Agency tools come with later milestones.

The demo network has 26 fictional venues in 13 real cities of France and Belgium, spread over 8 regions: supermarkets, pharmacies, bakeries, gyms, shopping malls, stations, restaurants and cinemas.

To start again from an empty demo, stop the API and delete `src/backend/src/ScreenAdBooking.Api/App_Data/`.

### Run the tests

Back end (with coverage, migrations and generated code excluded):

```bash
cd src/backend
dotnet test --settings coverage.runsettings --collect "XPlat Code Coverage"
```

Front end (Vitest, the run fails below 100% coverage):

```bash
cd src/frontend
npm run test:ci
```

## Project layout

```
src/backend/    ASP.NET Core solution: Domain, Application.Contracts, Application, Infrastructure, Api, and their test projects
src/frontend/   Angular workspace: projects/apps/screen-ad-booking-app and the shared libraries in projects/libs/shared
```

## License

[MIT](LICENSE)
