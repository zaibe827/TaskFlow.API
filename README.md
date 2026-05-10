# NetProject (ASP.NET Core + React)

Production-style starter built with:

- **Backend**: ASP.NET Core Web API (.NET 8), Clean Architecture-ish layering
- **Database**: SQL Server + EF Core (migrations included)
- **Auth**: JWT access tokens + rotating refresh tokens (stored hashed)
- **Mapping**: AutoMapper (manual DI registration)
- **Caching**: cache abstraction + in-memory implementation
- **Tests**: xUnit + Moq
- **Frontend**: React + Vite (TypeScript)

## Run prerequisites

- .NET SDK **8.x** (repo pinned via `global.json`)
- SQL Server (local) or SQL Server in Docker
- Node.js (recommended: latest LTS)

## Run the backend

From repo root:

```bash
dotnet run --project src/NetProject.Api/NetProject.Api.csproj
```

Default local URL with launch profile: `http://localhost:5078`
Swagger UI (dev): `http://localhost:5078/swagger`

The API will auto-apply migrations in Development.

### Connection string

Default is in `src/NetProject.Api/appsettings.json`:

`Server=localhost;Database=NetProjectDb;Trusted_Connection=True;TrustServerCertificate=True`

## Run the frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend reads `VITE_API_URL` from `frontend/.env.development`.
If you run API via compiled DLL (`dotnet NetProject.Api.dll`), use `http://localhost:5178`.
If you run API via `dotnet run`, use `http://localhost:5078`.

## Quick API summary

- ` Contain Endpoints wiht GET, POST, PUT and DELETE`


