# NuGet Packages Reference Guide

This document explains all the NuGet packages used in the Hotel Booking System and their purposes.

---

## Application Layer Packages

### **MediatR**
- **Purpose**: Implements the CQRS pattern (Command Query Responsibility Segregation)
- **Use**: Separates read operations (Queries) from write operations (Commands)
- **Example**: Instead of calling a service directly, you send a `CreateHotelCommand` through MediatR, which routes it to the appropriate handler
- **Benefit**: Loose coupling, single responsibility, easier testing

### **FluentValidation**
- **Purpose**: Input validation with a fluent API
- **Use**: Validates commands/queries before they reach your business logic
- **Example**:
  ```csharp
  RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
  RuleFor(x => x.Email).EmailAddress();
  ```
- **Benefit**: Clean, reusable validation rules separate from business logic

### **FluentValidation.DependencyInjectionExtensions**
- **Purpose**: Automatically registers FluentValidation validators in DI container
- **Benefit**: No manual registration needed

### **AutoMapper**
- **Purpose**: Object-to-object mapping (Entity ↔ DTO)
- **Use**: Converts domain entities to DTOs and vice versa
- **Example**: Maps `Hotel` entity to `HotelDto` without manual property copying
- **Benefit**: Reduces boilerplate code, maintains clean separation

### **AutoMapper.Extensions.Microsoft.DependencyInjection**
- **Purpose**: Integrates AutoMapper with .NET dependency injection
- **Benefit**: Automatic registration of mapping profiles

---

## Persistence Layer Packages

### **Microsoft.EntityFrameworkCore**
- **Purpose**: ORM (Object-Relational Mapper) - main EF Core library
- **Use**: Maps C# classes to database tables, provides LINQ queries
- **Example**: `dbContext.Hotels.Where(h => h.City == "Paris")`
- **Benefit**: Write C# code instead of SQL, type-safe queries

### **Microsoft.EntityFrameworkCore.Design**
- **Purpose**: Design-time tools for EF Core
- **Use**: Enables migrations, scaffolding, and design-time DbContext creation
- **Benefit**: Required for `dotnet ef migrations` commands

### **Npgsql.EntityFrameworkCore.PostgreSQL**
- **Purpose**: PostgreSQL database provider for EF Core
- **Use**: Enables EF Core to work with PostgreSQL databases
- **Benefit**: PostgreSQL-specific optimizations and features

### **Microsoft.EntityFrameworkCore.Tools**
- **Purpose**: Command-line tools for EF Core migrations
- **Use**: Run migrations from Package Manager Console in Visual Studio
- **Benefit**: Alternative way to run EF Core commands

---

## Infrastructure Layer Packages

### **Microsoft.Extensions.Http**
- **Purpose**: Provides HttpClient factory and typed clients
- **Use**: Make HTTP calls to external APIs (e.g., payment gateway, notification service)
- **Example**: Calling a third-party API for geocoding addresses
- **Benefit**: Connection pooling, proper disposal, easier testing

---

## API Layer Packages

### **Swashbuckle.AspNetCore**
- **Purpose**: Generates Swagger/OpenAPI documentation
- **Use**: Auto-generates interactive API documentation
- **Benefit**: Test endpoints in browser, automatic API docs, client code generation

### **Serilog.AspNetCore**
- **Purpose**: Structured logging framework
- **Use**: Logs application events with structured data
- **Example**: `Log.Information("Hotel {HotelId} created by {UserId}", hotelId, userId)`
- **Benefit**: Better than Console.WriteLine, searchable logs, multiple outputs

### **Serilog.Sinks.Console**
- **Purpose**: Writes logs to console
- **Use**: Development environment logging
- **Benefit**: See logs in terminal during development

### **Serilog.Sinks.File**
- **Purpose**: Writes logs to files
- **Use**: Production logging, log persistence
- **Benefit**: Logs survive application restarts, can be analyzed later

### **Microsoft.EntityFrameworkCore.Design**
- **Purpose**: Same as Persistence layer - enables EF migrations from API project
- **Benefit**: Can run `dotnet ef` commands from the API project

---

## Unit Tests Packages

### **xUnit**
- **Purpose**: Testing framework
- **Use**: Write and run unit tests
- **Example**: `[Fact]` and `[Theory]` attributes for test methods
- **Benefit**: Modern, popular, parallel test execution

