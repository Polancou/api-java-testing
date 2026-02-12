# ApiBaseCore: .NET 9 Clean Architecture API Template

A robust, enterprise-grade API template built with **.NET 9** following **Clean Architecture** principles. Designed to be a solid foundation for scalable applications.

---

## 🚀 Key Features

### 🛡️ Advanced Security
* **JWT & Refresh Tokens:** Secure authentication implementation.
* **Rate Limiting:** Protects against abuse using .NET 9 middleware.
* **Role-Based Access Control:** Built-in `Admin` and `User` roles.

### 🏗️ Architecture & Backend
* **Clean Architecture:** Strict separation of concerns (`Domain`, `Application`, `Infrastructure`, `Api`).
* **Service Abstraction:** Decoupled implementations for Storage (`IFileStorageService`) and Email (`IEmailService`).
* **Health Checks:** `/health` endpoint for monitoring.
* **Entity Framework Core 9:** SQL Server with optimistic concurrency support (`RowVersion`).
* **Dockerized:** Ready-to-deploy with Docker Compose.

---

## 🐳 Quick Start with Docker Compose

### 1. Prerequisites
- **Docker Desktop** installed an running.

### 2. Configuration
1.  Copy the environment file:
    ```bash
    cp .env.example .env
    ```
2.  Update `.env` with your credentials (especially `SA_PASSWORD`).

### 3. Run
```bash
docker-compose up --build
```

Once started:
- **Swagger API Docs:** [http://localhost:5272/swagger](http://localhost:5272/swagger)
- **Health Check:** [http://localhost:5272/health](http://localhost:5272/health)

### 4. Database
Migrations are applied automatically on container startup.

---

## 🛠️ Technology Stack

- **Framework:** .NET 9 (C# 13)
- **Database:** SQL Server (Azure SQL Edge)
- **ORM:** Entity Framework Core 9
- **Auth:** JWT (Bearer) + Google OAuth 2.0
- **Validation:** FluentValidation
- **Logging:** Serilog
- **Testing:** xUnit, Moq, FluentAssertions, WebApplicationFactory

---

## 🚀 Manual Execution (Local Dev)

### Configure & Run Backend

```bash
cd ApiBaseCore
dotnet user-secrets init
dotnet run --project ApiBaseCore.Api
```

### User Secrets (Optional for local dev without Docker)

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=ApiBaseCore_db;User Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True;"
dotnet user-secrets set "Jwt:Key" "SUPER_SECRET_KEY_MIN_64_CHARS_LONG_FOR_HMAC_SHA512"
```

---

## 🧪 Testing

```bash
cd ApiBaseCore
dotnet test
```