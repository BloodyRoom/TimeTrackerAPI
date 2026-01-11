# ⏱️ TimeTrackerAPI

**TimeTrackerAPI** is a RESTful Web API for tracking users' working time with authentication, time sessions, and statistics support.  
The project is built with **ASP.NET Core (.NET 10)** using **JWT authentication with Refresh Tokens** and **Entity Framework Core**.


> This project is educational–practical




## 🚀 Features

- 🔐 User registration and authentication
- 🪪 JWT Access Token + Refresh Token
- 🔁 Token refresh flow
- ⏱️ Start / pause / stop time sessions
- 📊 Time statistics per user
- 🧩 Clean architecture (Core / Domain / API)
- 📖 Swagger (OpenAPI) documentation

---

## 🧱 Tech Stack

- **ASP.NET Core (.NET 10)**
- **Entity Framework Core**
- **PostgreSQL**
- **JWT Bearer Authentication**
- **AutoMapper**
- **Swagger / OpenAPI**


---

## 🔐 Authentication

The API uses **JWT Bearer Authentication**:

- **Access Token** — short-lived
- **Refresh Token** — stored in the database and can be revoked

### Refresh token endpoint
```http
POST /api/Token/Refresh
```

---

## 📡 Main Endpoints

### 🔑 Account
| Method | Endpoint | Description |
|------|---------|-------------|
| POST | `/api/Account/Register` | Register new user |
| POST | `/api/Account/Login` | User login |
| POST | `/api/Account/Logout` | Logout user |
| POST | `/api/Account/Google` | Login via Google |

### ⏱️ Sessions
| Method | Endpoint | Description |
|------|---------|-------------|
| POST | `/api/Session/Start` | Start session |
| POST | `/api/Session/Pause` | Pause session |
| POST | `/api/Session/Resume` | Resume session |
| POST | `/api/Session/Stop` | Stop session |
| GET  | `/api/Session/Statistics/{type}` | Time statistics |

### 🔁 Tokens
| Method | Endpoint | Description |
|------|---------|-------------|
| POST | `/api/Token/Refresh` | Refresh access token |

---

## ⚙️ Configuration

### `appsettings.json`
```json
{
  "ConnectionStrings": {
    "Connection": "Host=...;Database=...;Username=...;Password=..."
  },
  "Jwt": {
    "Key": "SUPER_SECRET_KEY"
  }
}
```

> ❗ **Never commit real secrets or credentials**

---

## ▶️ Run the Project

```bash
dotnet restore
dotnet build
dotnet run
```

(Debug mode only)
Swagger UI will be available at:
```
https://localhost:{port}/swagger
```

---

## 📖 Swagger

The API is fully documented with Swagger.  
Supports authorization via **Authorize → Bearer Token**.
