# Core Concepts - Deep Dive

This document explains the fundamental concepts you'll master in Phase 1.

---

## Table of Contents

1. [Clean Architecture](#clean-architecture)
2. [Domain-Driven Design (DDD)](#domain-driven-design-ddd)
3. [CQRS Pattern](#cqrs-pattern)
4. [Repository Pattern](#repository-pattern)
5. [Unit of Work Pattern](#unit-of-work-pattern)
6. [Dependency Injection](#dependency-injection)
7. [Entity Framework Core](#entity-framework-core)
8. [MediatR](#mediatr)
9. [FluentValidation](#fluentvalidation)
10. [AutoMapper](#automapper)

---

## Clean Architecture

### What is Clean Architecture?

Clean Architecture is a software design philosophy that separates concerns into layers, with dependencies flowing inward toward the core business logic.

### Core Principles

**1. Independence of Frameworks**
- Business logic doesn't depend on frameworks
- Easy to change frameworks without affecting core logic

**2. Testability**
- Business rules can be tested without UI, database, or external services

**3. Independence of UI**
- UI can change without changing business logic
- Can support multiple UIs (Web, Mobile, Desktop)

**4. Independence of Database**
- Business rules don't know about the database
- Can swap PostgreSQL for SQL Server without changing core logic

**5. Independence of External Services**
- Business rules don't depend on external APIs or services

### The Dependency Rule

> **Source code dependencies must point only inward, toward higher-level policies.**

```
┌─────────────────────────────────────────────┐
│         External (Frameworks, DB)           │ ← Outer layer
├─────────────────────────────────────────────┤
│    Interface Adapters (Controllers, UI)     │
├─────────────────────────────────────────────┤
│  Application Business Rules (Use Cases)     │
├─────────────────────────────────────────────┤
│  Enterprise Business Rules (Entities)       │ ← Inner layer
└─────────────────────────────────────────────┘
```

### Benefits

✅ **Maintainability**: Changes in one layer don't affect others
✅ **Testability**: Easy to write unit tests without dependencies
✅ **Flexibility**: Can change frameworks, databases, or UI
✅ **Scalability**: Clear separation makes it easier to scale
✅ **Team Collaboration**: Teams can work on different layers independently

### Practical Example

**Bad Approach (Tightly Coupled):**
```csharp
public class HotelController
{
    public IActionResult CreateHotel(CreateHotelRequest request)
    {
        // ❌ Controller directly talks to database
        using var connection = new SqlConnection(connectionString);
        connection.Execute("INSERT INTO Hotels...");

        // ❌ Controller has business logic
        if (request.StarRating > 5)
            return BadRequest();

        return Ok();
    }
}
```

**Good Approach (Clean Architecture):**
```csharp
// Controller (API Layer)
public class HotelController
{
    private readonly IMediator _mediator;

    public async Task<IActionResult> CreateHotel(CreateHotelCommand command)
    {
        // ✅ Controller just delegates to use case
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}

// Handler (Application Layer)
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, HotelDto>
{
    private readonly IHotelRepository _repository;

    public async Task<HotelDto> Handle(CreateHotelCommand request, CancellationToken ct)
    {
        // ✅ Use case orchestrates domain logic
        var hotel = new Hotel(request.Name, request.City);
        await _repository.AddAsync(hotel);
        return MapToDto(hotel);
    }
}

// Entity (Domain Layer)
public class Hotel
{
    public void UpdateStarRating(int rating)
    {
        // ✅ Business rule in domain
        if (rating < 1 || rating > 5)
            throw new InvalidOperationException("Rating must be 1-5");

        StarRating = rating;
    }
}
```

---

## Domain-Driven Design (DDD)

### What is DDD?

Domain-Driven Design is an approach to software development that focuses on understanding and modeling the business domain.

### Key Building Blocks

#### 1. Entity

An object with a unique identity that persists over time.

**Characteristics:**
- Has a unique identifier (ID)
- Identity matters more than attributes
- Mutable (can change over time)

**Example:**
```csharp
public class Hotel : BaseEntity
{
    public int Id { get; private set; }        // Identity
    public string Name { get; private set; }
    public Address Address { get; private set; }

    // Two hotels are the same if they have the same ID
    public override bool Equals(object obj)
    {
        if (obj is Hotel other)
            return Id == other.Id;
        return false;
    }
}
```

#### 2. Value Object

An object defined by its values, not identity.

**Characteristics:**
- No unique identifier
- Immutable (cannot change)
- Two value objects are equal if all attributes match

**Example:**
```csharp
public class Address : ValueObject
{
    public string Street { get; }
    public string City { get; }
    public string Country { get; }

    public Address(string street, string city, string country)
    {
        Street = street;
        City = city;
        Country = country;
    }

    // Two addresses are the same if all values match
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
    }
}
```

**When to use Value Objects:**
- Address, Money, Email, PhoneNumber
- Date ranges, coordinates
- Any concept defined by its attributes

#### 3. Aggregate

A cluster of entities and value objects treated as a single unit.

**Characteristics:**
- Has an aggregate root (main entity)
- Enforces consistency boundaries
- Only aggregate root can be accessed from outside

**Example:**
```csharp
// Hotel is the Aggregate Root
public class Hotel : BaseEntity
{
    private readonly List<RoomType> _roomTypes = new();
    public IReadOnlyCollection<RoomType> RoomTypes => _roomTypes.AsReadOnly();

    // ✅ Add room type through aggregate root
    public void AddRoomType(RoomType roomType)
    {
        // Business rule: No duplicate room types
        if (_roomTypes.Any(rt => rt.Name == roomType.Name))
            throw new InvalidOperationException("Room type already exists");

        _roomTypes.Add(roomType);
    }

    // ❌ Don't allow direct access to collection
    // This ensures invariants are maintained
}
```

#### 4. Domain Service

Business logic that doesn't belong to a single entity.

**When to use:**
- Operation involves multiple entities
- Logic doesn't naturally fit in one entity

**Example:**
```csharp
public class PricingService
{
    public decimal CalculatePrice(Hotel hotel, RoomType roomType, DateTime checkIn, DateTime checkOut)
    {
        // Complex pricing logic involving multiple entities
        var basePrice = roomType.BasePrice;
        var seasonalMultiplier = GetSeasonalMultiplier(checkIn);
        var nights = (checkOut - checkIn).Days;

        return basePrice * seasonalMultiplier * nights;
    }
}
```

#### 5. Repository

Abstraction for accessing aggregates.

**Characteristics:**
- Collection-like interface
- Only for aggregate roots
- Hides persistence details

**Example:**
```csharp
public interface IHotelRepository
{
    Task<Hotel> GetByIdAsync(int id);
    Task<List<Hotel>> GetAllAsync();
    Task AddAsync(Hotel hotel);
    Task UpdateAsync(Hotel hotel);
    Task DeleteAsync(int id);
}
```

---

## CQRS Pattern

### What is CQRS?

**Command Query Responsibility Segregation** - Separate read operations from write operations.

### The Principle

> Every method should either be a command that performs an action, or a query that returns data, but not both.

### Commands (Write Operations)

**Characteristics:**
- Modify state
- Return simple results (void, ID, success/failure)
- Can have side effects
- Named as imperatives: CreateHotel, UpdateHotel, DeleteHotel

**Example:**
```csharp
// Command
public class CreateHotelCommand : IRequest<CreateHotelResponse>
{
    public string Name { get; set; }
    public string City { get; set; }
    public int StarRating { get; set; }
}

// Response
public class CreateHotelResponse
{
    public int HotelId { get; set; }
    public string Message { get; set; }
}

// Handler
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, CreateHotelResponse>
{
    private readonly IHotelRepository _repository;

    public async Task<CreateHotelResponse> Handle(CreateHotelCommand request, CancellationToken ct)
    {
        var hotel = new Hotel(request.Name, request.City, request.StarRating);
        await _repository.AddAsync(hotel);

        return new CreateHotelResponse
        {
            HotelId = hotel.Id,
            Message = "Hotel created successfully"
        };
    }
}
```

### Queries (Read Operations)

**Characteristics:**
- Read data only
- Return rich data (DTOs)
- No side effects
- Can be optimized for reads

**Example:**
```csharp
// Query
public class GetHotelByIdQuery : IRequest<HotelDto>
{
    public int HotelId { get; set; }
}

// DTO (Data Transfer Object)
public class HotelDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public int StarRating { get; set; }
    public List<RoomTypeDto> RoomTypes { get; set; }
}

// Handler
public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    private readonly IHotelRepository _repository;
    private readonly IMapper _mapper;

    public async Task<HotelDto> Handle(GetHotelByIdQuery request, CancellationToken ct)
    {
        var hotel = await _repository.GetByIdAsync(request.HotelId);

        if (hotel == null)
            throw new NotFoundException($"Hotel with ID {request.HotelId} not found");

        return _mapper.Map<HotelDto>(hotel);
    }
}
```

### Benefits of CQRS

✅ **Clear Intent**: Commands and queries have different purposes
✅ **Optimization**: Can optimize reads and writes separately
✅ **Scalability**: Can scale read and write sides independently
✅ **Simplicity**: Each handler has one responsibility
✅ **Security**: Easier to implement different permissions for read/write

---

## Repository Pattern

### What is the Repository Pattern?

An abstraction that mediates between the domain and data mapping layers, providing a collection-like interface for accessing domain objects.

### Purpose

1. **Decouple** business logic from data access
2. **Centralize** data access logic
3. **Testability** - easy to mock in tests
4. **Consistency** - uniform API for data operations

### Generic Repository

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}
```

### Specific Repository

```csharp
public interface IHotelRepository : IRepository<Hotel>
{
    // Add hotel-specific methods
    Task<Hotel> GetByNameAsync(string name);
    Task<List<Hotel>> GetByCityAsync(string city);
    Task<List<Hotel>> GetByStarRatingAsync(int rating);
}
```

### Implementation

```csharp
public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public HotelRepository(HotelDbContext context) : base(context)
    {
    }

    public async Task<Hotel> GetByNameAsync(string name)
    {
        return await _context.Hotels
            .Include(h => h.RoomTypes)
            .FirstOrDefaultAsync(h => h.Name == name);
    }

    public async Task<List<Hotel>> GetByCityAsync(string city)
    {
        return await _context.Hotels
            .Where(h => h.Address.City == city)
            .ToListAsync();
    }
}
```

---

## Dependency Injection

### What is Dependency Injection?

A design pattern where objects receive their dependencies from external sources rather than creating them.

### Types of Injection

**1. Constructor Injection (Recommended)**
```csharp
public class CreateHotelCommandHandler
{
    private readonly IHotelRepository _repository;
    private readonly ILogger<CreateHotelCommandHandler> _logger;

    // Dependencies injected via constructor
    public CreateHotelCommandHandler(
        IHotelRepository repository,
        ILogger<CreateHotelCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

### Service Lifetimes

**1. Transient**
- Created each time requested
- Use for lightweight, stateless services

```csharp
services.AddTransient<IEmailService, EmailService>();
```

**2. Scoped**
- Created once per request (HTTP request in web apps)
- Use for repositories, DbContext

```csharp
services.AddScoped<IHotelRepository, HotelRepository>();
services.AddScoped<HotelDbContext>();
```

**3. Singleton**
- Created once for application lifetime
- Use for stateless services, configuration

```csharp
services.AddSingleton<ICacheService, CacheService>();
```

---

## Entity Framework Core

### What is EF Core?

Entity Framework Core is an Object-Relational Mapper (ORM) that enables .NET developers to work with databases using .NET objects.

### DbContext

The main class that coordinates EF functionality for a data model.

```csharp
public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
```

### Fluent API Configuration

```csharp
public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(h => h.Description)
            .HasMaxLength(2000);

        // Value object mapping
        builder.OwnsOne(h => h.Address, address =>
        {
            address.Property(a => a.Street).HasColumnName("Street").HasMaxLength(200);
            address.Property(a => a.City).HasColumnName("City").HasMaxLength(100).IsRequired();
            address.Property(a => a.Country).HasColumnName("Country").HasMaxLength(100).IsRequired();
            address.Property(a => a.ZipCode).HasColumnName("ZipCode").HasMaxLength(20);
        });

        // One-to-many relationship
        builder.HasMany(h => h.RoomTypes)
            .WithOne()
            .HasForeignKey(rt => rt.HotelId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(h => h.Name);
        builder.HasIndex(h => new { h.Name, h.City }).IsUnique();

        // Soft delete filter
        builder.HasQueryFilter(h => !h.IsDeleted);
    }
}
```

---

## MediatR

### What is MediatR?

A library that implements the Mediator pattern, reducing dependencies between objects.

### Benefits

✅ **Decoupling**: Request and handler are separate
✅ **Single Responsibility**: Each handler does one thing
✅ **Pipeline Behaviors**: Add cross-cutting concerns (logging, validation)

### Request/Response

```csharp
// Request
public class GetHotelByIdQuery : IRequest<HotelDto>
{
    public int HotelId { get; set; }
}

// Handler
public class GetHotelByIdQueryHandler : IRequestHandler<GetHotelByIdQuery, HotelDto>
{
    public async Task<HotelDto> Handle(GetHotelByIdQuery request, CancellationToken ct)
    {
        // Handle the request
    }
}

// Usage in controller
var result = await _mediator.Send(new GetHotelByIdQuery { HotelId = 1 });
```

---

## FluentValidation

### What is FluentValidation?

A library for building strongly-typed validation rules.

### Example

```csharp
public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hotel name is required")
            .MaximumLength(200).WithMessage("Hotel name must not exceed 200 characters");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("City is required")
            .MaximumLength(100).WithMessage("City must not exceed 100 characters");

        RuleFor(x => x.StarRating)
            .InclusiveBetween(1, 5).WithMessage("Star rating must be between 1 and 5");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Invalid email format");
    }
}
```

---

## AutoMapper

### What is AutoMapper?

A library that simplifies object-to-object mapping.

### Mapping Profile

```csharp
public class HotelMappingProfile : Profile
{
    public HotelMappingProfile()
    {
        // Entity to DTO
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
            .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Address.Country));

        // Command to Entity
        CreateMap<CreateHotelCommand, Hotel>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

        // Nested mapping
        CreateMap<RoomType, RoomTypeDto>();
    }
}
```

---

## Summary

You've learned the core concepts that form the foundation of modern .NET backend development:

1. **Clean Architecture** - Organizing code for maintainability
2. **DDD** - Modeling the business domain
3. **CQRS** - Separating reads from writes
4. **Repository Pattern** - Abstracting data access
5. **Dependency Injection** - Loose coupling
6. **EF Core** - Working with databases
7. **MediatR** - Implementing CQRS
8. **FluentValidation** - Input validation
9. **AutoMapper** - Object mapping

Now you're ready to implement these concepts in code! 🚀
