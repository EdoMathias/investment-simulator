# Investment Simulator

A full-stack demo simulating user investments, featuring a clean separation between a .NET backend and a Vite + React frontend.

---

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Setup & Quick Start](#setup--quick-start)
4. [Architecture](#architecture)
5. [API Reference](#api-reference)

---

## Overview

This project demonstrates:
- A minimal .NET 9 REST API for investment simulation (JSON file storage)
- A modern Vite + React frontend (TypeScript)
- Local development with proxying

---

## Project Structure

- `backend/InvestmentServer` — Main API: business logic, JSON file storage, background worker
- `frontend/` — Vite + React app (TypeScript)

---

## Setup & Quick Start

**Prerequisites:**
- .NET 9 SDK
- Node.js 18+ and npm/yarn

**Run locally (Windows):**

```powershell
# Backend (API)
cd backend\InvestmentServer
dotnet run

# In a second terminal — Frontend (dev)
cd frontend
npm install
npm run dev
```

Open [http://localhost:5173](http://localhost:5173) (frontend). The frontend proxies API calls to the backend at [http://localhost:5055](http://localhost:5055).

---

## Architecture

**Backend:**
- .NET 9 REST API
- Stores account state in a JSON file (`accounts.json`)
- Dependency Injection for services
- Background worker processes investments asynchronously

**Frontend:**
- React + Vite (TypeScript)
- Communicates with backend via `/api/*` endpoints

---

## API Reference

Key backend endpoints ([see source](backend/InvestmentServer/Api/Endpoints.cs)):

- `GET /api/health` — Health check
- `POST /api/login` — Set current user
- `POST /api/logout` — Clear current user
- `GET /api/state` — Get account state
- `GET /api/investment-options` — List available investment options
- `GET /api/investment-history` — Get past investments
- `POST /api/invest` — Start an investment
- `/events/completions/stream` — Server-Sent Events stream for investment completions