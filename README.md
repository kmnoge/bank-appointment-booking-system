# Bank Branch Appointment Booking System

A Dockerised appointment booking starter for a bank branch, built with Angular, ASP.NET Core Web API, Entity Framework Core, SQL Server, and Redis.

## Features

- Angular reactive form for customer name, email, phone, branch, service, date, and time.
- ASP.NET Core REST API with request validation and consistent Problem Details errors.
- CQRS request separation with MediatR: commands handle writes (create/cancel), queries handle reads (branches/admin appointment listing), and controllers dispatch requests through `ISender`.
- SQL Server persistence with EF Core migrations, indexes, and automatic reference-branch seeding on API startup.
- Redis distributed slot lock to coordinate booking requests across API replicas; SQL Server's filtered unique index is the final protection against double-booking.
- Idempotency key support for safe request retries.
- Branch administrator dashboard to search and filter appointments by branch, date, status, customer details, and reference. Staff can cancel confirmed appointments.
- Branch-admin API routes require the `X-Branch-Admin-Key` header and fail closed when no key is configured.
- Dockerfiles for frontend and API, plus Docker Compose for SQL Server and Redis.
- Production app settings, health endpoint, CORS configuration, structured logging, and README run/test instructions.

## CQRS structure

The API uses Command Query Responsibility Segregation (CQRS) at the application request boundary. Read operations and state-changing operations are represented by separate MediatR request types and handlers:

- `CQRS/Commands/CreateAppointmentCommand.cs` and `CancelAppointmentCommand.cs` are write requests.
- `CQRS/Queries/GetBranchesQuery.cs` and `GetAdminAppointmentsQuery.cs` are read requests.
- `CQRS/Handlers/` contains one handler per request.
- Controllers validate HTTP-specific concerns and dispatch requests through MediatR rather than calling the application service directly.

This is CQRS without separate read/write databases: both paths currently use the same SQL Server-backed application service and schema, which keeps the starter straightforward to deploy. The handlers establish a clear boundary so validation behaviors, authorization policies, logging, and independent read models can be introduced later. Redis slot locking and SQL uniqueness protection remain in the booking write path.

## Prerequisites

- Docker Engine/Desktop with Docker Compose v2.
- For running outside Docker: .NET 8 SDK and Node.js 22+ / npm.

## Run with Docker (recommended)

1. Copy `.env.example` to `.env`.
2. Change `SQL_SA_PASSWORD` to a unique strong password. Keep `.env` out of source control.
3. Set `BRANCH_ADMIN_API_KEY` in `.env` to a long, unique random secret (at least 32 characters). This is a shared-key starter authentication mechanism, not a substitute for the bank's staff identity provider.
4. From the repository root, run:

   ```bash
   docker compose up --build -d
   docker compose ps
   ```

5. Open the app at <http://localhost:8081>.
6. API health check: <http://localhost:8081/health>. Swagger is intentionally disabled in production. To inspect the API during local development, run the API with `ASPNETCORE_ENVIRONMENT=Development` and browse `/swagger` on its local URL.
7. Follow startup logs if the database is still initializing:

   ```bash
   docker compose logs -f api
   ```

The API applies checked-in EF Core migrations and inserts the sample branch reference data when it starts. The sample branch data is demonstration data; replace it with approved branch records before production use.

To stop containers without deleting persisted data:

```bash
docker compose down
```

To delete the local SQL Server and Redis data as well (destructive):

```bash
docker compose down -v
```

## Run locally without Docker

Start SQL Server and Redis separately, then set these environment variables to point to them:

```bash
ConnectionStrings__Bookings="Server=localhost,1433;Database=BankAppointments;User Id=sa;Password=<your-password>;TrustServerCertificate=True;Encrypt=False"
ConnectionStrings__Redis="localhost:6379,abortConnect=false"
ASPNETCORE_ENVIRONMENT=Development
```

From `backend/BankAppointments.Api`:

```bash
dotnet restore
dotnet run
```

From `frontend` in another terminal:

```bash
npm install
npm start
```

