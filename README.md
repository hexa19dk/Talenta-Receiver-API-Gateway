### 🧠 Project Overview

Talenta-Receiver-API-Gateway is a backend service that acts as an API Gateway for orchestrating and managing API traffic between client applications and backend services (such as HRIS/Payroll systems) — particularly Talenta API integrations (Talenta by Mekari). It centralizes authentication, routing, business logic orchestration, and data flow with external systems like Talenta.
This project was built as part of a real integration workflow where API calls are validated, handled, and data is persisted or forwarded to downstream services.

## 🚀 Tech Stack

| Category | Technology |
|-------|-----------|
| Language | C# |
| Framework | ASP.NET Core (.NET Core) |
| API Style | RESTful API |
| Database | MySQL |
| ORM / Data Access | Repository Pattern (EF Core or custom implementation) |
| Architecture | Layered Architecture + Clean Architecture |
| Containerization | Docker |
| CI/CD | Jenkins |
| Configuration | appsettings.json |
| Utilities | Custom helpers, validators, and mappers |

### 📦 Project Structure

Talenta-Receiver-API-Gateway
📁 Config                – Application configuration setup
📁 Mappers               – DTO/Model mapping utilities
📁 Models                – Domain/data models
📁 Repositories          – Data access layer
📁 Services              – Business logic/services
📁 UseCases              – Application use case orchestrators
📁 Utils                 – Helpers & utilities
📁 Validators            – Input/request validators
📁 Protos                – (protobufs if gRPC or integration events)
Dockerfile              – Container build definition
Jenkinsfile             – Pipeline automation script
Program.cs              – Entry point
Startup.cs              – Middleware, DI, routing


## 🧠 Design Patterns & Principles

### 1. API Gateway Pattern
- Single entry point for external clients
- Centralized request validation and routing
- Simplifies client-side integrations

### 2. Service Layer Pattern
- Encapsulates business logic
- Keeps controllers thin and focused

### 3. Repository Pattern
- Abstracts database access
- Improves testability and separation of concerns

### 4. Use Case Pattern
- Represents specific business workflows
- Coordinates multiple services and repositories

### 5. Separation of Concerns (SoC)
- Each layer has a single responsibility
- Improves readability and maintainability


## 🔐 Security & Validation

- Request validation handled through **Validators**
- Supports secure authentication mechanisms (e.g., HMAC / token-based headers)
- Prevents invalid or malformed requests from reaching core services


## 🔄 Typical Request Flow

1. Client sends a request to the API Gateway
2. Gateway validates request headers and payload
3. Request is routed to the appropriate controller
4. Controller delegates logic to the Service / Use Case layer
5. Service performed:
   - Business logic
   - External API calls (Talenta)
   - Database operations (via repositories)
6. Response is standardized and returned to the client


## 🧪 Running the Project Locally

### Prerequisites
- .NET SDK
- MySQL
- Docker (optional)

### Steps

```bash
git clone https://github.com/hexa19dk/Talenta-Receiver-API-Gateway.git
cd Talenta-Receiver-API-Gateway

# Update database connection and secrets in appsettings.json
dotnet restore
dotnet build
dotnet run
