# Design Patterns in Hotel Booking System

This document explains the design patterns used in our Hotel Booking System and how they help create maintainable, scalable code.

---

## Table of Contents
1. [Factory Pattern](#factory-pattern)
2. [Repository Pattern](#repository-pattern)
3. [CQRS Pattern](#cqrs-pattern)
4. [Mediator Pattern](#mediator-pattern)
5. [Dependency Injection Pattern](#dependency-injection-pattern)
6. [Builder Pattern](#builder-pattern)

---

## Factory Pattern

### What is it?
A **Factory** is a creational pattern that provides a method for creating objects without specifying the exact class of object that will be created.

### Why use it?
- **Encapsulates object creation logic** - Complex setup code is hidden
- **Promotes reusability** - One place to create objects, used everywhere
- **Easy to maintain** - Change creation logic in one place
- **Testability** - Easy to create test objects with proper configuration

### Example in Our Project

#### InMemoryDbContextFactory

**Location:** `tests/BookingSystem.Service.Hotel.UnitTests/Helpers/InMemoryDbContextFactory.cs`

```csharp
public static class InMemoryDbContextFactory
{
    public static HotelDbContext Create()
    {
        var options = new DbContextOptionsBuilder<HotelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // Unique DB per test
            .Options;

        var context = new HotelDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
```

**Usage in Tests:**
```csharp
[Fact]
public void Test_CreateHotel()
{
    // Instead of repeating configuration code
    var context = InMemoryDbContextFactory.Create();

    // Use the context in your test
    var repository = new HotelRepository(context);
    // ... test logic
}
```

**Benefits:**
- ✅ Clean test code - no repetitive setup
- ✅ Consistent configuration across all tests
- ✅ Each test gets isolated database (Guid.NewGuid())
- ✅ Easy to modify factory if requirements change

#### AutoMapperHelper (Factory for Mapper)

**Location:** `tests/BookingSystem.Service.Hotel.UnitTests/Helpers/AutoMapperHelper.cs`

```csharp
public static class AutoMapperHelper
{
    public static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfile>();
        });

        return configuration.CreateMapper();
    }
}
```

**Usage:**
```csharp
[Fact]
public void Test_HandlerReturnsDto()
{
    var mapper = AutoMapperHelper.CreateMapper();
    var handler = new GetHotelQueryHandler(repository, mapper);
    // ... test logic
}
```

### When to Use Factory Pattern

✅ **Use when:**
- Object creation requires complex setup
- You need consistent object configuration
- You want to hide implementation details
- Creating test objects with specific configurations

❌ **Don't use when:**
- Simple object creation (just use `new`)
- Only one place creates the object
- No complex configuration needed

---

## Repository Pattern

### What is it?
A pattern that **abstracts data access logic** and provides a collection-like interface for accessing domain objects.

### Why use it?
- **Separates business logic from data access** - Clean Architecture
- **Easy to test** - Mock the repository interface
- **Centralized data access** - One place for queries
- **Easy to switch data sources** - Just implement the interface differently

### Example in Our Project

#### IHotelRepository Interface

**Location:** `src/BookingSystem.Service.Hotel.Application/Common/Interfaces/IHotelRepository.cs`

```csharp
public interface IHotelRepository
{
    Task<Domain.Entities.Hotel?> GetByIdAsync(int id);
    Task<List<Domain.Entities.Hotel>> GetAllAsync();
    Task<int> AddAsync(Domain.Entities.Hotel hotel);
    Task UpdateAsync(Domain.Entities.Hotel hotel);
    Task DeleteAsync(int id);
    Task<(List<Domain.Entities.Hotel> Hotels, int TotalCount)> SearchAsync(
        string? city = null,
        string? country = null,
        StarRating? minStarRating = null,
        HotelStatus? status = null,
        int pageNumber = 1,
        int pageSize = 10
    );
}
```

#### HotelRepository Implementation

**Location:** `src/BookingSystem.Service.Hotel.Persistence/Repositories/HotelRepository.cs`

```csharp
public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public HotelRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<int> AddAsync(Domain.Entities.Hotel hotel)
    {
        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();
        return hotel.Id;
    }

    // ... other methods
}
```

**Usage in Handlers:**
```csharp
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository; // ← Depends on interface, not implementation

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = new Domain.Entities.Hotel { /* ... */ };
        return await _repository.AddAsync(hotel);
    }
}
```

**Benefits:**
- ✅ Handlers don't know about EF Core or database
- ✅ Easy to mock in tests
- ✅ Can switch to different database without changing handlers
- ✅ Centralized query logic

### Testing with Repository Pattern

**Without Repository (Difficult to Test):**
```csharp
public class Handler
{
    private readonly HotelDbContext _context;

    public async Task Handle()
    {
        await _context.Hotels.AddAsync(hotel); // ← Hard to test
        await _context.SaveChangesAsync();
    }
}
```

**With Repository (Easy to Test):**
```csharp
[Fact]
public async Task Handler_ShouldCreateHotel()
{
    // Arrange
    var mockRepo = new Mock<IHotelRepository>();
    mockRepo.Setup(r => r.AddAsync(It.IsAny<Hotel>())).ReturnsAsync(1);

    var handler = new CreateHotelCommandHandler(mockRepo.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().Be(1);
    mockRepo.Verify(r => r.AddAsync(It.IsAny<Hotel>()), Times.Once);
}
```

---

## CQRS Pattern

### What is it?
**Command Query Responsibility Segregation** - Separate read operations (Queries) from write operations (Commands).

### Why use it?
- **Clear intent** - Command = change data, Query = read data
- **Easier to maintain** - Each operation is isolated
- **Different optimization strategies** - Optimize reads and writes separately
- **Scalability** - Can scale read and write sides independently

### Example in Our Project

#### Command (Write Operation)

**Location:** `src/BookingSystem.Service.Hotel.Application/Features/Hotels/Commands/CreateHotel/`

```csharp
// The Command
public class CreateHotelCommand : IRequest<int>
{
    public string Name { get; set; }
    public string City { get; set; }
    // ... other properties
}

// The Handler
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository;

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // Write operation - creates/modifies data
        var hotel = new Hotel { /* ... */ };
        return await _repository.AddAsync(hotel);
    }
}
```

#### Query (Read Operation)

**Location:** `src/BookingSystem.Service.Hotel.Application/Features/Hotels/Queries/SearchHotels/`

```csharp
// The Query
public class SearchHotelsQuery : IRequest<PagedResponse<HotelDto>>
{
    public string? City { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

// The Handler
public class SearchHotelsQueryHandler : IRequestHandler<SearchHotelsQuery, PagedResponse<HotelDto>>
{
    private readonly IHotelRepository _repository;
    private readonly IMapper _mapper;

    public async Task<PagedResponse<HotelDto>> Handle(SearchHotelsQuery request, CancellationToken cancellationToken)
    {
        // Read operation - no modifications
        var (hotels, totalCount) = await _repository.SearchAsync(/* ... */);
        var hotelDtos = _mapper.Map<List<HotelDto>>(hotels);
        return new PagedResponse<HotelDto>(hotelDtos, request.PageNumber, request.PageSize, totalCount);
    }
}
```

### Comparison

| Aspect | Command | Query |
|--------|---------|-------|
| **Purpose** | Modify data | Read data |
| **Return Type** | Often `int`, `bool`, or `void` | DTOs, entities, or lists |
| **Side Effects** | Yes (creates, updates, deletes) | No (read-only) |
| **Example** | CreateHotelCommand | SearchHotelsQuery |
| **Validation** | FluentValidation required | Minimal validation |

**Benefits:**
- ✅ Single Responsibility Principle
- ✅ Easy to understand intent
- ✅ Can optimize separately (caching for queries, transactions for commands)
- ✅ Easy to test

---

## Mediator Pattern

### What is it?
A pattern that **reduces direct dependencies** between objects by having them communicate through a mediator.

### Why use it?
- **Decoupling** - Controllers don't know about handlers
- **Single responsibility** - Each handler does one thing
- **Easy to add features** - Just add new commands/queries
- **Pipeline behaviors** - Add cross-cutting concerns (logging, validation)

### Example in Our Project

We use **MediatR** library to implement this pattern.

#### Controller (Sends Request via Mediator)

**Location:** `src/BookingSystem.Service.Hotel.Api/Controllers/HotelsController.cs`

```csharp
[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator; // ← The Mediator

    public HotelsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
    {
        var hotelId = await _mediator.Send(command); // ← Send to mediator
        return CreatedAtAction(nameof(GetHotel), new { id = hotelId }, hotelId);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResponse<HotelDto>>> SearchHotels(
        [FromQuery] string? city,
        [FromQuery] int pageNumber = 1)
    {
        var query = new SearchHotelsQuery { City = city, PageNumber = pageNumber };
        var result = await _mediator.Send(query); // ← Send to mediator
        return Ok(result);
    }
}
```

**Flow:**
```
Controller → IMediator.Send(command) → MediatR → Finds Handler → Handler Executes → Returns Result
```

**Benefits:**
- ✅ Controller doesn't know which handler processes the request
- ✅ Controller doesn't depend on 10 different handler classes
- ✅ Easy to add validation, logging, caching via MediatR pipeline behaviors
- ✅ Testable - can mock IMediator

### Without Mediator (Tightly Coupled)

```csharp
public class HotelsController : ControllerBase
{
    private readonly CreateHotelCommandHandler _createHandler;
    private readonly UpdateHotelCommandHandler _updateHandler;
    private readonly DeleteHotelCommandHandler _deleteHandler;
    private readonly GetHotelQueryHandler _getHandler;
    private readonly SearchHotelsQueryHandler _searchHandler;
    // ... 10 more handlers injected!

    public HotelsController(/* inject all 15 handlers */) { }
}
```

❌ **Problems:**
- Too many dependencies
- Controller knows about all handlers
- Hard to maintain

### With Mediator (Loosely Coupled)

```csharp
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator; // ← Only one dependency!

    public HotelsController(IMediator mediator) { }
}
```

✅ **Benefits:**
- Single dependency
- Add 100 handlers, controller doesn't change
- Clean and maintainable

---

## Dependency Injection Pattern

### What is it?
A pattern where objects receive their dependencies from external sources rather than creating them.

### Why use it?
- **Loose coupling** - Classes depend on interfaces, not implementations
- **Testability** - Easy to inject mocks
- **Flexibility** - Swap implementations without changing code
- **Lifecycle management** - Framework manages object lifetimes

### Example in Our Project

#### Registration (Program.cs)

**Location:** `src/BookingSystem.Service.Hotel.Api/Program.cs`

```csharp
// Register dependencies
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateHotelCommand).Assembly));
builder.Services.AddAutoMapper(typeof(MappingProfile));
```

#### Injection in Handler

```csharp
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository; // ← Injected dependency

    public CreateHotelCommandHandler(IHotelRepository repository) // ← Constructor injection
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // Use the injected dependency
        return await _repository.AddAsync(hotel);
    }
}
```

### Dependency Lifetimes

| Lifetime | Description | When to Use |
|----------|-------------|-------------|
| **Transient** | New instance every time | Lightweight, stateless services |
| **Scoped** | One instance per request | DbContext, repositories |
| **Singleton** | One instance for app lifetime | Configuration, caching |

**In our project:**
```csharp
builder.Services.AddScoped<IHotelRepository, HotelRepository>(); // ← Scoped (per request)
builder.Services.AddDbContext<HotelDbContext>(); // ← Scoped by default
builder.Services.AddSingleton<IConfiguration>(configuration); // ← Singleton
```

### Testing with DI

**Production Code:**
```csharp
var handler = new CreateHotelCommandHandler(realRepository);
```

**Test Code:**
```csharp
var mockRepo = new Mock<IHotelRepository>();
var handler = new CreateHotelCommandHandler(mockRepo.Object); // ← Inject mock
```

---

## Builder Pattern

### What is it?
A pattern for constructing complex objects step by step.

### Example in Our Project

#### DbContextOptionsBuilder (EF Core)

```csharp
var options = new DbContextOptionsBuilder<HotelDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString()) // ← Step 1
    .EnableSensitiveDataLogging() // ← Step 2 (optional)
    .Options; // ← Final build
```

#### MapperConfiguration (AutoMapper)

```csharp
var configuration = new MapperConfiguration(cfg =>
{
    cfg.AddProfile<MappingProfile>(); // ← Step 1
    cfg.AllowNullCollections = true; // ← Step 2 (optional)
});

var mapper = configuration.CreateMapper(); // ← Final build
```

**Benefits:**
- ✅ Readable fluent API
- ✅ Optional configurations
- ✅ Immutable result object
- ✅ Flexible construction

---

## Summary

| Pattern | Purpose | Example in Project |
|---------|---------|-------------------|
| **Factory** | Create objects with complex setup | InMemoryDbContextFactory, AutoMapperHelper |
| **Repository** | Abstract data access | IHotelRepository, HotelRepository |
| **CQRS** | Separate reads from writes | Commands vs Queries |
| **Mediator** | Decouple request senders from handlers | MediatR in controllers |
| **Dependency Injection** | Inject dependencies via constructor | All handlers, repositories, controllers |
| **Builder** | Construct complex objects step-by-step | DbContextOptionsBuilder, MapperConfiguration |

---

## Key Takeaways

1. **Factory Pattern** - Simplifies object creation with complex setup
2. **Repository Pattern** - Keeps data access logic separate and testable
3. **CQRS Pattern** - Clear separation between reads and writes
4. **Mediator Pattern** - Reduces coupling between components
5. **Dependency Injection** - Makes code testable and flexible
6. **Builder Pattern** - Provides fluent API for complex configuration

These patterns work together to create a **maintainable, testable, and scalable** application following **Clean Architecture** principles.