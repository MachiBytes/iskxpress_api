# ISK Express API

A food delivery API built with .NET 9 and Entity Framework Core.

## Quick Start

### Prerequisites
- .NET 9 SDK
- MySQL Database

### Clone & Run
```bash
# Clone the repository
git clone <repository-url>
cd iskxpress_api

# Restore dependencies
dotnet restore

# Set connection string (see Database Setup below)

# Create database
dotnet ef database update

# Run the application
dotnet run
```

## Database Setup

Set your MySQL connection string using dotnet user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=your-server.com;Port=3306;Database=iskxpress-staging;User=your-username;Password=your-password;"
```

Example:
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=*****.rds.amazonaws.com;Port=3306;Database=iskxpress-staging;User=admin;Password=*****;"
```

Then apply migrations:
```bash
dotnet ef database update
```

## Testing

Run comprehensive tests to verify database migrations and repository functionality:

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "DisplayName~DatabaseMigrationTests"
dotnet test --filter "DisplayName~RepositoryTests"

# Run tests with detailed output
dotnet test --logger console --verbosity normal
```

### Test Coverage
- **Database Migration Tests**: Verify all tables are created correctly with proper schema and relationships
- **Repository Tests**: Test CRUD operations and specialized repository methods
- **Schema Validation**: Ensure foreign keys, constraints, and navigation properties work as expected

## Making Changes

### Adding New Features
1. Create models in `Models/` folder
2. Update `IskExpressDbContext.cs` if needed
3. Create repositories in `Repositories/` folder
4. Register services in `Program.cs`
5. Add tests in `Tests/` folder

### Database Changes
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Apply to database
dotnet ef database update
```

### Development Commands
```bash
# Build project
dotnet build

# Run with hot reload
dotnet watch run

# Run tests
dotnet test
```

## Project Structure
```
├── Models/          # Database entities
├── Data/            # DbContext
├── Repositories/    # Data access layer
├── Tests/           # Unit and integration tests
├── Migrations/      # EF migrations
└── Program.cs       # App configuration
```

## API Endpoints
- `GET /` - Health check
- OpenAPI documentation available in development mode

---
Built for ISK Express food delivery platform 