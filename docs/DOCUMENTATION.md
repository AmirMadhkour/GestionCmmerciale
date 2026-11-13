# Gestion Commerciale — Full Project Documentation

A web application for managing clients, products and sales orders.
**Back end:** ASP.NET Core 8 Web API (Clean Architecture) · **Front end:** Angular 21 · **Database:** SQL Server (managed with SSMS).

---

## Table of contents

1. [Project overview](#1-project-overview)
2. [Prerequisites and installation](#2-prerequisites-and-installation)
3. [Getting the project running](#3-getting-the-project-running)
4. [Repository structure](#4-repository-structure)
5. [Architecture and design logic](#5-architecture-and-design-logic)
6. [Back end in detail](#6-back-end-in-detail)
7. [REST API reference](#7-rest-api-reference)
8. [Database (SQL Server / SSMS)](#8-database-sql-server--ssms)
9. [Front end in detail](#9-front-end-in-detail)
10. [Security](#10-security)
11. [Configuration reference](#11-configuration-reference)
12. [Testing the application](#12-testing-the-application)
13. [Troubleshooting](#13-troubleshooting)
14. [Known limitations and possible improvements](#14-known-limitations-and-possible-improvements)

---

## 1. Project overview

### 1.1 Purpose

Gestion Commerciale lets a company:

- keep a **client** directory,
- maintain a **product** catalogue with stock levels,
- create **orders** made of several lines, compute totals excluding tax (HT) and including tax (TTC) with a 19 % VAT, and follow each order through a life cycle (draft → validated → cancelled),
- restrict sensitive operations to **administrators**.

### 1.2 Main features

| Area | Features |
|---|---|
| Authentication | Login with email and password, JWT session, automatic logout when the token expires |
| Clients | List with search, detail page with order history and revenue, create, edit, delete |
| Products | List with search and stock badges, detail page with VAT breakdown, create, edit, delete |
| Orders | List with status filter and counters, creation with live totals, edit while in draft, validate, cancel, delete |
| Stock | Decreased when an order is validated, restored when a validated order is cancelled |
| Roles | `ADMIN` has full access; other roles are read and order-creation only |

### 1.3 Technology stack

| Layer | Technology |
|---|---|
| Front end | Angular 21 (standalone components, signals, zoneless), TypeScript 5.9, Tailwind CSS 3, RxJS 7 |
| Back end | ASP.NET Core 8, C# 12, Entity Framework Core 8, FluentValidation 12 |
| Security | JWT Bearer, BCrypt password hashing |
| Database | Microsoft SQL Server (Express edition is enough) |
| API docs | Swagger / OpenAPI (Swashbuckle) |

---

## 2. Prerequisites and installation

Everything below is installed once on the development machine (Windows 10/11).

### 2.1 Required software

| Tool | Version | Purpose | Where to get it |
|---|---|---|---|
| **.NET SDK** | 8.0 or later | Build and run the API | https://dotnet.microsoft.com/download |
| **Node.js** | 24 or later (tested with 26) | Run the Angular tooling | https://nodejs.org |
| **npm** | 10 or later (installed with Node.js) | Install front-end packages | — |
| **SQL Server** | 2019 or later, *Express* is fine | Database engine | https://www.microsoft.com/sql-server/sql-server-downloads |
| **SQL Server Management Studio (SSMS)** | 19 or later | Browse and query the database | https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms |
| **Git** | any recent version | Version control | https://git-scm.com |
| **Code editor** | Visual Studio 2022 or VS Code | Editing | — |

### 2.2 Required command-line tool

The EF Core command-line tool is used to create and apply database migrations:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

### 2.3 Check the installation

```powershell
dotnet --version        # 8.x
node --version          # v24 or higher
npm --version
dotnet ef --version     # 8.x
```

### 2.4 SQL Server setup notes

- During the SQL Server Express installation choose the **Basic** type. The default instance name is `SQLEXPRESS`.
- The project connects with **Windows authentication** to `localhost\SQLEXPRESS`. No SQL login is needed.
- Make sure the Windows service **SQL Server (SQLEXPRESS)** is *Running* (Services → `MSSQL$SQLEXPRESS`).

---

## 3. Getting the project running

### 3.1 Back end

```powershell
cd backend
dotnet restore
dotnet build
dotnet run --project src/GestionCommerciale.Api
```

The API listens on **http://localhost:5013** (HTTPS: https://localhost:7210).
Swagger UI is available at **http://localhost:5013/swagger** (opening `http://localhost:5013/` redirects there). It documents all 20 endpoints.

On first start the application automatically:

1. creates the `Mini_Projet` database if it does not exist,
2. applies every pending migration,
3. inserts the default administrator account (see below).

### 3.2 Front end

```powershell
cd Frontend
npm install
npm start
```

The application is served at **http://localhost:4200**.
Every request to `/api/...` is forwarded to the API on port 5013 by the development proxy (`proxy.conf.json`), so no CORS configuration is needed during development.

### 3.3 Default administrator account

| Field | Value |
|---|---|
| Email | `Amir@isi.com.tn` |
| Password | `Admin123!` |
| Role | `ADMIN` |

Change this password and the JWT secret before any real deployment (see [Security](#10-security)).

### 3.4 Manual database commands (optional)

The database is normally created by the API at start-up. The same result can be obtained manually:

```powershell
cd backend
dotnet ef database update --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api
```

Creating a new migration after changing an entity:

```powershell
dotnet ef migrations add <MigrationName> --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api
```

### 3.5 Production build of the front end

```powershell
cd Frontend
npm run build
```

The output is written to `Frontend/dist/gestion-commerciale`. In production the API address is configured in `src/environments/environment.ts` (`apiUrl`).

---

## 4. Repository structure

```
mini_project_isi_madhkour/
├── backend/
│   ├── GestionCommerciale.sln
│   ├── src/
│   │   ├── GestionCommerciale.Domain/          Entities, enums, business rules
│   │   ├── GestionCommerciale.Application/     Use cases, DTOs, validators, interfaces
│   │   ├── GestionCommerciale.Infrastructure/  EF Core, repositories, JWT, hashing, migrations
│   │   └── GestionCommerciale.Api/             Controllers, middleware, configuration
│   └── tests/
│       ├── GestionCommerciale.UnitTests/
│       └── GestionCommerciale.IntegrationTests/
├── Frontend/                                   Angular application
├── database/                                   Database-related files
└── docs/                                       Documentation
```

### 4.1 Back-end projects in detail

```
GestionCommerciale.Domain/
├── Common/BaseEntity.cs                Id (Guid) and CreatedAt shared by every entity
├── Constants/                          Roles, TaxConstants (VAT rate 0.19)
├── Entities/                           User, Client, Product, Order, OrderLine
├── Enums/                              UserRole, OrderStatus
└── Exceptions/BusinessRuleException.cs

GestionCommerciale.Application/
├── Common/Exceptions/                  NotFound, Conflict, Unauthorized, Forbidden
├── Common/Interfaces/                  ICurrentUserService, IDateTimeProvider
├── DTOs/                               Auth, Clients, Products, Orders
├── Interfaces/Repositories/            IRepository<T>, IClientRepository, ...
├── Interfaces/Security/                IPasswordHasher, IJwtTokenGenerator
├── Interfaces/Services/                IAuthService, IClientService, ...
├── Mappings/                           Entity → DTO extension methods
├── Services/                           AuthService, ClientService, ProductService, OrderService
├── Validators/                         FluentValidation rules per DTO
└── DependencyInjection.cs

GestionCommerciale.Infrastructure/
├── Data/AppDbContext.cs
├── Data/Configurations/                Fluent API mapping per entity
├── Data/Seed/DbSeeder.cs               Migration + default admin
├── Migrations/                         EF Core migrations
├── Repositories/                       Repository<T>, specific repositories, UnitOfWork
├── Security/                           JwtTokenGenerator, PasswordHasher, JwtSettings
├── Services/DateTimeProvider.cs
└── DependencyInjection.cs

GestionCommerciale.Api/
├── Controllers/                        Auth, Clients, Products, Orders
├── Extensions/                         JWT, authorization policies, CORS, Swagger, database start-up
├── Filters/ValidationFilter.cs
├── Middleware/ExceptionHandlingMiddleware.cs
├── Services/CurrentUserService.cs
├── Program.cs
└── appsettings.json
```

### 4.2 Front-end structure

```
Frontend/
├── package.json · angular.json · tsconfig*.json
├── tailwind.config.js · postcss.config.js · proxy.conf.json
└── src/
    ├── index.html · main.ts · styles.css
    ├── environments/                   environment.ts, environment.development.ts
    └── app/
        ├── app.component.*  app.config.ts  app.routes.ts
        ├── core/                       constants, models, services, interceptors, guards, utils
        ├── layout/                     main-layout, navbar
        ├── shared/                     reusable components, directive, pipe
        └── features/                   auth, clients, products, orders
```

---

## 5. Architecture and design logic

### 5.1 Clean Architecture

The back end is split into four projects. **Dependencies always point inward**, towards the domain:

```
        ┌──────────────────────────────────────────────┐
        │  Api  (controllers, middleware, DI wiring)   │
        │  ┌────────────────────────────────────────┐  │
        │  │ Infrastructure (EF Core, JWT, BCrypt)  │  │
        │  │  ┌──────────────────────────────────┐  │  │
        │  │  │ Application (use cases, DTOs,    │  │  │
        │  │  │ validators, interfaces)          │  │  │
        │  │  │  ┌────────────────────────────┐  │  │  │
        │  │  │  │ Domain (entities, rules)   │  │  │  │
        │  │  │  └────────────────────────────┘  │  │  │
        │  │  └──────────────────────────────────┘  │  │
        │  └────────────────────────────────────────┘  │
        └──────────────────────────────────────────────┘
```

| Project | Knows about | Never knows about |
|---|---|---|
| **Domain** | nothing | EF Core, HTTP, DTOs |
| **Application** | Domain | EF Core, SQL Server, ASP.NET |
| **Infrastructure** | Application, Domain | Controllers |
| **Api** | Application, Infrastructure | — |

**Why this structure**

- **Business rules stay testable and independent.** An order refuses to be validated when it is empty, whatever database or web framework is used.
- **Technology can change.** The Application layer only depends on interfaces such as `IClientRepository` or `IPasswordHasher`. SQL Server or BCrypt could be replaced by changing only Infrastructure.
- **Each class has one job.** Controllers translate HTTP, services orchestrate use cases, entities protect their own rules, repositories talk to the database.

### 5.2 Life of a request

Example: `POST /api/orders` (create an order).

```
Browser (Angular)
   │  HTTP + "Authorization: Bearer <jwt>"
   ▼
ExceptionHandlingMiddleware         catches every exception, converts to a clean JSON error
   ▼
Authentication / Authorization      validates the JWT, checks the role
   ▼
ValidationFilter                    runs the FluentValidation rules on the request body
   ▼
OrdersController.Create             thin: forwards to the service
   ▼
OrderService.CreateAsync            loads client and products, builds the Order entity
   ▼
Order (Domain)                      enforces the business rules (quantity > 0, draft only ...)
   ▼
Repository + UnitOfWork             adds the entity, then SaveChangesAsync (one transaction)
   ▼
SQL Server
```

### 5.3 Patterns used

| Pattern | Where | Reason |
|---|---|---|
| **Repository** | `IRepository<T>` and specific repositories | Hide EF Core from the use cases |
| **Unit of Work** | `IUnitOfWork.SaveChangesAsync` | Save all changes of a use case in one transaction |
| **DTO + mapping** | `Application/DTOs`, `Application/Mappings` | Never expose entities directly; control the JSON shape |
| **Rich domain model** | `Order`, `Product`, `Client` | Rules live in the entity (`private set`, factory methods) |
| **Factory method** | `Order.Create`, `Product.Create`, `Client.Create` | Entities are always created in a valid state |
| **Dependency Injection** | `AddApplication()`, `AddInfrastructure()` | Loose coupling, easy testing |
| **Options pattern** | `JwtSettings` | Strongly typed configuration |
| **Global exception middleware** | `ExceptionHandlingMiddleware` | One place that maps exceptions to HTTP status codes |

### 5.4 Front-end architecture

The Angular application is organised the same way: a **core** (singletons used everywhere), **shared** (reusable UI pieces), **layout**, and **features** (one folder per business area, each lazy-loaded).

- State is held in **signals**; services expose read-only signals and observables.
- Route access is enforced twice: by **guards** (navigation) and by the **`*appHasRole`** directive (what is shown on screen). The API remains the final authority.
- All HTTP concerns (token header, error handling) live in two **interceptors**, so components stay free of that logic.

---

## 6. Back end in detail

### 6.1 Domain

#### Entities

| Entity | Key fields | Notes |
|---|---|---|
| `BaseEntity` | `Id` (Guid), `CreatedAt` | `Id` is generated by the domain, `protected set` |
| `User` | `Email`, `PasswordHash`, `FullName`, `Role` | Role is stored as text |
| `Client` | `LastName`, `FirstNameOrCompany`, `Email`, `Phone`, `Address` (string array), `Orders` | Email is trimmed and lower-cased by `Create` / `Update` |
| `Product` | `Reference`, `ProductName`, `Description`, `UnitPriceHT`, `StockQuantity` | `IncreaseStock` / `DecreaseStock` validate quantities |
| `Order` | `OrderNumber`, `ClientId`, `DateOrder`, `OrderStatus`, `Lines`, `TotalHt`, `TotalTtc` | Totals are recalculated whenever lines change |
| `OrderLine` | `OrderId`, `ProductId`, `Quantity`, `UnitPrice`, `LineTotal` | The product price is **copied** at the time of the order, so later price changes do not alter past orders |

#### Enums and constants

- `OrderStatus`: `DRAFT`, `VALIDATED`, `CANCELLED`
- `UserRole`: `ADMIN`, `USER`, `COMMERCIAL`
- `TaxConstants.VATRATE` = `0.19`

#### Business rules enforced by the domain

| Rule | Where |
|---|---|
| A line can be added only while the order is `DRAFT` | `Order.AddLine` |
| Quantity must be greater than 0; unit price cannot be negative | `Order.AddLine` |
| The quantity ordered cannot exceed the **available stock** (the total across lines of the same product is checked) | `Order.AddLine` |
| Adding a product already in the order **adds to its quantity** instead of creating a second line | `Order.AddLine` |
| Only a `DRAFT` order can be validated, and it must contain at least one line | `Order.Validate` |
| Lines can be cleared only while `DRAFT` | `Order.ClearLines` |
| An order already `CANCELLED` cannot be cancelled again | `Order.Cancel` |
| `TotalHt` = sum of line totals; `TotalTtc` = `TotalHt × 1.19` | `Order.RecalculateTotals` |
| Stock can never become negative ("Stock insufisant") | `Product.DecreaseStock` |
| A product needs a reference and a name; price and stock cannot be negative | `Product.Create` |

Violations throw `BusinessRuleException`, which the API turns into **HTTP 400**.

### 6.2 Application

#### Services (use cases)

| Service | Responsibilities |
|---|---|
| `AuthService` | Verify credentials, issue a JWT, return the current user |
| `ClientService` | CRUD; refuses duplicate emails and deleting a client that has orders |
| `ProductService` | CRUD; refuses duplicate references and deleting a product used in an order |
| `OrderService` | Create, update, list, by-client list, validate, cancel, delete |

#### Order life cycle

```
                 validate (ADMIN)                     cancel (ADMIN)
   DRAFT  ───────────────────────►  VALIDATED  ──────────────────────►  CANCELLED
     │       stock is decreased          │          stock is restored         ▲
     │                                    │                                    │
     └────────────────────────────────────┴─ cancel (ADMIN) ──────────────────┘
        (a draft was never deducted, so cancelling it does not touch the stock)
```

- **Create / update:** the quantities are checked against the current stock (HTTP 400 if exceeded), but the stock itself is **not modified**. It is only decreased on validation.
- **Validate:** each line decreases the product stock. If one product lacks stock the whole operation fails (HTTP 400) and nothing is saved.
- **Cancel:** stock is given back **only if the order had been validated**.
- **Update:** allowed only for drafts; the lines are replaced by the new ones.
- **Order number:** generated as `CMD-yyyyMM-0001`, incrementing within each month.

#### Validators (FluentValidation)

| DTO | Main rules |
|---|---|
| `LoginDto` | Email required and well-formed; password required |
| `CreateClientDto` / `UpdateClientDto` | First name or company required (≤ 150); valid email; phone matches `^\+?[0-9\s]{8,15}$` |
| `CreateProductDto` / `UpdateProductDto` | Reference required (≤ 50); name required (≤ 200); price ≥ 0; initial stock ≥ 0 |
| `CreateOrderDto` / `UpdateOrderDto` | At least one line; each line needs a product and a quantity > 0; the client is required on creation |

Validators are discovered automatically (`AddValidatorsFromAssembly`) and executed by the API's `ValidationFilter`, which returns a `400` with errors grouped by field.

#### Exceptions

| Exception | Layer | HTTP status |
|---|---|---|
| `BusinessRuleException` | Domain | 400 |
| `ArgumentException` | any | 400 |
| `UnauthorizedException` | Application | 401 |
| `ForbiddenException` | Application | 403 |
| `NotFoundException` | Application | 404 |
| `ConflictException` | Application | 409 |
| anything else | — | 500 (generic message, details only in the log) |

### 6.3 Infrastructure

- **`AppDbContext`** exposes `Users`, `Clients`, `Products`, `Orders`, `OrderLines`. It applies all `IEntityTypeConfiguration` classes from the assembly and declares every `Id` as `ValueGeneratedNever`, because identifiers are produced by the domain.
- **Configurations** define table names, key and index constraints, decimal precision `decimal(18,2)`, delete behaviours, and the backing field `_lines` / `_orders` for collections.
- **`Repository<T>`** provides `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `Update`, `Remove`. Specific repositories add queries (`EmailExistsAsync`, `ReferenceExistsAsync`, `GetOrderWithDetailsAsync`, `GetByClientIdAsync`, `GenerateOrderNumberAsync`, ...).
- **`UnitOfWork`** wraps `SaveChangesAsync`.
- **`PasswordHasher`** uses BCrypt (`BCrypt.Net-Next`).
- **`JwtTokenGenerator`** signs tokens with HMAC-SHA256 and includes the claims `sub`, `email`, `jti`, `role`.
- **`DbSeeder`** runs the migrations and creates the default admin when the `Users` table is empty.

### 6.4 API layer

- **Controllers** are thin: they call a service and return the result.
- **`ExceptionHandlingMiddleware`** returns RFC 7807 `application/problem+json` errors.
- **`ValidationFilter`** runs FluentValidation on every action argument.
- **`CurrentUserService`** reads the user id, email and role from the JWT claims.
- **CORS** policy `AllowAngular` allows `http://localhost:4200`.
- **Swagger / OpenAPI** is enabled in every environment (Development and Production) and supports Bearer authentication (the *Authorize* button). The root URL `/` redirects to `/swagger`, and the raw specification is available at `/swagger/v1/swagger.json`.

Pipeline order in `Program.cs`:

```
Exception middleware → migrate & seed → Swagger → HTTPS redirection
→ CORS → Authentication → Authorization → Controllers
```

---

## 7. REST API reference

Base URL: `http://localhost:5013/api`
All endpoints except `POST /auth/login` require the header `Authorization: Bearer <token>`.

### 7.1 Endpoints

| Method | Route | Role | Description |
|---|---|---|---|
| POST | `/auth/login` | public | Log in, returns the JWT |
| GET | `/auth/me` | any | Claims of the current user |
| GET | `/clients` | any | List clients |
| GET | `/clients/{id}` | any | Client detail |
| POST | `/clients` | ADMIN | Create a client |
| PUT | `/clients/{id}` | ADMIN | Update a client |
| DELETE | `/clients/{id}` | ADMIN | Delete a client (409 if it has orders) |
| GET | `/products` | any | List products |
| GET | `/products/{id}` | any | Product detail |
| POST | `/products` | ADMIN | Create a product |
| PUT | `/products/{id}` | ADMIN | Update a product |
| DELETE | `/products/{id}` | ADMIN | Delete a product (409 if used in an order) |
| GET | `/orders` | any | List orders (summary) |
| GET | `/orders/{id}` | any | Order detail with lines |
| GET | `/orders/client/{clientId}` | any | Orders of one client |
| POST | `/orders` | any | Create an order (status `DRAFT`) |
| PUT | `/orders/{id}` | any | Replace the lines of a draft order |
| POST | `/orders/{id}/validate` | ADMIN | Validate and decrease stock |
| POST | `/orders/{id}/cancel` | ADMIN | Cancel (restores stock if it was validated) |
| DELETE | `/orders/{id}` | ADMIN | Delete an order |

### 7.2 Status codes

| Code | Meaning |
|---|---|
| 200 / 201 / 204 | Success, created, success without body |
| 400 | Validation error or business-rule violation |
| 401 | Missing, invalid or expired token; wrong credentials |
| 403 | Authenticated but not allowed |
| 404 | Resource not found |
| 409 | Conflict (duplicate email or reference, resource still in use) |
| 500 | Unexpected server error |

### 7.3 Examples

**Login**

```http
POST /api/auth/login
Content-Type: application/json

{ "email": "Amir@isi.com.tn", "password": "Admin123!" }
```

```json
{ "token": "eyJhbGciOi...", "email": "Amir@isi.com.tn", "fullName": "AmirISI", "role": "ADMIN" }
```

**Create a product**

```json
{ "reference": "PRD-001", "name": "Keyboard", "description": "USB keyboard", "unitPriceHT": 100, "initialStock": 10 }
```

**Create an order**

```json
{
  "clientId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "lines": [ { "productID": "9b2c3a10-0c5e-4d7b-9a40-1f2e3d4c5b6a", "quantity": 2 } ]
}
```

> The line property is spelled `productID` in the request body and `productId` in responses.

**Order detail response**

```json
{
  "id": "…", "orderNumber": "CMD-202611-0001",
  "clientId": "…", "clientName": "Test2 Cli",
  "orderDate": "2026-11-13T03:10:00Z", "orderStatus": "DRAFT",
  "totalHt": 200.00, "totalTTC": 238.0000,
  "lines": [ { "productId": "…", "productName": "Keyboard", "quantity": 2, "unitPrice": 100.00, "lineTotal": 200.00 } ]
}
```

**Error responses**

```json
{ "type": "https://httpstatuses.com/401", "title": "Non authentifié", "status": 401, "detail": "Email ou mdp incorrect" }
```

```json
{ "title": "Erreur de validation des données requises.", "status": 400,
  "errors": { "Email": ["Email requis"], "Password": ["Le mot de passe est requis"] } }
```

---

## 8. Database (SQL Server / SSMS)

### 8.1 Connection

| Setting | Value |
|---|---|
| Server | `localhost\SQLEXPRESS` |
| Database | `Mini_Projet` |
| Authentication | Windows Authentication |
| Connection string | `Server=localhost\SQLEXPRESS;Database=Mini_Projet;Trusted_Connection=True;TrustServerCertificate=True;` |

The connection string is stored in `backend/src/GestionCommerciale.Api/appsettings.json` under `ConnectionStrings:DefaultConnection`.

#### Connecting with SSMS

1. Open **SQL Server Management Studio**.
2. In the *Connect to Server* dialog choose **Server type:** Database Engine, **Server name:** `localhost\SQLEXPRESS`, **Authentication:** Windows Authentication.
3. If SSMS complains about the certificate, tick **Trust server certificate** (Options → Connection Properties).
4. Click **Connect**. Expand **Databases → Mini_Projet → Tables**.

The database appears after the API has been started once (or after `dotnet ef database update`).

### 8.2 Tables

#### `Users`

| Column | Type | Constraints |
|---|---|---|
| `Id` | `uniqueidentifier` | **PK** |
| `Email` | `nvarchar(150)` | NOT NULL, **UNIQUE** |
| `PasswordHash` | `nvarchar(max)` | NOT NULL (BCrypt hash) |
| `FullName` | `nvarchar(max)` | NOT NULL |
| `Role` | `nvarchar(max)` | NOT NULL (`ADMIN`, `USER`, `COMMERCIAL`) |
| `CreatedAt` | `datetime2` | NOT NULL |

#### `Clients`

| Column | Type | Constraints |
|---|---|---|
| `Id` | `uniqueidentifier` | **PK** |
| `LastName` | `nvarchar(max)` | NOT NULL |
| `FirstNameOrCompany` | `nvarchar(max)` | NOT NULL |
| `Phone` | `nvarchar(max)` | NOT NULL |
| `Email` | `nvarchar(150)` | NOT NULL, **UNIQUE** |
| `Address` | `nvarchar(max)` | NOT NULL — address lines joined with `;` |
| `CreatedAt` | `datetime2` | NOT NULL |

#### `Products`

| Column | Type | Constraints |
|---|---|---|
| `Id` | `uniqueidentifier` | **PK** |
| `Reference` | `nvarchar(50)` | NOT NULL, **UNIQUE** |
| `ProductName` | `nvarchar(max)` | NOT NULL |
| `Description` | `nvarchar(max)` | NOT NULL |
| `UnitPriceHT` | `decimal(18,2)` | NOT NULL |
| `StockQuantity` | `int` | NOT NULL |
| `CreatedAt` | `datetime2` | NOT NULL |

#### `Orders`

| Column | Type | Constraints |
|---|---|---|
| `Id` | `uniqueidentifier` | **PK** |
| `OrderNumber` | `nvarchar(max)` | NULL — format `CMD-yyyyMM-0001` |
| `ClientId` | `uniqueidentifier` | NOT NULL, **FK → Clients(Id)**, `ON DELETE RESTRICT` |
| `DateOrder` | `datetime2` | NOT NULL |
| `OrderStatus` | `nvarchar(max)` | NOT NULL (`DRAFT`, `VALIDATED`, `CANCELLED`) |
| `TotalHt` | `decimal(18,2)` | NOT NULL |
| `TotalTtc` | `decimal(18,2)` | NOT NULL |
| `CreatedAt` | `datetime2` | NOT NULL |

#### `OrderLines`

| Column | Type | Constraints |
|---|---|---|
| `Id` | `uniqueidentifier` | **PK** |
| `OrderId` | `uniqueidentifier` | NOT NULL, **FK → Orders(Id)**, `ON DELETE CASCADE` |
| `ProductId` | `uniqueidentifier` | NOT NULL, **FK → Products(Id)**, `ON DELETE RESTRICT` |
| `Quantity` | `int` | NOT NULL |
| `UnitPrice` | `decimal(18,2)` | NOT NULL — price copied from the product when the line is created |
| `LineTotal` | `decimal(18,2)` | NOT NULL |
| `CreatedAt` | `datetime2` | NOT NULL |

#### `__EFMigrationsHistory`

Created by Entity Framework. It records the migrations already applied. Do not edit it by hand.

### 8.3 Relationships

```
 Clients 1 ────────< Orders 1 ────────< OrderLines >──────── 1 Products
          (Restrict)         (Cascade)              (Restrict)
```

- A **client** has many orders. A client with orders **cannot be deleted** (`Restrict`).
- An **order** has many lines. Deleting an order **deletes its lines** (`Cascade`).
- A **product** appears in many order lines. A product used in an order **cannot be deleted** (`Restrict`).

### 8.4 Indexes

| Index | Table | Column(s) | Unique |
|---|---|---|---|
| `IX_Users_Email` | Users | Email | yes |
| `IX_Clients_Email` | Clients | Email | yes |
| `IX_Products_Reference` | Products | Reference | yes |
| `IX_Orders_ClientId` | Orders | ClientId | no |
| `IX_OrderLines_OrderId` | OrderLines | OrderId | no |
| `IX_OrderLines_ProductId` | OrderLines | ProductId | no |

### 8.5 Migrations

Migrations are stored in `backend/src/GestionCommerciale.Infrastructure/Migrations`. They are the source of truth for the schema.

| Task | Command (from the `backend` folder) |
|---|---|
| Create a migration | `dotnet ef migrations add Name --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api` |
| Apply migrations | `dotnet ef database update --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api` |
| Remove the last migration (not applied) | `dotnet ef migrations remove --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api` |
| Generate a SQL script | `dotnet ef migrations script --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api -o schema.sql` |
| Drop the database | `dotnet ef database drop --project src/GestionCommerciale.Infrastructure --startup-project src/GestionCommerciale.Api` |

Whenever an entity or a configuration changes, create a new migration and apply it.

Two migrations exist: `InitialCreate` (all tables) and `AddOrderNumberIndex` (limits `Orders.OrderNumber` to 30 characters and adds a unique index on it).

#### Ready-to-run SQL script

`database/Mini_Projet.sql` contains everything needed to build the database by hand in SSMS: creation of the `Mini_Projet` database, the five tables, keys, indexes and the default administrator account. It can be run several times without error.

To regenerate the schema part after a model change (from the `backend/src` folder):

```powershell
dotnet ef migrations script --idempotent -o schema.sql --project GestionCommerciale.Infrastructure --startup-project GestionCommerciale.Api
```

### 8.6 Useful SQL queries (run them in an SSMS *New Query* window)

```sql
USE Mini_Projet;

-- All users and their roles
SELECT Id, Email, FullName, Role FROM Users;

-- Products with low stock
SELECT Reference, ProductName, StockQuantity
FROM Products
WHERE StockQuantity <= 5
ORDER BY StockQuantity;

-- Orders with their client
SELECT o.OrderNumber, c.FirstNameOrCompany, c.LastName, o.OrderStatus, o.TotalHt, o.TotalTtc, o.DateOrder
FROM Orders o
JOIN Clients c ON c.Id = o.ClientId
ORDER BY o.DateOrder DESC;

-- Lines of one order
SELECT p.Reference, p.ProductName, l.Quantity, l.UnitPrice, l.LineTotal
FROM OrderLines l
JOIN Products p ON p.Id = l.ProductId
JOIN Orders o ON o.Id = l.OrderId
WHERE o.OrderNumber = 'CMD-202611-0001';

-- Revenue of validated orders per client
SELECT c.FirstNameOrCompany, c.LastName, COUNT(*) AS Orders, SUM(o.TotalTtc) AS Revenue
FROM Orders o
JOIN Clients c ON c.Id = o.ClientId
WHERE o.OrderStatus = 'VALIDATED'
GROUP BY c.FirstNameOrCompany, c.LastName
ORDER BY Revenue DESC;

-- Migrations already applied
SELECT * FROM __EFMigrationsHistory;
```

### 8.7 Maintenance

- **Backup:** in SSMS, right-click `Mini_Projet` → *Tasks* → *Back Up…* → choose a `.bak` destination.
- **Reset to a clean database:** drop the database (`dotnet ef database drop`, or right-click → *Delete* in SSMS) and start the API again. It recreates the schema and the default administrator.
- **Add an administrator manually:** hash the password through the API code path rather than inserting plain text; passwords are always stored as BCrypt hashes.

---

## 9. Front end in detail

### 9.1 Technology choices

| Choice | Reason |
|---|---|
| Standalone components | No NgModules; each component declares its own imports |
| Signals and `computed` | Simple, fast reactive state without manual subscriptions in templates |
| Zoneless change detection | Less overhead; updates are driven by signals |
| Reactive forms (typed) | Strong typing and explicit validation |
| Lazy-loaded feature routes | Each feature is downloaded only when visited |
| Tailwind CSS | Consistent design through utility classes plus a few shared component classes |

### 9.2 Routing

| Path | Component | Access |
|---|---|---|
| `/login` | Login | guests only (`guestGuard`) |
| `/` | redirects to `/orders` | authenticated |
| `/orders` | Order list | authenticated |
| `/orders/new` | Order form | authenticated |
| `/orders/:id` | Order detail | authenticated |
| `/orders/:id/edit` | Order form (draft only) | authenticated |
| `/clients` · `/clients/:id` | List · detail | authenticated |
| `/clients/new` · `/clients/:id/edit` | Form | **ADMIN** (`roleGuard`) |
| `/products` · `/products/:id` | List · detail | authenticated |
| `/products/new` · `/products/:id/edit` | Form | **ADMIN** (`roleGuard`) |
| `/forbidden` | 403 page | authenticated |
| anything else | 404 page | authenticated |

Every route inside the main layout is protected by `authGuard`. Titles are set per route.

### 9.3 Core

#### Constants
- `app.constants.ts`: application name, `VAT_RATE` (0.19), currency code (`TND`), storage keys, notification duration, low-stock threshold.
- `roles.ts`: `Role` enum (`ADMIN`, `USER`, `COMMERCIAL`), French labels, and `toRole()` which accepts either the enum key or its value.

#### Models
`auth.model.ts`, `client.model.ts`, `product.model.ts`, `order.model.ts`, `api-error.model.ts` mirror the API's JSON contract exactly (for instance orders expose `totalHt` and `totalTTC`).

#### Services

| Service | Role |
|---|---|
| `AuthService` | `login`, `logout`, `currentUser` signal, `isAuthenticated`, `isAdmin`, `roleLabel`, `hasRole()`, `isSessionValid()` |
| `TokenStorageService` | Saves and reads the token and user in `localStorage`, decodes the JWT, checks its expiry |
| `ClientService`, `ProductService`, `OrderService` | Typed wrappers around the REST endpoints (`OrderService` also has `validate`, `cancel`, `getByClient`) |
| `NotificationService` | Toast messages (success, error, warning, info) with automatic dismissal |

#### Interceptors

| Interceptor | Behaviour |
|---|---|
| `authInterceptor` | Adds `Authorization: Bearer <token>` to API requests when a valid token exists |
| `errorInterceptor` | Normalises every failure into an `ApiError`. **401** → warning and redirect to login; **403** → "Accès refusé" toast; **0** → "server unreachable"; **5xx** → generic error; field errors are passed to the form |

#### Guards

| Guard | Behaviour |
|---|---|
| `authGuard` | Redirects to `/login?returnUrl=…` when the session is missing or expired |
| `guestGuard` | Redirects logged-in users away from `/login` |
| `roleGuard` | Reads `route.data.roles`; redirects to `/forbidden` when the role does not match |

#### Utilities
- `price.utils.ts`: rounding and VAT calculations.
- `form.utils.ts`: copies server-side validation errors (for example `Lines[0].ProductID`) onto the matching form controls.

### 9.4 Authentication flow

```
Login form ──► AuthService.login ──► POST /api/auth/login
                                         │
                      token + user saved in localStorage, currentUser signal updated
                                         │
                       navigate to returnUrl or /orders
                                         │
 every later request ──► authInterceptor adds the Bearer token
 token expired / 401 ──► session cleared, redirect to /login
```

- The role comes from the login response and is also present in the token.
- `returnUrl` is accepted only when it is a local path, which prevents open redirects.

### 9.5 Role-based UI

| Element | Visible to |
|---|---|
| New / Edit / Delete buttons for clients and products | ADMIN |
| Validate / Cancel / Delete buttons on an order | ADMIN |
| Edit button on a draft order | any authenticated user |
| Navbar user name and role badge | all |

`*appHasRole="'Admin'"` is a structural directive that adds or removes the element reactively when the user changes. It can also take an array of roles.

### 9.6 Shared components

| Component | Purpose |
|---|---|
| `notification-toast` | Displays toast messages |
| `confirm-dialog` | Reusable confirmation modal (variants *danger*, *warning*, *primary*) |
| `loading-spinner` | Loading indicator |
| `field-error` | Shows the first validation message of a form control (French messages) |
| `page-header` | Title, subtitle, back link and action slot |
| `status-badge` | Order status pill: *Brouillon* (amber), *Validée* (green), *Annulée* (red) |
| `forbidden`, `not-found` | 403 and 404 pages |
| `order-status` pipe | Translates a status code into its French label |
| `has-role` directive | Role-based rendering |

### 9.7 Screens

- **Login:** validated form, show/hide password, error banner.
- **Orders list:** status filter tabs with counters, search by number or client, revenue summary cards.
- **Order form:** client selector, dynamic lines, live totals (HT, VAT, TTC), a warning and a disabled save button when a quantity exceeds stock, a product cannot be chosen twice.
- **Order detail:** lines table, totals, client link, and admin actions with confirmation dialogs.
- **Clients:** searchable list, form with a dynamic address list, detail page with the client's orders and validated revenue.
- **Products:** searchable list with stock badges and a low-stock banner, form with a live price preview, detail page with VAT breakdown. After creation, stock changes only through order validation and cancellation.

### 9.8 Development proxy

`proxy.conf.json` forwards `/api` to `http://localhost:5013`. The front end therefore calls the relative URL `/api/...` (`environment.apiUrl`), and the browser sees a single origin.

---

## 10. Security

| Topic | Implementation |
|---|---|
| **Authentication** | JWT Bearer. Tokens are signed with HMAC-SHA256 and valid for `ExpiryMinutes` (120). Issuer, audience, lifetime and signature are validated. |
| **Passwords** | Hashed with BCrypt; never stored or logged in clear text. |
| **Authorization** | Role claim checked by `[Authorize(Roles = "ADMIN")]` on sensitive endpoints, mirrored in the UI by `roleGuard` and `*appHasRole`. |
| **Error output** | Unexpected errors return a generic message; details are written to the server log only. |
| **CORS** | Only `http://localhost:4200` is allowed (in development the proxy makes it unnecessary). |
| **Input validation** | FluentValidation on the server; typed form validation on the client. |
| **Open redirect** | The login `returnUrl` must be a local path. |

**Before deploying for real:**

1. Move the JWT secret out of `appsettings.json` (environment variable, user-secrets or a secret manager). It must contain at least 32 characters.
2. Change the default administrator password.
3. Serve both the API and the front end over HTTPS.
4. Update the CORS origin to the production front-end address.

---

## 11. Configuration reference

`backend/src/GestionCommerciale.Api/appsettings.json`

| Key | Meaning |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `JwtSettings:Secret` | HMAC signing key (≥ 32 characters) |
| `JwtSettings:Issuer` | Token issuer (`GestionCommercialeAPI`) |
| `JwtSettings:Audience` | Token audience (`GestionCommercialeFrontEnd`) |
| `JwtSettings:ExpiryMinutes` | Token lifetime in minutes |

Ports (`Properties/launchSettings.json`): HTTP `5013`, HTTPS `7210`.
Front end: `Frontend/proxy.conf.json` (development target) and `Frontend/src/environments/*.ts` (`apiUrl`).

---

## 12. Testing the application

### 12.1 With Swagger

1. Start the API and open http://localhost:5013/swagger.
2. Call `POST /api/auth/login` and copy the `token`.
3. Click **Authorize** and paste `Bearer <token>`.
4. Try the endpoints.

### 12.2 Suggested end-to-end scenario (front end)

1. Log in as the administrator.
2. Create a **product** (reference `PRD-001`, price 100, stock 10).
3. Create a **client**.
4. Create an **order** for 4 units → status *Brouillon*; stock is still 10.
5. Open the order and click **Valider** → stock becomes 6, status *Validée*.
6. Click **Annuler** → stock returns to 10, status *Annulée*.
7. Try to delete the client or the product while an order references them → the application refuses with a clear message.

### 12.3 Expected behaviours worth checking

| Action | Expected result |
|---|---|
| Wrong password | 401, message under the form |
| Create a client with an existing email | 409 |
| Create a product with an existing reference | 409 |
| Order line with quantity 0 or negative | 400 with field message |
| Order line quantity above the available stock | 400 "Stock insuffisant pour … : N disponible(s), M demandé(s)." |
| Order totals | `totalTTC` = `totalHt` × 1.19 |
| Validate an order with insufficient stock | 400 "Stock insufisant", stock unchanged |
| Validate an order twice | 400 |
| Edit a validated order | refused |
| Call an ADMIN endpoint with a non-admin token | 403 |

### 12.4 Automated tests

The solution contains two test projects (`GestionCommerciale.UnitTests`, `GestionCommerciale.IntegrationTests`, xUnit). They currently hold only the template test; business-rule tests for `Order`, `Product` and the services are a natural next step. The front end uses Vitest through `npm test`.

---

## 13. Troubleshooting

| Symptom | Cause and fix |
|---|---|
| `dotnet ef` not found | Install the tool: `dotnet tool install --global dotnet-ef --version 8.*` |
| *An Application Control policy has blocked this file* | Windows **Smart App Control** blocks freshly compiled, unsigned DLLs. Turn it off in Windows Security → App & browser control → Smart App Control settings (this cannot be turned back on without reinstalling Windows), or develop on a machine without it. |
| Cannot connect to SQL Server | Check that the `MSSQL$SQLEXPRESS` service is running and that the server name is `localhost\SQLEXPRESS`. |
| *The certificate chain was issued by an authority that is not trusted* | Keep `TrustServerCertificate=True` in the connection string (and tick *Trust server certificate* in SSMS). |
| *Database already exists* / schema mismatch | Drop the database and restart the API, or create a new migration for the change. |
| Build fails: file is locked by `GestionCommerciale.Api` | The API is still running. Stop it (Ctrl+C) before rebuilding. |
| Front end shows "Impossible de joindre le serveur" | The API is not running on port 5013, or the proxy file was not picked up; restart `npm start`. |
| Angular refuses to install / run | Use Node.js 24 or later with Angular 21. |
| Login works but every call returns 401 | The token expired or the system clock is wrong; log in again. |
| 401 on Swagger calls | Use the **Authorize** button and include the word `Bearer` before the token. |
| Swagger page not found | Open `http://localhost:5013/swagger` (the port is set in `launchSettings.json`); make sure the API is running. |
| Port already in use | Change the port in `launchSettings.json` and in `Frontend/proxy.conf.json`. |

---

## 14. Known limitations and possible improvements

**Known limitations**

- Only one user (the seeded administrator) exists. There is no registration or user-management screen, so the `USER` and `COMMERCIAL` roles can only be tested by adding users to the database.
- The stock check at order creation is a snapshot: stock can still run out between the creation of a draft and its validation, which is why validation checks it again.
- The API allows deleting a validated order, and doing so does not restore stock. The user interface hides the button for validated orders.
- Two orders created at exactly the same moment may be given the same number; the numbering is not concurrency-safe on its own.
- The `UpdateOrderDto` carries a `clientId` that is ignored: the client of an existing order cannot be changed.
- Passwords of the seeded administrator and the JWT secret are present in the configuration file for development convenience.

**Possible improvements**

- User management (create users, change roles and passwords).
- Refresh tokens and token revocation.
- Pagination and server-side search for the lists.
- PDF export of orders and invoices.
- A dashboard with sales statistics and low-stock alerts.
- Unit tests for the domain and the services, integration tests for the API.
- Docker Compose for the API, front end and SQL Server.
- Continuous-integration pipeline running build and tests.
