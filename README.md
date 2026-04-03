# PrintFlow — Custom Print Shop Order Management System

Web-based order management system for a custom print shop (signage, business cards, banners, branded merchandise, stickers). Customers configure, order, and pay for custom printed products. Administrators manage orders through production stages.

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8, C# |
| Database | PostgreSQL 16, Entity Framework Core |
| Auth | ASP.NET Identity + JWT + Google OAuth2 |
| Payments | Stripe (Sandbox) |
| Message Queue | RabbitMQ + MassTransit |
| PDF Generation | QuestPDF |
| Email | SMTP (Mailtrap for testing) |
| Validation | FluentValidation |
| Mapping | AutoMapper |
| Containerization | Docker + Docker Compose |
| Architecture | Clean Architecture |

## Project Structure

```
PrintFlow-backend/
├── src/
│   ├── core/
│   │   ├── PrintFlow.Domain/            # Entities, enums, base classes
│   │   └── PrintFlow.Application/       # DTOs, services, interfaces, validators,
│   │                                    # mappings, exceptions, message contracts
│   ├── infrastructure/
│   │   ├── PrintFlow.Persistence/       # DbContext, EF configs, migrations, seed data
│   │   └── PrintFlow.Infrastructure/    # Repository implementations, Stripe, email,
│   │                                    # invoice services, MassTransit config
│   └── presentation/
│       └── PrintFlow.API/               # Controllers, middleware, extensions
├── Worker/
│   └── PrintFlow.Worker/               # RabbitMQ consumers for async tasks
├── Tests/
│   └── PrintFlow.UnitTests/            # Unit and integration tests
├── Dockerfile                          # API container
├── Dockerfile.worker                   # Worker container
├── docker-compose.yml                  # Full stack orchestration
└── README.md
```

## Quick Start (Docker)

Run the entire stack with one command:

```bash
git clone https://github.com/ujera/PrintFlow-backend.git
cd PrintFlow-backend
docker-compose up --build
```

This starts 4 containers:
- **PostgreSQL** — database on port 5432
- **RabbitMQ** — message broker on port 5672 (management UI on 15672)
- **API** — REST API on port 5000
- **Worker** — background task processor

API Swagger UI: `http://localhost:5000/swagger`
RabbitMQ Dashboard: `http://localhost:15672` (guest/guest)

## Manual Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL 16](https://www.postgresql.org/download/) (or Docker)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (or Docker)

### 1. Start PostgreSQL and RabbitMQ

```bash
docker run -d --name printflow-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=printflow_db -p 5432:5432 postgres:16
docker run -d --name printflow-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 2. Configure connection string

Edit `src/presentation/PrintFlow.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=printflow_db;Username=postgres;Password=postgres"
  }
}
```

### 3. Configure external services (optional)

Use .NET User Secrets to store API keys securely:

```bash
cd src/presentation/PrintFlow.API
dotnet user-secrets init
dotnet user-secrets set "Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_key"
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_your_key"
dotnet user-secrets set "Stripe:WebhookSecret" "whsec_your_secret"
```

### 4. Run migrations

```bash
dotnet ef migrations add Init --project src/infrastructure/PrintFlow.Persistence --startup-project src/presentation/PrintFlow.API
```

### 5. Run the application

Terminal 1 — API:
```bash
dotnet run --project src/presentation/PrintFlow.API
```

Terminal 2 — Worker:
```bash
dotnet run --project Worker/PrintFlow.Worker
```

The API auto-migrates the database and seeds initial data on startup.

## Default Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@printflow.io | Admin123! |

Customer accounts are created via Google OAuth or the test registration endpoint.

## Testing the API

1. Open Swagger UI at `https://localhost:7024/swagger`
2. Call `POST /api/auth/login` with admin credentials
3. Copy the `accessToken` from the response
4. Click the **Authorize** button, paste the token
5. All protected endpoints are now accessible

### Test Customer Flow

1. Register: `POST /api/auth/register-test` with email, name, password
2. Authorize with the customer token
3. Add to cart: `POST /api/cart/items`
4. Create order: `POST /api/orders`
5. Watch the Worker terminal for email processing logs

## API Endpoints

