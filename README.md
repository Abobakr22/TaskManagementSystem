# Task Management System API

A robust and scalable RESTful API built with **.NET 9** applying **Clean Architecture** principles and **SOLID** design patterns. Designed for efficient task management with background processing, distributed caching, and secure authentication.

---

## 🚀 Features

- **Clean Architecture** — Separation of concerns across Domain, Application, Infrastructure, and API layers
- **JWT Authentication & Authorization** — Secure, role-based endpoint protection
- **Background Task Processing** — Async status updates via `.NET Channels` (in-memory queue) and `IHostedService`
- **Distributed Caching** — Redis integration to optimize read performance and reduce database load
- **Entity Framework Core 9** — Code-first approach with SQL Server

---

## 🛠️ Technologies

| Technology | Purpose |
|---|---|
| .NET 9.0 (ASP.NET Core) | Web API framework |
| Entity Framework Core 9.0 | ORM & migrations |
| MS SQL Server | Primary database |
| Redis | Distributed caching |
| BCrypt | Password hashing |
| Swagger / OpenAPI | API documentation |

---

## ⚙️ Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- MS SQL Server (LocalDB or full instance)
- Docker Desktop (for Redis)

---

## 🏃 Getting Started

### 1. Start Redis

```bash
docker run --name task-redis -p 6379:6379 -d redis
```

### 2. Apply Database Migrations

Open **Package Manager Console** in Visual Studio, set the default project to `TaskManagementSystem.Infrastructure`, then run:

```powershell
Update-Database
```

> The app applies pending migrations automatically on startup, but running this manually ensures the database is ready before first launch.

### 3. Run the API

Set `TaskManagementSystem.Api` as the startup project and press **F5**.

Swagger UI will open at:
```
https://localhost:<port>/swagger
```

---

## 🔐 Authentication

A default admin account is seeded automatically on first run:

| Field | Value |
|---|---|
| Email | `admin@example.com` |
| Password | `Admin@123` |

### Testing Protected Endpoints

1. Call `POST /api/Auth/login` with the credentials above
2. Copy the returned token
3. Click **Authorize** in Swagger UI
4. Enter `Bearer <your_token>` (e.g., `Bearer eyJhbG...`)
5. Click **Authorize** — all protected Task and Admin endpoints are now accessible
