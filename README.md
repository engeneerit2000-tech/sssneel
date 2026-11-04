# SNEL Wallet Platform API

Backend API for the SNEL Wallet Platform, built with Clean Architecture on ASP.NET Core and EF Core.

## Project Structure

- `SNEL.WalletPlatform.Domain`
  - Domain entities and enums
- `SNEL.WalletPlatform.Application`
  - Use-case services, interfaces, models, AutoMapper profiles, shared result/localization contracts
- `SNEL.WalletPlatform.Infrastructure`
  - EF Core persistence, repositories, Unit of Work, connector implementations, dependency wiring
- `SNEL.WalletPlatform.Presentation`
  - API controllers, middleware, authentication, request localization

## Implemented Modules

- Customers
  - Customer service/controller flow with localized responses
- Billing Inquiry
  - `IBillingServiceService` inquiry flow
  - Connector pattern (`BillerConnector` + resolver)
  - `SnelBillerConnector` querying invoices from DB
  - Supports `unpaidOnly` filter (default `false`)

## Data Access & Migrations

- EF Core + PostgreSQL (`Npgsql`)
- `ApplicationDbContext` in Infrastructure
- Startup DB initialization uses migrations (`Database.MigrateAsync()`)
- Migrations are in:
  - `SNEL.WalletPlatform.Infrastructure/Persistence/Migrations`

### Common EF Commands

```bash
dotnet ef migrations add Initial \
  --project SNEL.WalletPlatform.Infrastructure/SNEL.WalletPlatform.Infrastructure.csproj \
  --startup-project SNEL.WalletPlatform.Presentation/SNEL.WalletPlatform.Presentation.csproj \
  --context SNEL.WalletPlatform.Infrastructure.Persistence.ApplicationDbContext \
  --output-dir Persistence/Migrations
```

```bash
dotnet ef database update \
  --project SNEL.WalletPlatform.Infrastructure/SNEL.WalletPlatform.Infrastructure.csproj \
  --startup-project SNEL.WalletPlatform.Presentation/SNEL.WalletPlatform.Presentation.csproj \
  --context SNEL.WalletPlatform.Infrastructure.Persistence.ApplicationDbContext
```

## API/Service Conventions

- Controllers return `ApiResponse<T>` via `ApiControllerBase.FromResult(...)`
- Application services return `Result<T>` / `PagedResult<T>`
- User-facing messages are localized via:
  - `SNEL.WalletPlatform.Application/Common/Localization/LocalizationKeys.cs`
  - `SNEL.WalletPlatform.Application/SharedResource.resx`
  - `SNEL.WalletPlatform.Application/Resources/SharedResource.en.resx`
  - `SNEL.WalletPlatform.Application/Resources/SharedResource.fr.resx`

## Testing

- xUnit tests:
  - `SNEL.WalletPlatform.Application.Tests`
- Run:

```bash
dotnet test SNEL.WalletPlatform.Application.Tests/SNEL.WalletPlatform.Application.Tests.csproj
```
