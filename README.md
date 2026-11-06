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

Early development. Setup instructions will be added as the code lands.

## License

[MIT](LICENSE)
