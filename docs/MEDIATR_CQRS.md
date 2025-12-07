# MediatR and CQRS Pattern Guide

This document explains how MediatR works, CQRS pattern, and how requests are routed to handlers.

---

## Table of Contents

1. [What is CQRS?](#what-is-cqrs)
2. [What is MediatR?](#what-is-mediatr)
3. [How MediatR Works](#how-mediatr-works)
4. [Registration and Discovery](#registration-and-discovery)
5. [Request Flow](#request-flow)
6. [Commands vs Queries](#commands-vs-queries)
7. [Real Examples](#real-examples)
8. [Validation Pipeline](#validation-pipeline)
9. [Best Practices](#best-practices)

---

## What is CQRS?

**CQRS** = **Command Query Responsibility Segregation**

### Core Principle:
Separate operations that **change data** (Commands) from operations that **read data** (Queries).

### Why Use CQRS?

**Without CQRS (Traditional Service Layer):**
```csharp
public class HotelService
{
    public Hotel GetHotel(int id) { }           // Read
    public List<Hotel> GetAllHotels() { }       // Read
    public void CreateHotel(Hotel hotel) { }    // Write
    public void UpdateHotel(Hotel hotel) { }    // Write
    public void DeleteHotel(int id) { }         // Write
}

// Problems:
// - All operations mixed together
// - Hard to apply different rules to reads vs writes
// - Can't optimize reads and writes independently
// - Testing requires mocking entire service
```

**With CQRS:**
```csharp
// Commands (Write operations)
public class CreateHotelCommand : IRequest<int> { }
public class UpdateHotelCommand : IRequest { }
public class DeleteHotelCommand : IRequest { }

// Queries (Read operations)
public class GetHotelByIdQuery : IRequest<HotelDto> { }
public class GetHotelsListQuery : IRequest<List<HotelDto>> { }

// Benefits:
// ✅ Clear separation
// ✅ Each operation has its own class and handler
// ✅ Easy to test in isolation
// ✅ Can optimize reads and writes differently
// ✅ Single Responsibility Principle
```

---

## What is MediatR?

**MediatR** is a simple mediator pattern implementation that implements the **Request/Response** pattern.

### Core Concepts:

1. **Request** (Command or Query) - What you want to do
2. **Handler** - How to do it
3. **Mediator** - Routes requests to handlers

### Simple Analogy:

Think of MediatR like a **post office**:
- You send a **letter** (Request/Command/Query)
- The **post office** (MediatR) knows where to deliver it
- The **recipient** (Handler) processes the letter
- The recipient sends back a **response**

---

## How MediatR Works

### The Flow:

```
┌─────────────┐
│   Client    │  (Controller, Endpoint)
│  (API Layer)│
└──────┬──────┘
       │ 1. Send command/query
       ↓
┌─────────────────────────────────┐
│        MediatR.Send()           │
│  (Mediator - Routing Logic)     │
└──────┬──────────────────────────┘
       │ 2. Find matching handler
       ↓
┌─────────────────────────────────┐
│   Handler.Handle()              │
│  (Business Logic)               │
└──────┬──────────────────────────┘
       │ 3. Return result
       ↓
┌─────────────┐
│   Client    │  Receives response
└─────────────┘
```

### Code Example:

```csharp
// 1. REQUEST (Command or Query)
public class CreateHotelCommand : IRequest<int>
{
    public string Name { get; set; }
    public string City { get; set; }
}

// 2. HANDLER
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // Business logic
        var hotel = new Hotel(request.Name, request.City);
        await _repository.AddAsync(hotel);
        return hotel.Id;
    }
}

// 3. USAGE (in API layer)
[HttpPost]
public async Task<IActionResult> CreateHotel([FromBody] CreateHotelCommand command)
{
    var hotelId = await _mediator.Send(command);  // MediatR routes to handler
    return Ok(hotelId);
}
```

---

## Registration and Discovery

### How MediatR Finds Handlers

MediatR uses **reflection** to automatically discover commands/queries and their handlers.

### Registration Code:

**Application Layer - DependencyInjection.cs:**
```csharp
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Service.Hotel.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR - scans assembly for handlers
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Register AutoMapper
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

**API Layer - Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Register Application services (MediatR, AutoMapper, Validators)
builder.Services.AddApplication();

// ... other registrations
```

---

### What Happens During Registration?

**At Startup (Application Start):**

1. **MediatR scans the Application assembly**
   ```csharp
   Assembly.GetExecutingAssembly()  // Application layer assembly
   ```

2. **Finds all Requests (Commands/Queries)**
   - Any class implementing `IRequest<T>`
   - Example: `CreateHotelCommand : IRequest<int>`

3. **Finds all Handlers**
   - Any class implementing `IRequestHandler<TRequest, TResponse>`
   - Example: `CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>`

4. **Registers mappings in DI container**
   ```
   CreateHotelCommand → CreateHotelCommandHandler
   GetHotelByIdQuery → GetHotelByIdQueryHandler
   GetHotelsListQuery → GetHotelsListQueryHandler
   ```

---

### Discovery Example:

**MediatR finds:**
```csharp
// Command
public class CreateHotelCommand : IRequest<int> { }

// Handler
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int> { }
```

**MediatR registers:**
```
IRequestHandler<CreateHotelCommand, int> → CreateHotelCommandHandler
```

**At runtime when you call:**
```csharp
await _mediator.Send(new CreateHotelCommand { Name = "Hilton" });
```

**MediatR does:**
1. Looks up handler for `CreateHotelCommand`
2. Finds `CreateHotelCommandHandler`
3. Resolves handler from DI container (with all dependencies)
4. Calls `handler.Handle(command, cancellationToken)`
5. Returns result

---

## Request Flow

### Complete Flow with Dependency Injection:

```csharp
// 1. CLIENT (API Controller)
[ApiController]
[Route("api/hotels")]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;

    // Mediator injected by DI
    public HotelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateHotel([FromBody] CreateHotelCommand command)
    {
        // Send command to MediatR
        var hotelId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetHotel), new { id = hotelId }, hotelId);
    }
}

// 2. MEDIATR (Routing)
// MediatR receives: CreateHotelCommand
// MediatR looks up: IRequestHandler<CreateHotelCommand, int>
// MediatR finds: CreateHotelCommandHandler
// MediatR resolves from DI with dependencies

// 3. HANDLER (Business Logic)
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository;

    // Dependencies injected by DI
    public CreateHotelCommandHandler(IHotelRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // Create domain entity
        var hotel = new Domain.Entities.Hotel(
            request.Name,
            request.Description,
            request.StarRating,
            request.City,
            request.Country
        );

        // Save to database
        var createdHotel = await _repository.AddAsync(hotel);

        // Return result
        return createdHotel.Id;
    }
}

// 4. RESULT
// Handler returns: int (hotel ID)
// MediatR returns to: Controller
// Controller returns: HTTP 201 Created with hotel ID
```

---

## Commands vs Queries

### Commands (Write Operations)

**Purpose:** Change system state (Create, Update, Delete)

**Characteristics:**
- Modifies data
- Can have side effects
- Returns simple result (ID, success, void)
- Should be idempotent when possible

**Example:**
```csharp
public class CreateHotelCommand : IRequest<int>
{
    public string Name { get; set; }
    public string City { get; set; }
    // ... other properties
}

public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = new Hotel(request.Name, request.City);
        await _repository.AddAsync(hotel);
        return hotel.Id;  // Returns new hotel ID
    }
}
```

**Other Command Examples:**
- `UpdateHotelCommand : IRequest` (returns void)
- `DeleteHotelCommand : IRequest<bool>` (returns success)
- `UpdateHotelPriceCommand : IRequest<decimal>` (returns new price)

---

### Queries (Read Operations)

**Purpose:** Read data without changing state

**Characteristics:**
- Only reads data
- No side effects
- Returns DTOs (Data Transfer Objects)
- Can be cached
- Safe to retry

**Example:**
```csharp
public class GetHotelByIdQuery : IRequest<HotelDto>
{
    public int HotelId { get; set; }
}

public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    public async Task<HotelDto> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _repository.GetByIdAsync(request.HotelId);

        if (hotel == null)
            throw new HotelNotFoundException(request.HotelId);

        return _mapper.Map<HotelDto>(hotel);  // Returns DTO
    }
}
```

**Other Query Examples:**
- `GetHotelsListQuery : IRequest<List<HotelDto>>`
- `SearchHotelsQuery : IRequest<PagedResult<HotelDto>>`
- `GetHotelsByCity : IRequest<List<HotelDto>>`

---

### Comparison Table:

| Feature | Command | Query |
|---------|---------|-------|
| **Purpose** | Change data | Read data |
| **Side Effects** | Yes | No |
| **Return Type** | Simple (int, bool, void) | Complex (DTO, List) |
| **Cacheable** | No | Yes |
| **Safe to Retry** | Sometimes | Always |
| **Example** | CreateHotel, UpdateHotel | GetHotel, GetHotelsList |

---

## Real Examples

### Example 1: Create Hotel (Command)

```csharp
// 1. Command
public class CreateHotelCommand : IRequest<int>
{
    public string Name { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

// 2. Validator
public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.City).NotEmpty();
        RuleFor(x => x.Country).NotEmpty();
    }
}

// 3. Handler
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository;

    public CreateHotelCommandHandler(IHotelRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = new Hotel(request.Name, request.City, request.Country);
        var created = await _repository.AddAsync(hotel);
        return created.Id;
    }
}

// 4. Usage in API
[HttpPost]
public async Task<IActionResult> CreateHotel([FromBody] CreateHotelCommand command)
{
    var hotelId = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetHotel), new { id = hotelId }, hotelId);
}
```

---

### Example 2: Get Hotel (Query)

```csharp
// 1. Query
public class GetHotelByIdQuery : IRequest<HotelDto>
{
    public int HotelId { get; set; }
}

// 2. Handler
public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    private readonly IHotelRepository _repository;
    private readonly IMapper _mapper;

    public GetHotelByIdQueryHandler(IHotelRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<HotelDto> Handle(GetHotelByIdQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _repository.GetByIdAsync(request.HotelId);

        if (hotel == null)
            throw new HotelNotFoundException(request.HotelId);

        return _mapper.Map<HotelDto>(hotel);
    }
}

// 3. Usage in API
[HttpGet("{id}")]
public async Task<IActionResult> GetHotel(int id)
{
    var query = new GetHotelByIdQuery { HotelId = id };
    var hotel = await _mediator.Send(query);
    return Ok(hotel);
}
```

---

## Validation Pipeline

You can add **validation behavior** to automatically validate all requests before they reach handlers.

### Validation Behavior:

```csharp
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(failures);
        }

        return await next();
    }
}
```

### Register in DependencyInjection:

```csharp
services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
});
```

### Flow with Validation:

```
Request → ValidationBehavior → Handler → Response
            ↓
         Validates
            ↓
      If invalid: throw ValidationException
      If valid: continue to handler
```

---

## Best Practices

### 1. One Handler Per Request

```csharp
// ✅ GOOD: One request, one handler
public class CreateHotelCommand : IRequest<int> { }
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int> { }

// ❌ BAD: Multiple handlers for same request (not supported)
public class AnotherCreateHotelHandler : IRequestHandler<CreateHotelCommand, int> { }
```

---

### 2. Keep Handlers Focused

```csharp
// ✅ GOOD: Handler does one thing
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    public async Task<int> Handle(...)
    {
        var hotel = new Hotel(...);
        await _repository.AddAsync(hotel);
        return hotel.Id;
    }
}

// ❌ BAD: Handler does too much
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    public async Task<int> Handle(...)
    {
        var hotel = new Hotel(...);
        await _repository.AddAsync(hotel);
        await _emailService.SendWelcomeEmail(hotel);
        await _searchService.IndexHotel(hotel);
        await _cacheService.InvalidateCache();
        return hotel.Id;
    }
}
// Use domain events or message bus for side effects instead
```

---

### 3. Commands Should Not Return Complex Data

```csharp
// ✅ GOOD: Returns simple value
public class CreateHotelCommand : IRequest<int> { }  // Returns ID

// ✅ GOOD: Returns success/failure
public class UpdateHotelCommand : IRequest<bool> { }

// ❌ BAD: Returns complex object
public class CreateHotelCommand : IRequest<HotelDto> { }  // Use Query instead
```

---

### 4. Queries Should Not Modify Data

```csharp
// ✅ GOOD: Only reads data
public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    public async Task<HotelDto> Handle(...)
    {
        var hotel = await _repository.GetByIdAsync(...);
        return _mapper.Map<HotelDto>(hotel);
    }
}

// ❌ BAD: Modifies data in query
public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    public async Task<HotelDto> Handle(...)
    {
        var hotel = await _repository.GetByIdAsync(...);
        hotel.IncrementViewCount();  // ❌ Modifying data in query!
        await _repository.UpdateAsync(hotel);
        return _mapper.Map<HotelDto>(hotel);
    }
}
```

---

### 5. Use Meaningful Names

```csharp
// ✅ GOOD: Clear intention
CreateHotelCommand
GetHotelByIdQuery
UpdateHotelPriceCommand
SearchHotelsByCityQuery

// ❌ BAD: Vague names
HotelCommand
HotelQuery
ProcessHotelRequest
```

---

## Summary

### Key Concepts:

| Concept | Description |
|---------|-------------|
| **CQRS** | Separate reads (Queries) from writes (Commands) |
| **MediatR** | Mediator pattern implementation for request/response |
| **Command** | Write operation that changes data |
| **Query** | Read operation that returns data |
| **Handler** | Processes a specific command or query |
| **Registration** | MediatR automatically discovers handlers via reflection |

### Benefits:

- ✅ **Separation of Concerns** - Each operation is isolated
- ✅ **Testability** - Easy to unit test handlers
- ✅ **Flexibility** - Can optimize reads and writes independently
- ✅ **Maintainability** - Easy to add new operations
- ✅ **Single Responsibility** - Each handler does one thing

---

## Further Reading

- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [CQRS Pattern by Martin Fowler](https://martinfowler.com/bliki/CQRS.html)
- [Clean Architecture with CQRS](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/apply-simplified-microservice-cqrs-ddd-patterns)