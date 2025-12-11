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



### project types

```bash
dotnet new list
Template Name                                 Short Name                    Language    Tags
--------------------------------------------  ----------------------------  ----------  ----------------------------------
API Controller                                apicontroller                 [C#]        Web/ASP.NET
ASP.NET Core Empty                            web                           [C#],F#     Web/Empty
ASP.NET Core gRPC Service                     grpc                          [C#]        Web/gRPC/API/Service
ASP.NET Core Web API                          webapi                        [C#],F#     Web/Web API/API/Service
ASP.NET Core Web API (native AOT)             webapiaot                     [C#]        Web/Web API/API/Service
ASP.NET Core Web App (Model-View-Controller)  mvc                           [C#],F#     Web/MVC
ASP.NET Core Web App (Razor Pages)            webapp,razor                  [C#]        Web/MVC/Razor Pages
Blazor Web App                                blazor                        [C#]        Web/Blazor/WebAssembly
Blazor WebAssembly Standalone App             blazorwasm                    [C#]        Web/Blazor/WebAssembly/PWA
Class Library                                 classlib                      [C#],F#,VB  Common/Library
Console App                                   console                       [C#],F#,VB  Common/Console
dotnet gitattributes file                     gitattributes,.gitattributes              Config
dotnet gitignore file                         gitignore,.gitignore                      Config
Dotnet local tool manifest file               tool-manifest                             Config
EditorConfig file                             editorconfig,.editorconfig                Config
global.json file                              globaljson,global.json                    Config
MSBuild Directory.Build.props file            buildprops                                MSBuild/props
MSBuild Directory.Build.targets file          buildtargets                              MSBuild/props
MSBuild Directory.Packages.props file         packagesprops                             MSBuild/packages/props/CPM
MSTest Playwright Test Project                mstest-playwright             [C#]        Test/MSTest/Playwright/Desktop/Web
MSTest Test Class                             mstest-class                  [C#],F#,VB  Test/MSTest
MSTest Test Project                           mstest                        [C#],F#,VB  Test/MSTest/Desktop/Web
MVC Controller                                mvccontroller                 [C#]        Web/ASP.NET
MVC ViewImports                               viewimports                   [C#]        Web/ASP.NET
MVC ViewStart                                 viewstart                     [C#]        Web/ASP.NET
NuGet Config                                  nugetconfig,nuget.config                  Config
NUnit 3 Test Item                             nunit-test                    [C#],F#,VB  Test/NUnit
NUnit Playwright Test Project                 nunit-playwright              [C#]        Test/NUnit/Playwright/Desktop/Web
NUnit Test Project                            nunit                         [C#],F#,VB  Test/NUnit/Desktop/Web
Protocol Buffer File                          proto                                     Web/gRPC
Razor Class Library                           razorclasslib                 [C#]        Web/Razor/Library
Razor Component                               razorcomponent                [C#]        Web/ASP.NET
Razor Page                                    page                          [C#]        Web/ASP.NET
Razor View                                    view                          [C#]        Web/ASP.NET
Solution File                                 sln,solution                              Solution
Web Config                                    webconfig                                 Config
Windows Forms App                             winforms                      [C#],VB     Common/WinForms
Windows Forms Class Library                   winformslib                   [C#],VB     Common/WinForms
Windows Forms Control Library                 winformscontrollib            [C#],VB     Common/WinForms
Worker Service                                worker                        [C#],F#     Common/Worker/Web
WPF Application                               wpf                           [C#],VB     Common/WPF
WPF Class Library                             wpflib                        [C#],VB     Common/WPF
WPF Custom Control Library                    wpfcustomcontrollib           [C#],VB     Common/WPF
WPF User Control Library                  wpfusercontrollib             [C#],VB     Common/WPF
xUnit Test Project                        xunit                         [C#],F#,VB  Test/xUnit/Desktop/Web
```

---

## .NET Project Templates Guide

### Web Application Templates

| Template | Short Name | Purpose | When to Use |
|----------|------------|---------|-------------|
| **ASP.NET Core Empty** | `web` | Minimal web app with no predefined structure | Custom web apps, learning, when you want full control |
| **ASP.NET Core Web API** | `webapi` | REST API with controllers | Backend APIs, microservices |
| **ASP.NET Core Web API (native AOT)** | `webapiaot` | AOT-compiled API for fast startup | Cloud functions, serverless, containers where startup time matters |
| **ASP.NET Core gRPC Service** | `grpc` | High-performance RPC service | Internal microservice communication, real-time services |
| **ASP.NET Core Web App (MVC)** | `mvc` | Model-View-Controller pattern | Full-featured web apps with server-side rendering |
| **ASP.NET Core Web App (Razor Pages)** | `webapp`, `razor` | Page-focused web app | Simpler web apps, forms, content-focused sites |

#### Web API vs gRPC vs MVC

| Feature | Web API | gRPC | MVC |
|---------|---------|------|-----|
| Protocol | HTTP/REST + JSON | HTTP/2 + Protocol Buffers | HTTP + HTML |
| Client | Any HTTP client | Generated client | Browser |
| Use Case | Public APIs | Internal services | Web applications |
| Performance | Good | Excellent | Good |
| Browser Support | Yes | Limited | Yes |

#### When to Choose:
- **webapi**: Public REST APIs, mobile app backends, third-party integrations
- **webapiaot**: Lambda/Azure Functions, containerized apps where cold start matters
- **grpc**: High-throughput internal services, real-time bidirectional streaming
- **mvc**: Enterprise web apps with complex views, SEO requirements
- **webapp/razor**: Simpler web apps, content sites, admin panels