### Public
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/categories | List all active categories |
| GET | /api/categories/:id/products | Products in category |
| GET | /api/products/:id | Product details with options and pricing |

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/auth/login | Admin login (email + password) |
| POST | /api/auth/google | Customer login (Google OAuth2) |
| POST | /api/auth/refresh | Refresh access token |
| POST | /api/auth/register-test | Register test customer (dev only) |
| GET | /api/auth/me | Current user profile |

### Customer (requires Customer role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/cart | Get current cart |
| POST | /api/cart/items | Add item to cart |
| PUT | /api/cart/items/:id | Update cart item |
| DELETE | /api/cart/items/:id | Remove from cart |
| POST | /api/orders | Create order from cart |
| GET | /api/orders | List my orders |
| GET | /api/orders/:id | Order details |
| POST | /api/orders/:id/pay | Initiate Stripe card payment |
| GET | /api/orders/:id/invoice | Download invoice PDF |

### Notifications (requires authentication)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/notifications | List my notifications |
| GET | /api/notifications/unread-count | Unread notification count |
| PUT | /api/notifications/:id/read | Mark notification as read |

### Admin (requires Admin role)
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | /api/admin/dashboard | Dashboard summary cards |
| POST | /api/admin/categories | Create category |
| GET | /api/admin/categories/:id | Get category |
| PUT | /api/admin/categories/:id | Update category |
| POST | /api/admin/products | Create product |
| GET | /api/admin/products/:id | Get product |
| PUT | /api/admin/products/:id | Update product |
| DELETE | /api/admin/products/:id | Deactivate product |
| GET | /api/admin/orders | All orders (filterable, paginated) |
| GET | /api/admin/orders/:id | Order detail |
| PUT | /api/admin/orders/:id/status | Update order status |
| POST | /api/admin/orders/:id/approve-payment | Approve bank transfer |
| POST | /api/admin/orders/:id/cancel | Cancel order |
| POST | /api/admin/orders/:id/refund | Refund order via Stripe |

### Webhook
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | /api/webhooks/stripe | Stripe payment webhook (hidden from Swagger) |

## Order Status Flow

```
CREATED → AWAITING_PAYMENT (bank transfer selected)
CREATED → PAID (card payment succeeds)
CREATED → PAYMENT_FAILED (card payment fails)
PAYMENT_FAILED → PAID (retry succeeds)
AWAITING_PAYMENT → PAID (admin approves)
PAID → IN_PRODUCTION → QUALITY_CHECK → READY_FOR_PICKUP → COMPLETED
Any (except COMPLETED) → CANCELLED
```

## Asynchronous Tasks (Worker)

The Worker project processes these tasks via RabbitMQ:

| Event | Consumer | Action |
|-------|----------|--------|
| OrderCreatedEvent | OrderCreatedConsumer | Sends order confirmation email |
| OrderStatusChangedEvent | OrderStatusChangedConsumer | Sends status update notification |
| PaymentSucceededEvent | PaymentSucceededConsumer | Sends payment confirmation email |
| PaymentFailedEvent | PaymentFailedConsumer | Sends payment retry reminder |
| OrderCompletedEvent | OrderCompletedConsumer | Generates invoice PDF and emails it |

## Database Entities

- **User** — customer and admin accounts (ASP.NET Identity)
- **ProductCategory** — product groupings (Business Cards, Banners, etc.)
- **Product** — printable product types with base pricing
- **ProductOption** — configurable options (material, size, finishing)
- **PricingTier** — quantity-based pricing
- **Order** — customer orders with status tracking
- **OrderItem** — line items with configuration and uploaded files
- **Payment** — payment transaction records (Stripe + bank transfer)
- **OrderStatusHistory** — audit trail for all status changes
- **Notification** — email and in-app notification log
- **CartItem** — shopping cart items

## Sample API Calls

**Create a product (admin):**
```json
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

**Add item to cart (customer):**
```json
POST /api/cart/items
{
    "productId": "c1000000-0000-0000-0000-000000000001",
    "quantity": 250,
    "configJson": "{\"material\":\"Premium 400gsm\",\"finishing\":\"Matte Lamination\"}"
}
```

**Create order (customer):**
```json
POST /api/orders
{
    "paymentMethod": "Card",
    "notes": "Rush order please"
}
```

**Update order status (admin):**
```json
PUT /api/admin/orders/{id}/status
{
    "status": "InProduction",
    "notes": "Started printing"
}
```