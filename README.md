# CryptoTracker — Startup Instruction

## Requirements
- .NET 8 SDK (https://dotnet.microsoft.com/download)
- Visual Studio 2022 / Rider / VS Code

## Quick start

### 1. Install dependencies
```bash
cd CryptoTracker
dotnet restore
```

### 2. Launch a project
```bash
dotnet run
```
Open in browser: **http://localhost:5000**

Database `cryptotracker.db` (SQLite) will be created automatically on first launch.

---

## Project structure
```
CryptoTracker/
├── Controllers/
│   ├── AuthController.cs       # POST /api/auth/login|register|logout, GET /api/auth/me
│   ├── CryptoController.cs     # GET /api/crypto/prices, /prices/by-ids, /search
│   └── PortfolioController.cs  # GET|POST /api/portfolio, DELETE /api/portfolio/{id}
├── Models/
│   ├── User.cs                 # User entity
│   ├── PortfolioItem.cs        # The coin in the portfolio
│   └── CoinGeckoDto.cs         # DTO for CoinGecko API and responses
├── Data/
│   └── AppDbContext.cs         # Entity Framework context
├── wwwroot/
│   ├── index.html              # Home - Exchange Rate Table
│   ├── portfolio.html          # Personal portfolio
│   ├── login.html              # Login page
│   ├── register.html           # Registration page
│   ├── css/style.css           # Custom styles
│   ├── js/auth.js              # General authorization utilities
│   ├── js/app.js               # Homepage logic
│   ├── js/portfolio.js         # Portfolio Page Logic
│   └── robots.txt              # SEO
├── Program.cs                  # Application configuration
├── appsettings.json            # Database connection string
└── CryptoTracker.csproj        # NuGet packages
```

## API Endpoints

| Method | URL | Description |
|-------|-----|----------|
| POST | /api/auth/register | Registration |
| POST | /api/auth/login | Login |
| POST | /api/auth/logout | Logout |
| GET | /api/auth/me | Current user |
| GET | /api/crypto/prices | Top coins from the market |
| GET | /api/crypto/search?query=... | Coin search |
| GET | /api/portfolio | User portfolio |
| POST | /api/portfolio | Add a coin |
| DELETE | /api/portfolio/{id} | Delete item |

## Switching to SQL Server (optional)
In `CryptoTracker.csproj` replace package:
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
```
In `Program.cs` replace:
```csharp
options.UseSqlite(...)  →  options.UseSqlServer(...)
```
In `appsettings.json`:
```json
"DefaultConnection": "Server=localhost;Database=CryptoTracker;Trusted_Connection=true;"
```
