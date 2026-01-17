# Investment Simulator

A small full‑stack demo that simulates simple user investments. The project demonstrates a clear separation between a minimal .NET backend and a Vite + React frontend, with a proxy for local development.

---

## Quick start

Prerequisites:
- .NET 9 SDK (for backend)
- Node.js 18+ and npm/yarn (for frontend)

Clone and run locally (Windows example shown):

```powershell
# backend (API)
cd backend\InvestmentServer
dotnet run

# in a second terminal — frontend (dev)
cd frontend
npm install
npm run dev
```

Open http://localhost:5173 (frontend). The frontend proxies API calls to the backend at http://localhost:5055.

---

## What’s included

- `backend/InvestmentServer` — Main API (business logic, in-memory storage, background worker).
- `backend/DemoServer` — Small demo/static server used for examples.
- `frontend/` — Vite + React app (TypeScript).
- `frontend/src/api` — Lightweight API client and endpoint constants.

---

## Architecture & behavior

- API is a tiny REST service (no DB) that stores account state in memory and runs a background worker to complete timed investments.
- Frontend communicates with the API via `/api/*` and uses a Vite dev proxy (see `vite.config.ts`).
- Key backend endpoints (see `backend/InvestmentServer/Program.cs`):
  - `GET /api/health` — health check
  - `POST /api/login` — set current user
  - `GET /api/state` — get account state
  - `GET /api/investment-options` — available options
  - `POST /api/invest` — start an investment

---

## Local development details

Backend (InvestmentServer):
```powershell
cd backend\InvestmentServer
# development (defaults to http://localhost:5055)
dotnet run
```

Frontend (dev server):
```bash
cd frontend
npm install
npm run dev   # opens on http://localhost:5173 by default
```

Notes:
> The frontend proxies `/api` to `http://localhost:5055` — no CORS configuration required for the dev setup.

---

## Build & production (minimal)

Build frontend:
```bash
cd frontend
npm run build
# preview the production build
npm run preview
```

Publish backend (example):
```powershell
cd backend\InvestmentServer
dotnet publish -c Release -o ./publish
```

---

## Where to look in the code

- Backend
  - `InvestmentServer/Program.cs` — routes and DI
  - `InvestmentServer/Services/InvestmentService.cs` — core investment logic
  - `InvestmentServer/Storage/InMemoryAccountStore.cs` — in-memory state
  - `InvestmentServer/Workers/InvestmentProcessor.cs` — background worker
- Frontend
  - `frontend/src/pages` — UI pages (login, investment)
  - `frontend/src/api` — `endpoints.ts`, `http.ts` (API client)

