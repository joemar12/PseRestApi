# PseRestApi

A REST API for querying Philippine Stock Exchange (PSE) stock price data. This application provides endpoints to retrieve the 
latest stock prices, historical prices for specific dates, and paginated price history within date ranges. There's a dedicated 
stock price scraper Docker image published to GHCR that can be run on a schedule.

## Tech Stack

- **.NET 10**
- **Entity Framework Core with a PostgreSQL DB behind it**
