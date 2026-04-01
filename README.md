# PrintFlow — Custom Print Shop Order Management System

Web-based order management system for a custom print shop. Customers configure, order, and pay for printed products. Admins manage orders through production stages.

## Tech Stack

- **Backend:** ASP.NET Core 8, Entity Framework Core, PostgreSQL
- **Auth:** ASP.NET Identity + JWT
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure → API)

## Quick Start (Docker)

The fastest way to run everything:

```bash
git clone https://github.com/your-repo/PrintFlow-backend.git
cd PrintFlow-backend
docker-compose up --build
```

API will be available at: `http://localhost:5000/swagger`

## Manual Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16](https://www.postgresql.org/download/) (or Docker)

### 1. Start PostgreSQL

**Option A — Docker (recommended):**

```bash
docker run -d --name printflow-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=printflow_db -p 5432:5432 postgres:16
```

**Option B — Local install:**

Install PostgreSQL and create a database named `printflow_db`.

### 2. Configure connection string

Edit `src/presentation/PrintFlow.API/appsettings.json`:

```json
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=printflow_db;Username=postgres;Password=postgres"
}
```

### 3. Run migrations

```bash
dotnet ef migrations add Init --project src/infrastructure/PrintFlow.Persistence --startup-project src/presentation/PrintFlow.API
```

### 4. Run the application

```bash
dotnet run --project src/presentation/PrintFlow.API
```

The app auto-migrates the database and seeds initial data on startup.

Open Swagger UI at: `https://localhost:{port}/swagger`

## Default Accounts

| Role  | Email               | Password   |
|-------|---------------------|------------|
| Admin | admin@printflow.io  | Admin123!  |

Customer accounts are created via Google OAuth.

## Testing the API

1. Open Swagger UI
2. Call `POST /api/auth/login` with admin credentials above
3. Copy the `accessToken` from the response
4. Click **Authorize** button in Swagger, paste the token
5. All protected endpoints are now accessible

### Sample API calls

**Get all categories (public):**
```
GET /api/categories
```

**Create a product (admin):**
```
POST /api/admin/products
{
    "categoryId": "b1000000-0000-0000-0000-000000000001",
    "name": "Premium Business Card",
    "description": "Thick 400gsm premium cards",
    "basePrice": 0.25,
    "options": [
        { "optionType": "Material", "name": "400gsm Matte", "priceModifier": 0.0 },
        { "optionType": "Finishing", "name": "Spot UV", "priceModifier": 0.08 }
    ],
    "pricingTiers": [
        { "minQuantity": 100, "maxQuantity": 499, "unitPrice": 0.25 },
        { "minQuantity": 500, "maxQuantity": 2000, "unitPrice": 0.18 }
    ]
}
```

## Project Structure

```
src/
  core/
    PrintFlow.Domain/            # Entities, enums, base classes
    PrintFlow.Application/       # DTOs, interfaces, services, validators, mappings
  infrastructure/
    PrintFlow.Persistence/       # DbContext, EF configurations, migrations, seed data
    PrintFlow.Infrastructure/    # Repository implementations, external service integrations
  presentation/
    PrintFlow.API/               # Controllers, middleware, extensions, Program.cs
```

## API Endpoints

### Public
- `GET  /api/categories` — List all categories
- `GET  /api/categories/:id/products` — Products in category
- `GET  /api/products/:id` — Product details with options and pricing

### Auth
- `POST /api/auth/login` — Admin login (email + password)
- `POST /api/auth/google` — Customer login (Google OAuth)
- `POST /api/auth/refresh` — Refresh access token
- `GET  /api/auth/me` — Current user profile

### Customer (requires authentication)
- `GET    /api/cart` — Get cart
- `POST   /api/cart/items` — Add to cart
- `PUT    /api/cart/items/:id` — Update cart item
- `DELETE /api/cart/items/:id` — Remove from cart
- `POST   /api/orders` — Create order from cart
- `GET    /api/orders` — My orders
- `GET    /api/orders/:id` — Order details
- `GET    /api/notifications` — My notifications
- `PUT    /api/notifications/:id/read` — Mark as read

### Admin (requires Admin role)
- `GET  /api/admin/dashboard` — Dashboard summary
- `POST /api/admin/categories` — Create category
- `PUT  /api/admin/categories/:id` — Update category
- `POST /api/admin/products` — Create product
- `PUT  /api/admin/products/:id` — Update product
- `DELETE /api/admin/products/:id` — Deactivate product
- `GET  /api/admin/orders` — All orders (filterable, paginated)
- `GET  /api/admin/orders/:id` — Order detail
- `PUT  /api/admin/orders/:id/status` — Update order status
- `POST /api/admin/orders/:id/approve-payment` — Approve offline payment
- `POST /api/admin/orders/:id/cancel` — Cancel order
