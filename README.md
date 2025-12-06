# Dotnet PostgreSQL Demo

## Initializing the .NET Project

```bash
dotnet new sln -o DotnetPgDemo
dotnet new webapi --use-controllers -o DotnetPgDemo.Api
dotnet sln add DotnetPgDemo.Api/DotnetPgDemo.Api.csproj
dotnet new gitignore


dotnet run --project DotnetPgDemo.Api

initdb -D dbdata
pg_ctl -D dbdata -l logfile start

## EF Core
dotnet add DotnetPgDemo.Api/DotnetPgDemo.Api.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add DotnetPgDemo.Api/DotnetPgDemo.Api.csproj package Microsoft.EntityFrameworkCore.Design

dotnet tool update --global dotnet-ef
dotnet ef migrations Add InitialCreate
dotnet ef database update --project DotnetPgDemo.Api/DotnetPgDemo.Api.csproj
```

```json
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=mydb;Username=postgres;Password=123456"
  }
```

## Managing PostgreSQL Server

If you encounter errors like `could not bind IPv4 address "127.0.0.1": Permission denied`, another PostgreSQL instance is likely running (usually as a system service).

### Windows

**Stop System Service** (Run as Administrator):

```cmd
net stop postgresql-x64-16
:: If unsure of the name: sc query state= all | findstr "postgres"
```

**Start Local Instance:**

```cmd
pg_ctl -D dbdata -l logfile start
```

**Restart Local Instance:**

```cmd
pg_ctl -D dbdata -l logfile restart
```

**Connect:**

```cmd
psql -h localhost -U postgres
```

### macOS

**Stop System Service:**

```bash
brew services stop postgresql
# Or: brew services stop postgresql@15
```

**Start Local Instance:**

```bash
pg_ctl -D dbdata -l logfile start
```

**Restart Local Instance:**

```bash
pg_ctl -D dbdata -l logfile restart
```

**Connect:**

```bash
psql postgres
```

### Linux (Ubuntu/Debian)

**Stop System Service:**

```bash
sudo systemctl stop postgresql
```

**Start Local Instance:**

```bash
pg_ctl -D dbdata -l logfile start
```

**Restart Local Instance:**

```bash
pg_ctl -D dbdata -l logfile restart
```

**Connect:**

```bash
psql -h localhost -d postgres
```