The Angular dev server runs at <http://localhost:4200>. Configure `frontend/src/environments/environment.ts` if your API is not available at the expected `/api` path; for local development, use an Angular proxy or set the API base URL to the API origin.

## API overview

### `GET /api/appointments/branches`

Returns active branches.

### `GET /api/admin/appointments`
Returns up to 500 appointments for branch staff. Optional query parameters: `branchId`, `date` (`YYYY-MM-DD`), `status` (`Confirmed` or `Cancelled`), and `search` (reference, customer name, email, or phone number). Send the configured `X-Branch-Admin-Key` header.

Example:

```bash
curl -H 'X-Branch-Admin-Key: <your-admin-key>' \
  'http://localhost:8081/api/admin/appointments?status=Confirmed&date=2026-11-02'
```

### `POST /api/admin/appointments/{id}/cancel`
Cancels a confirmed appointment by its GUID. Requires the `X-Branch-Admin-Key` header. The appointment's slot becomes available again because only confirmed bookings occupy the unique slot index.

### `POST /api/appointments`

Example request (appointment time is interpreted as South African local time, UTC+02:00):

```json
{
  "fullName": "Alex Mokoena",
  "email": "alex@example.com",
  "phoneNumber": "+27 82 123 4567",
  "bankBranchId": 1,
  "serviceType": "Account services",
  "appointmentAt": "2026-11-02T10:30:00",
  "idempotencyKey": "client-generated-unique-request-id"
}
```

Success returns HTTP `201 Created` with a booking reference and confirmation message. Validation errors return `400`; a slot conflict returns `409`; unexpected errors return a generic `500` response while the detailed exception is written to server logs.

Quick test with `curl`:

```bash
curl -s http://localhost:8081/api/appointments/branches

curl -i -X POST http://localhost:8081/api/appointments \
  -H 'Content-Type: application/json' \
  -d '{"fullName":"Alex Mokoena","email":"alex@example.com","phoneNumber":"+27 82 123 4567","bankBranchId":1,"serviceType":"Account services","appointmentAt":"2026-11-02T10:30:00","idempotencyKey":"test-booking-001"}'
```

Choose a future weekday/time within the 90-day booking window when testing. Reuse the same idempotency key to exercise retry behaviour; use a new key to test a separate booking. To test concurrent slot protection, submit two different idempotency keys for the same branch and exact appointment time at nearly the same time: one should succeed and the other should receive `409 Conflict`.

## Tests and quality checks

Frontend unit test runner:

```bash
cd frontend
npm install
npm test
npm run build
```

Backend build:

```bash
cd backend/BankAppointments.Api
dotnet restore
dotnet build --configuration Release
```

The repository includes the Angular/Karma test runner configuration, API health endpoint, and reproducible API smoke-test commands. Add automated API integration tests (including parallel booking and idempotency cases) and CI pipeline enforcement before production rollout.

## Configuration

- `backend/BankAppointments.Api/appsettings.json`: local defaults. Override secrets and connection strings using environment variables.
- `backend/BankAppointments.Api/appsettings.Production.json`: production logging and CORS defaults. Configure `BranchAdmin__ApiKey` through the deployment environment or secret manager; do not commit a real key. Configure `ConnectionStrings__Bookings`, `ConnectionStrings__Redis`, and `Cors__AllowedOrigins__0` through the deployment environment or secret manager.
- `.env`: local Docker Compose secrets only; never commit it.

## Production deployment checklist

This is a production-oriented foundation, not a claim of a completed bank security certification. Before exposing it to customers:

1. Use managed SQL Server and Redis with TLS, authentication, private networking, backups, monitoring, and disaster-recovery tests.
2. Put the app behind HTTPS ingress/reverse proxy, set exact CORS origins, and store credentials in a secret manager.
3. Replace the shared branch-admin API key with the bank-approved identity provider (OIDC/OAuth2), role-based authorization, and branch-scoped permissions. Add rate limiting, audit logging for every staff action, bot protection, and data-retention controls.
4. Review POPIA obligations, consent/privacy notices, data minimisation, access logging, and incident response with the bank's security/privacy teams.
5. Add email/SMS confirmation through an approved provider, rescheduling workflows, and audited appointment history. The starter admin dashboard currently supports viewing, filtering, searching, and cancellation.
6. Review the appointment policy, timezone, branch hours, public holidays, slot duration/capacity, and the sample branch list with business owners.
7. Add automated integration/load/security tests and CI/CD image scanning; pin container images and NuGet/npm dependencies to the versions approved by your release process.