### **xunit.runner.visualstudio**
- **Purpose**: Visual Studio test runner integration
- **Use**: Run tests from VS/VSCode Test Explorer
- **Benefit**: IDE integration, visual test results

### **Microsoft.NET.Test.Sdk**
- **Purpose**: .NET Test Platform
- **Use**: Required to run tests with `dotnet test`
- **Benefit**: Core testing infrastructure

### **Moq**
- **Purpose**: Mocking framework
- **Use**: Create fake implementations of interfaces for testing
- **Example**: Mock repository to test handlers without a real database
- **Benefit**: Isolate code under test, fast tests, no database needed

### **FluentAssertions**
- **Purpose**: Expressive assertion library
- **Use**: Write readable test assertions
- **Example**: `result.Should().NotBeNull()` vs `Assert.NotNull(result)`
- **Benefit**: Better error messages, more readable tests

---

## Integration Tests Packages

### **Microsoft.AspNetCore.Mvc.Testing**
- **Purpose**: In-memory test server for ASP.NET Core
- **Use**: Test API endpoints without deploying
- **Example**: Send HTTP requests to your API in tests
- **Benefit**: Fast, no external dependencies

### **Testcontainers**
- **Purpose**: Manages Docker containers in tests
- **Use**: Spin up real services (databases, Redis) for testing
- **Benefit**: Test against real infrastructure, automatic cleanup

### **Testcontainers.PostgreSql**
- **Purpose**: PostgreSQL-specific Testcontainers support
- **Use**: Automatically starts PostgreSQL in Docker for integration tests
- **Benefit**: Test with real database, no manual setup

---

## Architecture Tests Packages

### **NetArchTest.Rules**
- **Purpose**: Enforces architectural rules in code
- **Use**: Validates Clean Architecture dependencies
- **Example**: "Domain layer should not reference Application layer"
- **Benefit**: Prevents architectural violations, CI/CD integration

---

## Package Summary by Layer

### Application Layer
```bash
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### Persistence Layer
```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
```

### Infrastructure Layer
```bash
dotnet add package Microsoft.Extensions.Http
```

### API Layer
```bash
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Microsoft.EntityFrameworkCore.Design
```

### Unit Tests
```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package FluentAssertions
```

### Integration Tests
```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package FluentAssertions
dotnet add package Testcontainers
dotnet add package Testcontainers.PostgreSql
```

### Architecture Tests
```bash
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package NetArchTest.Rules
dotnet add package FluentAssertions
```

---

## Quick Reference Table

| Purpose | Packages |
|---------|----------|
| **CQRS Pattern** | MediatR |
| **Validation** | FluentValidation, FluentValidation.DependencyInjectionExtensions |
| **Mapping** | AutoMapper, AutoMapper.Extensions.Microsoft.DependencyInjection |
| **Database ORM** | Microsoft.EntityFrameworkCore |
| **PostgreSQL** | Npgsql.EntityFrameworkCore.PostgreSQL |
| **EF Migrations** | Microsoft.EntityFrameworkCore.Design, Microsoft.EntityFrameworkCore.Tools |
| **HTTP Clients** | Microsoft.Extensions.Http |
| **Logging** | Serilog.AspNetCore, Serilog.Sinks.Console, Serilog.Sinks.File |
| **API Docs** | Swashbuckle.AspNetCore |
| **Test Framework** | xUnit, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk |
| **Test Mocking** | Moq |
| **Test Assertions** | FluentAssertions |
| **Integration Testing** | Microsoft.AspNetCore.Mvc.Testing, Testcontainers, Testcontainers.PostgreSql |
| **Architecture Rules** | NetArchTest.Rules |

---

## Why These Packages?

These packages are **industry-standard tools** used in production systems worldwide. They:

1. **Follow best practices** - CQRS, Clean Architecture, SOLID principles
2. **Improve productivity** - Less boilerplate code, more functionality
3. **Enhance maintainability** - Clear separation of concerns, testable code
4. **Enable scalability** - Designed for large, complex applications
5. **Provide reliability** - Battle-tested in production environments
6. **Support testing** - Comprehensive test coverage at all levels

---

## Additional Resources

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [AutoMapper Documentation](https://docs.automapper.org/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Serilog Documentation](https://serilog.net/)
- [xUnit Documentation](https://xunit.net/)
- [Moq Documentation](https://github.com/moq/moq4)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
