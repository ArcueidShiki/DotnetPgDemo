```bash
dotnet new sln -o DotnetPgDemo
dotnet new webapi --use-controllers -o DotnetPgDemo.Api
dotnet sln add DotnetPgDemo.Api/DotnetPgDemo.Api.csproj
dotnet new gitignore
```