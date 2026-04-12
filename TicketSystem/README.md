# 🎫 TicketAI — AI-Powered Customer Support System

<div align="center">

![Angular](https://img.shields.io/badge/Angular-21.x-DD0031?style=for-the-badge&logo=angular&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![MongoDB](https://img.shields.io/badge/MongoDB-Atlas-47A248?style=for-the-badge&logo=mongodb&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white)
![AI](https://img.shields.io/badge/AI-GPT--4o--mini-412991?style=for-the-badge&logo=openai&logoColor=white)
![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)

**A full-stack AI-powered customer support ticket management system with automatic ticket classification, smart agent routing, and threaded conversations.**

[Features](#-features) • [Architecture](#-architecture) • [Tech Stack](#-technology-stack) • [Setup](#-setup--installation) • [API Docs](#-api-documentation) • [Testing](#-testing)

**[🌐 Live Demo](https://ticketai.netlify.app)** 

</div>

---

## 📖 Project Overview

TicketAI is a production-ready customer support platform that leverages AI to eliminate manual ticket triage. When a customer submits a support ticket, GPT-4o-mini automatically classifies its **category** (e.g., Authentication Issue, Billing Issue) and **priority** (Low / Medium / High). A smart routing engine then assigns the ticket to the most suitable available agent based on their specialisation and current workload. **Assisting** an agent in providing responses from a knowledge base.

### The Problem
Support teams waste time manually reading, categorising, and routing tickets. Agents with different specialisations receive tickets outside their expertise, slowing resolution times and frustrating customers.

### The Solution
TicketAI automates the entire intake workflow:
1. Customer submits a ticket with a description
2. AI classifies category & priority within seconds
3. Smart router picks the least-loaded specialist agent
4. Agent receives the ticket with an AI-drafted reply suggestion sourced from a knowledge base
5. All parties communicate through a threaded conversation interface

---

## 🏗️ Architecture

```
<img width="1440" height="1228" alt="image" src="https://github.com/user-attachments/assets/9a45d022-14fa-41e8-8757-99f1f293a940" />

```

### Component Breakdown

| Component | Technology | Responsibility |
|-----------|-----------|----------------|
| **Frontend SPA** | Angular 21 | Reactive UI, role-based views, JWT management |
| **Backend API** | .NET 10 Web API | Business logic, auth, REST endpoints |
| **Database** | MongoDB Atlas | Persistent storage for users, tickets, knowledge |
| **AI Classification** | GitHub Models (GPT-4o-mini) | Auto-classify ticket category + priority |
| **AI Reply Assist** | GitHub Models (GPT-4o-mini) | Draft agent replies from knowledge base |
| **Smart Router** | Custom .NET service | Category-aware agent assignment |
| **Auth** | JWT + BCrypt | Stateless authentication, secure password storage |

---

## ✨ Features

### 👥 User Management System (RBAC)

Three roles with strictly enforced permissions:

| Role | Can Do |
|------|--------|
| **Customer** | Register/Login · Create tickets · View own tickets · Reply in thread |
| **Agent** | View assigned tickets · Update status · Reply in thread · Use AI reply-assist |
| **Admin** | Manage all users (CRUD) · View all tickets · Assign tickets to agents · Post in any thread |

- Soft deactivation (users cannot login but data is preserved)
- Permanent deletion with self-delete protection
- Role change by Admin at any time

### 🎫 Ticket Management

- **Create** — Customer submits title + description
- **Auto-classify** — AI assigns category and priority asynchronously
- **Smart-assign** — Least-loaded specialist agent receives the ticket
- **Update status** — Agent moves Open → In Progress → Resolved
- **Threaded conversations** — Reply-to-message with full author/role attribution
- **Agent notes** — Internal legacy notes field (pre-threading support)
- **Live refresh** — Background polling every 12 seconds with instant bump on mutations

### 🤖 AI Integration

- **Ticket Classification** — GPT-4o-mini reads the description and outputs a `{category, priority}` JSON
- **Smart Routing** — Specialist agents (with `HandledCategories`) are preferred; generalist agents are a fallback; ties broken by fewest open tickets
- **Reply Assist** — Agent clicks "Suggested Reply", backend fetches relevant knowledge base articles, GPT drafts a professional response
- **Knowledge Base** — 9 seeded articles covering auth, billing, technical, performance, notifications, and privacy topics

### 🎨 UI / UX

- Fully responsive design (mobile → desktop)
- Light theme with consistent design tokens (CSS variables)
- Demo credentials quick-fill on login page
- Deactivated account popup with admin contact info
- Animated transitions and hover effects
- Font Awesome 7 icons throughout
- Sora (display) + DM Sans (body) Google Fonts

---

## 🛠️ Technology Stack

### Frontend
| Technology | Version | Purpose |
|-----------|---------|---------|
| Angular | 21.2.x | SPA framework |
| Angular SSR | 21.2.x | Server-side rendering |
| RxJS | 7.8.x | Reactive state |
| TypeScript | 5.9.x | Type safety |
| SCSS | — | Component styling |
| Font Awesome Free | 7.2.0 | Icons |
| Sora / DM Sans | — | Typography |

### Backend
| Technology | Version | Purpose |
|-----------|---------|---------|
| .NET | 10.0 | Web API runtime |
| ASP.NET Core | 10.0 | REST framework |
| MongoDB.Driver | 3.7.1 | Database ORM |
| BCrypt.Net-Next | 4.1.0 | Password hashing |
| Microsoft.IdentityModel.Tokens | 8.x | JWT validation |
| Azure.AI.OpenAI | 2.1.0 | GitHub Models client |
| Swashbuckle | 10.x | Swagger/OpenAPI |

### Infrastructure
| Service | Purpose |
|---------|---------|
| MongoDB Atlas (Free Tier) | Cloud database |
| GitHub Models | GPT-4o-mini API endpoint |
| MonsterASP.NET | Deployment host |
| Netlify.App | Deployment host |

---

## 📁 Folder Structure

```
TicketSystem/
├── README.md
│
├── TicketSystem.API/
│   └── TicketSystem.API/
│       ├── TicketSystem.API.csproj
│       ├── TicketSystem.API.slnx
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── dotnet-tools.json
│       ├── Program.cs
│       │
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── AdminController.cs
│       │   └── Ticketcontroller.cs
│       │
│       ├── Data/
│       │   └── DataSeeder.cs
│       │
│       ├── DTOs/
│       │   ├── AiDTOs.cs
│       │   ├── AuthDTOs.cs
│       │   ├── TicketDTOs.cs
│       │   └── UserDTOs.cs
│       │
│       ├── Middleware/
│       │   └── ExceptionMiddleware.cs
│       │
│       ├── Models/
│       │   ├── Enums.cs
│       │   ├── KnowledgeArticle.cs
│       │   ├── Ticket.cs
│       │   ├── TicketMessage.cs
│       │   └── User.cs
│       │
│       ├── Properties/
│       │   └── launchSettings.json
│       │
│       ├── Services/
│       │   ├── AdminService.cs
│       │   ├── AiService.cs
│       │   ├── AuthService.cs
│       │   ├── KnowledgeBaseService.cs
│       │   ├── SmartRoutingService.cs
│       │   └── TicketService.cs
│       │
│       └── Settings/
│           └── Mongodbsettings.cs
│
└── ticket-system/                        # Angular Frontend
    ├── angular.json
    ├── package.json
    ├── package-lock.json
    ├── tsconfig.json
    ├── tsconfig.app.json
    ├── tsconfig.spec.json
    ├── .editorconfig
    ├── .gitignore
    ├── .prettierrc
    │
    ├── .vscode/
    │   ├── extensions.json
    │   ├── launch.json
    │   ├── mcp.json
    │   └── tasks.json
    │
    ├── public/
    │   ├── _redirects
    │   └── web.config
    │
    └── src/
        ├── index.html
        ├── main.ts
        ├── main.server.ts
        ├── server.ts
        ├── styles.scss
        │
        ├── app/
        │   ├── app.component.html
        │   ├── app.component.scss
        │   ├── app.component.ts
        │   ├── app.module.ts
        │   ├── app.module.server.ts
        │   ├── app.routes.server.ts
        │   ├── app-routing.module.ts
        │   ├── app.spec.ts
        │   │
        │   ├── core/
        │   │   ├── guards/
        │   │   │   ├── auth.guard.ts
        │   │   │   └── role.guard.ts
        │   │   ├── interceptors/
        │   │   │   └── jwt.interceptor.ts
        │   │   ├── models/
        │   │   │   └── models.ts
        │   │   ├── services/
        │   │   │   ├── admin.service.ts
        │   │   │   ├── auth.service.ts
        │   │   │   ├── live-refresh.service.ts
        │   │   │   └── ticket.service.ts
        │   │   └── utils/
        │   │       └── http-error.util.ts
        │   │
        │   ├── features/
        │   │   ├── admin/
        │   │   │   └── dashboard/
        │   │   │       ├── admin-dashboard.component.html
        │   │   │       ├── admin-dashboard.component.scss
        │   │   │       └── admin-dashboard.component.ts
        │   │   ├── agent/
        │   │   │   └── dashboard/
        │   │   │       ├── agent-dashboard.component.html
        │   │   │       ├── agent-dashboard.component.scss
        │   │   │       └── agent-dashboard.component.ts
        │   │   ├── auth/
        │   │   │   ├── login/
        │   │   │   │   ├── login.component.html
        │   │   │   │   ├── login.component.scss
        │   │   │   │   └── login.component.ts
        │   │   │   └── register/
        │   │   │       ├── register.component.html
        │   │   │       ├── register.component.scss
        │   │   │       └── register.component.ts
        │   │   └── customer/
        │   │       ├── create-ticket/
        │   │       │   ├── create-ticket.component.html
        │   │       │   ├── create-ticket.component.scss
        │   │       │   └── create-ticket.component.ts
        │   │       └── dashboard/
        │   │           ├── customer-dashboard.component.html
        │   │           ├── customer-dashboard.component.scss
        │   │           └── customer-dashboard.component.ts
        │   │
        │   └── shared/
        │       └── components/
        │           └── navbar/
        │               ├── navbar.component.html
        │               ├── navbar.component.scss
        │               └── navbar.component.ts
        │
        └── environments/
            ├── environment.ts
            └── environment.prod.ts
```

---

## ⚙️ Setup & Installation

### Prerequisites

| Tool | Version | Download |
|------|---------|----------|
| Node.js | ≥ 20.19 | [nodejs.org](https://nodejs.org) |
| npm | ≥ 8 | Bundled with Node.js |
| .NET SDK | 10.0 | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| MongoDB Atlas Account | Free | [cloud.mongodb.com](https://cloud.mongodb.com) |
| GitHub Account (for Models API key) | — | [github.com](https://github.com) |

---

### 2️⃣ MongoDB Atlas Setup

1. Sign in to [MongoDB Atlas](https://cloud.mongodb.com)
2. Create a **free M0 cluster**
3. Under **Database Access** → Add a database user with read/write permissions
4. Under **Network Access** → Add IP address `0.0.0.0/0` (or your specific IP)
5. Click **Connect** → **Drivers** → copy the connection string:
   ```
   mongodb+srv://<username>:<password>@cluster0.xxxxx.mongodb.net/
   ```

---

### 3️⃣ GitHub Models API Key

1. Go to [github.com/marketplace/models](https://github.com/marketplace/models)
2. Navigate to **GPT-4o mini** → click **Get API Key**
3. Generate a personal access token with the **Models** scope
4. Copy your token (starts with `ghp_...` or `github_pat_...`)

---

### 4️⃣ Backend Setup

```bash
cd TicketSystem/TicketSystem.API/TicketSystem.API
```

Open `appsettings.json` and fill in your credentials:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://<user>:<pass>@cluster0.xxxxx.mongodb.net/",
    "DatabaseName": "ticketdb",
    "UsersCollection": "users",
    "TicketsCollection": "tickets",
    "KnowledgeBaseCollection": "knowledgeArticles"
  },
  "JwtSettings": {
    "Secret": "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS_LONG!!",
    "Issuer": "TicketSystem",
    "Audience": "TicketSystemUsers"
  },
  "GitHubModels": {
    "ApiKey": "YOUR_GITHUB_MODELS_API_KEY",
    "Endpoint": "https://models.inference.ai.azure.com",
    "Model": "gpt-4o-mini"
  }
}
```

> ⚠️ **Never commit real secrets.** Use User Secrets or environment variables in production.

Run the API:

```bash
dotnet restore
dotnet run --launch-profile http
```

The API starts at `http://localhost:5000`. Swagger UI is available at `http://localhost:5000/swagger`.

> On first run, **DataSeeder** automatically creates users, sample tickets, and 9 knowledge base articles.

---

### 5️⃣ Frontend Setup

```bash
cd TicketSystem/ticket-system
npm install
```

Verify the API URL in `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api'
};
```

Start the development server:

```bash
npm start
# or
ng serve
```

Open your browser at **http://localhost:4200**

---

## 🔐 Environment Variables

### Backend (`appsettings.json`)

| Key | Description | Example |
|-----|-------------|---------|
| `MongoDbSettings:ConnectionString` | MongoDB Atlas connection URI | `mongodb+srv://user:pass@cluster...` |
| `MongoDbSettings:DatabaseName` | Target database name | `ticketdb` |
| `JwtSettings:Secret` | JWT signing key (≥ 32 chars) | `MySecretKey123...` |
| `JwtSettings:Issuer` | JWT issuer claim | `TicketSystem` |
| `JwtSettings:Audience` | JWT audience claim | `TicketSystemUsers` |
| `GitHubModels:ApiKey` | GitHub Models personal access token | `ghp_xxxx...` |
| `GitHubModels:Endpoint` | Inference endpoint | `https://models.inference.ai.azure.com` |
| `GitHubModels:Model` | Model identifier | `gpt-4o-mini` |

### Frontend (`environment.ts`)

| Key | Description | Example |
|-----|-------------|---------|
| `apiUrl` | Backend base URL | `http://localhost:5000/api` |

---

## 📄 Database Schema

### User Document

```json
{
  "_id": "ObjectId",
  "fullName": "string",
  "email": "string (unique)",
  "passwordHash": "string (bcrypt)",
  "role": 0,
  "isActive": true,
  "createdAt": "ISODate",
  "handledCategories": ["AuthenticationIssue", "SecurityIssue"]
}
```

`handledCategories` — empty array = generalist agent (receives any ticket). Non-empty = specialist (preferred for matching categories).

### Ticket Document

```json
{
  "_id": "ObjectId",
  "title": "string",
  "description": "string",
  "status": 0,
  "priority": 1,
  "category": 2,
  "createdById": "ObjectId ref",
  "createdByName": "string",
  "assignedToId": "ObjectId ref | null",
  "assignedToName": "string | null",
  "agentNotes": "string",
  "messages": [
    {
      "id": "ObjectId string",
      "authorRole": 1,
      "authorUserId": "ObjectId ref",
      "authorName": "string",
      "body": "string",
      "createdAt": "ISODate",
      "replyToMessageId": "string | null"
    }
  ],
  "createdAt": "ISODate",
  "updatedAt": "ISODate",
  "resolvedAt": "ISODate | null"
}
```

### KnowledgeArticle Document

```json
{
  "_id": "ObjectId",
  "title": "string",
  "summary": "string",
  "body": "string",
  "category": 0,
  "keywords": ["password", "reset"],
  "createdAt": "ISODate"
}
```

### Enums

```csharp
// UserRole
Admin = 0, Agent = 1, Customer = 2

// TicketStatus
Open = 0, InProgress = 1, Resolved = 2

// TicketPriority
Low = 0, Medium = 1, High = 2

// TicketCategory
AuthenticationIssue = 0, BillingIssue = 1, TechnicalIssue = 2,
PerformanceIssue = 3, FeatureRequest = 4, GeneralInquiry = 5,
AccountManagement = 6, NotificationIssue = 7, DataIssue = 8,
SecurityIssue = 9, UncategorizedIssue = 10
```

---

## 📡 API Documentation

Base URL: `http://localhost:5000/api`

All protected endpoints require the header:
```
Authorization: Bearer <jwt_token>
```

---

### 🔑 Authentication Endpoints

#### `POST /api/auth/register`

Register a new customer or agent account.

**Request Body:**
```json
{
  "fullName": "Jane Smith",
  "email": "jane@example.com",
  "password": "SecurePass@123",
  "role": 2
}
```
> `role`: `1` = Agent, `2` = Customer. Admin role is blocked on public registration.

**Response `200`:**
```json
{
  "token": "eyJhbGci...",
  "userId": "6849a1b2c3d4e5f6a7b8c9d0",
  "fullName": "Jane Smith",
  "email": "jane@example.com",
  "role": "Customer"
}
```

**Response `409`:** Email already registered.

---

#### `POST /api/auth/login`

Authenticate and receive a JWT.

**Request Body:**
```json
{
  "email": "admin@ticketsys.com",
  "password": "Admin@123"
}
```

**Response `200`:** Same as register response.

**Response `401`:** Invalid credentials.

**Response `403`:**
```json
{ "message": "user_deactivated" }
```

---

### 🎫 Ticket Endpoints

#### `POST /api/ticket` — *Customer only*

Create a new support ticket. AI classification and smart routing happen asynchronously in the background.

**Request Body:**
```json
{
  "title": "Cannot login to my account",
  "description": "I keep getting an invalid password error even after resetting via email. This started yesterday."
}
```

**Response `200`:** Returns the created `TicketDto`.

---

#### `GET /api/ticket/my` — *Customer only*

Get all tickets created by the authenticated customer.

**Response `200`:**
```json
[
  {
    "id": "6849a1b2c3d4e5f6a7b8c9d0",
    "title": "Cannot login to my account",
    "description": "...",
    "status": "InProgress",
    "priority": "High",
    "category": "AuthenticationIssue",
    "createdByName": "Jane Smith",
    "assignedToName": "Sarah (Auth & Accounts)",
    "agentNotes": "",
    "messages": [],
    "createdAt": "2025-01-15T10:30:00Z",
    "updatedAt": "2025-01-15T10:31:05Z"
  }
]
```

---

#### `GET /api/ticket/assigned` — *Agent only*

Get all tickets assigned to the authenticated agent.

**Response `200`:** Array of `TicketDto`.

---

#### `PUT /api/ticket/{id}/status` — *Agent only*

Update the status of an assigned ticket.

**Request Body:**
```json
{ "status": 1 }
```
> `status`: `0` = Open, `1` = InProgress, `2` = Resolved

**Response `200`:** `{ "message": "Status updated." }`

**Response `404`:** Ticket not found or not assigned to caller.

---

#### `PUT /api/ticket/{id}/notes` — *Agent only*

Save internal notes on a ticket (legacy field, pre-threading).

**Request Body:**
```json
{ "notes": "Checked logs, reset was sent. Waiting on customer reply." }
```

**Response `200`:** `{ "message": "Notes saved." }`

---

#### `POST /api/ticket/{id}/messages` — *Customer, Agent, Admin*

Add a threaded message to a ticket conversation.

**Request Body:**
```json
{
  "body": "I tried the reset link again and it still fails.",
  "replyToMessageId": "6849a1b2c3d4e5f6a7b8c9d1"
}
```
> `replyToMessageId` is optional.

**Response `200`:**
```json
{
  "id": "6849a1b2c3d4e5f6a7b8c9d2",
  "authorRole": "Customer",
  "authorName": "Jane Smith",
  "body": "I tried the reset link again and it still fails.",
  "createdAt": "2025-01-15T11:00:00Z",
  "replyToMessageId": "6849a1b2c3d4e5f6a7b8c9d1"
}
```

**Error responses:** `400` empty/too long body, `403` not authorised on this ticket, `404` ticket not found.

---

#### `GET /api/ticket/{id}/reply-assist` — *Agent only*

Get an AI-drafted reply suggestion based on the ticket and knowledge base. Only works if the ticket is assigned to the calling agent.

**Response `200`:**
```json
{
  "draft": "Hello,\n\nThank you for contacting us about your login issue. Based on our password reset process...",
  "sources": [
    {
      "articleId": "6849a1b2c3d4e5f6a7b8c9d5",
      "title": "Reset password flow"
    }
  ]
}
```

**Response `403`:** Ticket not assigned or assigned to a different agent.

---

### 🔧 Admin Endpoints

All admin endpoints require `Authorization: Bearer <admin_jwt>`.

#### `GET /api/admin/users`

Get all users sorted by creation date descending.

**Response `200`:**
```json
[
  {
    "id": "...",
    "fullName": "Sarah (Auth & Accounts)",
    "email": "agent.auth@ticketsys.com",
    "role": "Agent",
    "isActive": true,
    "createdAt": "2025-01-01T00:00:00Z",
    "handledCategories": ["AuthenticationIssue", "AccountManagement", "SecurityIssue"]
  }
]
```

---

#### `GET /api/admin/agents`

Get all active agents (used to populate assignment dropdowns).

**Response `200`:** Array of `UserDto` where `role === "Agent"` and `isActive === true`.

---

#### `POST /api/admin/users`

Create a user with any role (including Admin).

**Request Body:**
```json
{
  "fullName": "New Agent",
  "email": "newagent@company.com",
  "password": "Agent@123",
  "role": 1
}
```

**Response `200`:** `{ "message": "User created." }`

**Response `409`:** Email already exists.

---

#### `PUT /api/admin/users/role`

Change a user's role.

**Request Body:**
```json
{
  "userId": "6849a1b2c3d4e5f6a7b8c9d0",
  "newRole": 1
}
```

**Response `200`:** `{ "message": "Role updated." }`

---

#### `DELETE /api/admin/users/{id}`

Soft-deactivate a user (sets `isActive = false`).

**Response `200`:** `{ "message": "User deactivated." }`

---

#### `PUT /api/admin/users/{id}/activate`

Re-activate a deactivated user.

**Response `200`:** `{ "message": "User activated." }`

---

#### `DELETE /api/admin/users/{id}/permanent`

Permanently remove a user document. Admins cannot delete their own account.

**Response `200`:** `{ "message": "User permanently deleted." }`

**Response `400`:** `{ "message": "You cannot delete your own account." }`

---

#### `GET /api/admin/tickets`

Get all tickets across all users.

**Response `200`:** Array of `TicketDto`.

---

#### `PUT /api/admin/tickets/{ticketId}/assign`

Manually assign a ticket to an agent (overrides smart routing).

**Request Body:**
```json
{ "agentId": "6849a1b2c3d4e5f6a7b8c9d0" }
```

**Response `200`:** `{ "message": "Ticket assigned." }`

**Response `400`:** Ticket or agent not found.

---

## 🧪 Testing

### Test Credentials

The DataSeeder populates these accounts automatically on first run:

| Role | Email | Password | Notes |
|------|-------|----------|-------|
| **Admin** | `admin@ticketsys.com` | `Admin@123` | Full system access |
| **Agent (Auth)** | `agent.auth@ticketsys.com` | `Agent@123` | Handles auth, account & security tickets |
| **Agent (Billing)** | `agent.billing@ticketsys.com` | `Agent@123` | Handles billing tickets |
| **Agent (Technical)** | `agent.tech@ticketsys.com` | `Agent@123` | Handles tech, performance, data, notification |
| **Agent (Generalist)** | `agent@ticketsys.com` | `Agent@123` | Handles any category |
| **Customer** | `customer@ticketsys.com` | `Customer@123` | Standard customer account |

> The **Login** page has **Quick Demo** buttons that auto-fill these credentials.

### Sample API Calls with cURL

```bash
# 1. Login as admin
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@ticketsys.com","password":"Admin@123"}'

# 2. Save the token
TOKEN="<paste token here>"

# 3. Get all users
curl http://localhost:5000/api/admin/users \
  -H "Authorization: Bearer $TOKEN"

# 4. Login as customer
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"customer@ticketsys.com","password":"Customer@123"}'

CUST_TOKEN="<paste customer token here>"

# 5. Create a ticket
curl -X POST http://localhost:5000/api/ticket \
  -H "Authorization: Bearer $CUST_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"title":"Billing charge issue","description":"I was charged twice for my subscription this month. Transaction IDs: TXN-001, TXN-002."}'

# 6. View my tickets
curl http://localhost:5000/api/ticket/my \
  -H "Authorization: Bearer $CUST_TOKEN"
```

### Swagger UI

With the backend running, visit:
```
http://localhost:5000/swagger
```

---

## 🚀 Deployment

### Backend — MonsterASP.NET

1. Publish the .NET project:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
2. Upload `publish/` contents to MonsterASP.NET via File Manager or FTP
3. Set environment variables in the hosting control panel (or use `appsettings.Production.json`)
4. Update the CORS policy in `Program.cs` with your production domain

### Frontend — MonsterASP.NET

1. Update `src/environments/environment.prod.ts` with your API URL:
   ```typescript
   export const environment = {
     production: true,
     apiUrl: 'https://YOUR_DOMAIN.monsterasp.net/api'
   };
   ```
2. Build the Angular app:
   ```bash
   ng build --configuration production
   ```
3. Upload the contents of `dist/ticket-system/browser/` to your hosting public folder

---

## 🤝 Contributing

Contributions are welcome! Please follow these steps:

1. **Fork** the repository
2. **Create a branch** for your feature:
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Commit your changes** with clear messages:
   ```bash
   git commit -m "feat: add email notification on ticket assignment"
   ```
4. **Push** to your fork:
   ```bash
   git push origin feature/your-feature-name
   ```
5. Open a **Pull Request** against `main`

### Commit Message Convention

| Prefix | When to use |
|--------|-------------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `docs:` | Documentation only |
| `refactor:` | Code restructuring without feature change |
| `style:` | CSS / formatting |
| `chore:` | Build scripts, dependencies |

### Code Standards

- **Angular:** Use `ChangeDetectionStrategy.OnPush` for all dashboard components; avoid impure pipes
- **.NET:** Follow existing service pattern; keep controllers thin; add XML docs to public methods
- **No secrets in commits:** Use `appsettings.Development.json` (gitignored) for local secrets

---

## 📜 License

This project is licensed under the **MIT License**.

```
MIT License

Copyright (c) 2025 TicketAI Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
```

---

<div align="center">

Built with ❤️ using Angular, .NET 10, MongoDB, and GPT-4o-mini

</div>