## Design notes

Redis lock keys are scoped to branch and appointment slot. Lock release uses a compare-and-delete Lua script so a request cannot delete a lock acquired later by another request. Redis is not the only concurrency control: the filtered unique SQL index protects against duplicate confirmed bookings if lock timing or infrastructure fails. The current design supports one confirmed appointment per branch per 30-minute start time; if each branch can serve multiple customers simultaneously, introduce explicit slot capacity and atomic capacity reservation instead of removing the uniqueness guard blindly.

## CQRS application design (updated)

The CQRS path is split into distinct request types, handlers, and read/write service contracts:

- **Commands:** `CreateAppointmentCommand`, `CancelAppointmentCommand`.
- **Queries:** `GetBranchesQuery`, `GetAdminAppointmentsQuery`.
- **Handlers:** each request has one MediatR handler under `CQRS/Handlers`.
- **Write-side boundary:** `IAppointmentCommandService` / `AppointmentCommandService` own booking and cancellation mutations.
- **Read-side boundary:** `IAppointmentQueryService` / `AppointmentQueryService` own read-only projections and filtered reads.
- **Validation:** FluentValidation validators run before handlers via `ValidationBehavior`.
- **Observability:** `RequestLoggingBehavior` logs request type and duration without logging request payloads or customer PII.
- **HTTP error contract:** validation errors return `400`, unavailable booking slots return `409`, and unexpected errors return a generic `500` Problem Details response.

The read and write services currently use the same SQL Server database and EF Core context. This is CQRS with separated application paths, not separate databases/event sourcing. Keeping a shared database is intentional for this deployment size; read replicas or a separate read model can be introduced later without changing controller request dispatch.

## Tests

Run backend unit tests (requires .NET 8 SDK):

```bash
dotnet test tests/BankAppointments.UnitTests/BankAppointments.UnitTests.csproj --configuration Release
```

The test project currently covers command validation. Before production, add integration tests against SQL Server/Redis for booking success, idempotent retries, cancellation, duplicate-slot races across multiple API instances, and Redis outage behaviour. Run the frontend test suite from `frontend/` with `npm ci && npm test`.

## Production hardening checklist

This repository is a production-oriented starter, not a bank-approved or security-certified deployment. Before exposing it to real customers:

1. Replace the shared `X-Branch-Admin-Key` mechanism with the bank's OIDC/OAuth identity provider and role/branch claims. Enforce branch scope in server-side authorization, not only in the UI.
2. Configure HTTPS/TLS end-to-end; use managed secrets for SQL Server, Redis, and identity credentials. Never commit `.env` or production secrets.
3. Use a dedicated least-privilege SQL identity instead of `sa` outside local Docker development. Encrypt SQL and Redis traffic in production.
4. Add audit events for staff actions, retention controls, monitoring/alerting, rate limiting, backup/restore tests, and a security review aligned with POPIA and bank policy.
5. Review business-hours, public holidays, branch-local time zone, capacity, cancellation policy, and slot availability rules with the business owner.
6. Run build, automated tests, dependency/container scans, load tests, and multi-instance concurrency tests in CI before deployment.

### Important concurrency note

Redis locking reduces contention across API replicas; it is not the correctness boundary by itself. The SQL Server filtered unique index on `(BankBranchId, AppointmentAtUtc)` for confirmed appointments remains the final protection against two confirmed bookings for one branch/time. The lock uses a random token and compare-and-delete Lua release. The 60-second lock TTL should be monitored and tuned to the expected maximum write duration.

### Time handling

The current domain rules assume South African standard time (UTC+02:00) and a fixed offset. If the system expands beyond South Africa or needs formal daylight-saving/time-zone support, store a configured IANA time-zone ID per branch and convert using `TimeZoneInfo` instead of relying on a fixed offset.