### Blazor Templates

| Template | Short Name | Purpose | When to Use |
|----------|------------|---------|-------------|
| **Blazor Web App** | `blazor` | Full-stack Blazor with Server/WebAssembly options | Modern web apps, .NET developers building SPAs |
| **Blazor WebAssembly Standalone** | `blazorwasm` | Client-side only Blazor | Static site hosting, offline-capable PWAs |

#### Server vs WebAssembly

| Feature | Blazor Server | Blazor WebAssembly |
|---------|---------------|-------------------|
| Execution | Server via SignalR | Browser via WASM |
| Initial Load | Fast | Slow (downloads .NET runtime) |
| Latency | Network dependent | None after load |
| Offline | No | Yes |
| Server Resources | High (per-connection) | Low |

#### When to Choose:
- **blazor** (Server): Internal apps, low latency requirements, simpler deployment
- **blazorwasm**: Public apps, offline capability, reduced server load, PWAs

### Console & Library Templates

| Template | Short Name | Purpose | When to Use |
|----------|------------|---------|-------------|
| **Console App** | `console` | Command-line application | CLI tools, batch processing, utilities |
| **Class Library** | `classlib` | Reusable code library | Shared code, NuGet packages, domain logic |
| **Worker Service** | `worker` | Long-running background service | Windows Services, scheduled tasks, message processing |

#### When to Choose:
- **console**: One-time scripts, CLI tools, utilities
- **classlib**: Code shared across multiple projects, NuGet packages
- **worker**: Background processing, scheduled jobs, Windows/Linux services

### Test Project Templates

| Template | Short Name | Purpose |
|----------|------------|---------|
| **MSTest Test Project** | `mstest` | Microsoft's test framework |
| **NUnit Test Project** | `nunit` | Popular community test framework |
| **xUnit Test Project** | `xunit` | Modern, extensible test framework |
| **MSTest Playwright Test** | `mstest-playwright` | E2E browser testing with MSTest |
| **NUnit Playwright Test** | `nunit-playwright` | E2E browser testing with NUnit |

#### MSTest vs NUnit vs xUnit

| Feature | MSTest | NUnit | xUnit |
|---------|--------|-------|-------|
| Microsoft Support | ✓ Official | Community | Community |
| Popularity | Medium | High | Very High |
| Extensibility | Good | Good | Excellent |
| Parallel Execution | Good | Good | Built-in |
| Assertions | Basic | Rich | Minimal (use FluentAssertions) |

#### When to Choose:
- **mstest**: Microsoft shops, Visual Studio integration priority
- **nunit**: Migration from older projects, rich assertion library
- **xunit**: Modern projects, ASP.NET Core projects (used by Microsoft internally)
- **playwright**: End-to-end browser testing, UI automation

### Desktop Application Templates

| Template | Short Name | Purpose | When to Use |
|----------|------------|---------|-------------|
| **Windows Forms App** | `winforms` | Traditional Windows desktop | Legacy support, simple business apps |
| **WPF Application** | `wpf` | Modern Windows desktop with XAML | Rich UI, data-driven apps, MVVM |
| **WPF Class Library** | `wpflib` | Reusable WPF components | Shared WPF controls/resources |
| **WPF Custom Control Library** | `wpfcustomcontrollib` | Templated WPF controls | Custom control development |
| **WPF User Control Library** | `wpfusercontrollib` | Composite WPF controls | Reusable UI compositions |

#### WinForms vs WPF

| Feature | Windows Forms | WPF |
|---------|--------------|-----|
| UI Design | Designer drag-drop | XAML + Designer |
| Data Binding | Basic | Powerful |
| Graphics | GDI+ | DirectX-based |
| Styling | Limited | Full templates/styles |
| Learning Curve | Easy | Steeper |
| Performance | Good | Better for complex UIs |

#### When to Choose:
- **winforms**: Quick prototypes, simple tools, maintaining legacy apps
- **wpf**: New desktop apps, rich UIs, MVVM architecture, data visualization

### Configuration Templates

| Template | Short Name | Purpose |
|----------|------------|---------|
| **NuGet Config** | `nugetconfig` | Package source configuration |
| **global.json** | `globaljson` | SDK version pinning |
| **EditorConfig** | `editorconfig` | Code style enforcement |
| **gitignore** | `gitignore` | Git ignore rules |
| **gitattributes** | `gitattributes` | Git attributes |
| **Web Config** | `webconfig` | IIS configuration |
| **Solution File** | `sln` | Visual Studio solution |

### Comparison Summary: Similar Templates

#### Empty vs Template-Based
```
web (empty) → Full control, no opinions
webapi      → Controllers, OpenAPI, ready for REST
mvc         → Views, controllers, full MVC structure
```

#### Library Variants
```
classlib            → General-purpose code
wpflib              → WPF-specific with XAML support
wpfcustomcontrollib → Templatable WPF controls
wpfusercontrollib   → Composite WPF UserControls
razorclasslib       → Razor components for Blazor/MVC
```

#### Test Framework Selection
```
Simple unit tests     → xunit (modern, widely adopted)
.NET Framework legacy → nunit (mature, battle-tested)
Microsoft ecosystem   → mstest (integrated tooling)
Browser testing       → Add Playwright to any framework
```