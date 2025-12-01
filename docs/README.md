# Hotel Service - Phase 1 Learning Guide

## Overview

Welcome to Phase 1! You're building the **Hotel Service** - the foundation that will teach you Clean Architecture, CQRS, and production-ready patterns.

**What You'll Build:**
- Hotel entity with full CRUD operations
- RoomType entity linked to hotels
- Clean Architecture with proper layer separation
- CQRS pattern using MediatR
- PostgreSQL database with Entity Framework Core
- Complete test suite

**Technologies:**
- .NET 8 Web API
- PostgreSQL
- Entity Framework Core
- MediatR (CQRS)
- FluentValidation
- AutoMapper
- Swagger/OpenAPI
- xUnit (Testing)

---

## Project Structure

```
BookingSystem.Service.Hotel/
├── src/
│   ├── Hotel.Api/                          # 🌐 Entry point
│   │   ├── Controllers/ or Endpoints/      # HTTP endpoints
│   │   ├── Middleware/                     # Custom middleware
│   │   ├── Program.cs                      # App configuration
│   │   └── appsettings.json                # Configuration
│   │
│   ├── Hotel.Application/                  # 🎯 Use Cases (CQRS)
│   │   ├── Features/
│   │   │   ├── Hotels/
│   │   │   │   ├── Commands/               # Write operations
│   │   │   │   │   ├── CreateHotel/
│   │   │   │   │   ├── UpdateHotel/
│   │   │   │   │   └── DeleteHotel/
│   │   │   │   └── Queries/                # Read operations
│   │   │   │       ├── GetHotelById/
│   │   │   │       ├── GetHotelsList/
│   │   │   │       └── SearchHotels/
│   │   │   └── RoomTypes/
│   │   ├── Common/
│   │   │   ├── Interfaces/                 # Repository interfaces
│   │   │   ├── Mappings/                   # AutoMapper profiles
│   │   │   └── Behaviors/                  # MediatR behaviors
│   │   └── DTOs/                           # Data Transfer Objects
│   │
│   ├── Hotel.Domain/                       # 💎 Core Business Logic
│   │   ├── Entities/
│   │   │   ├── Hotel.cs
│   │   │   ├── RoomType.cs
│   │   │   ├── Amenity.cs
│   │   │   └── BaseEntity.cs
│   │   ├── ValueObjects/
│   │   │   ├── Address.cs
│   │   │   └── ContactInfo.cs
│   │   ├── Enums/
│   │   │   ├── HotelStatus.cs
│   │   │   └── StarRating.cs
│   │   └── Exceptions/
│   │       └── HotelNotFoundException.cs
│   │
│   ├── Hotel.Persistence/                  # 🗄️ Database Access
│   │   ├── Contexts/
│   │   │   └── HotelDbContext.cs
│   │   ├── Configurations/                 # EF Core configurations
│   │   │   ├── HotelConfiguration.cs
│   │   │   └── RoomTypeConfiguration.cs
│   │   ├── Repositories/
│   │   │   ├── HotelRepository.cs
│   │   │   └── RoomTypeRepository.cs
│   │   ├── Migrations/                     # EF Core migrations
│   │   └── Seeds/                          # Seed data
│   │
│   └── Hotel.Infrastructure/               # 🔌 External Services
│       ├── Services/
│       │   └── DateTimeService.cs
│       └── HttpClients/
│
└── tests/
    ├── Hotel.UnitTests/                    # Unit tests
    ├── Hotel.IntegrationTests/             # API tests
    └── Hotel.ArchitectureTests/            # Architecture rules
```

---

## Layer Responsibilities

### 1. Domain Layer (Core)
**Purpose:** Pure business logic and entities

**Contains:**
- Entities (Hotel, RoomType)
- Value Objects (Address, ContactInfo)
- Enums
- Domain exceptions
- Business rules

**Rules:**
- ❌ NO dependencies on other layers
- ❌ NO framework dependencies
- ✅ Only C# and business logic

**Example:**
```csharp
// Hotel entity with business rules
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }

    // Business rule enforced in domain
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name cannot be empty");

        Name = name;
    }
}
```

---

### 2. Application Layer (Use Cases)
**Purpose:** Orchestrate business operations

**Contains:**
- Commands (write operations)
- Queries (read operations)
- Command/Query Handlers
- DTOs (Data Transfer Objects)
- Validators
- Mapping profiles
- Repository interfaces

**Depends on:** Domain only

**Pattern:** CQRS (Command Query Responsibility Segregation)

**Example:**
```csharp
// Command
public class CreateHotelCommand : IRequest<CreateHotelResponse>
{
    public string Name { get; set; }
    public string City { get; set; }
    public int StarRating { get; set; }
}

// Handler
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, CreateHotelResponse>
{
    private readonly IHotelRepository _repository;

    public async Task<CreateHotelResponse> Handle(...)
    {
        var hotel = new Hotel(command.Name, command.City);
        await _repository.AddAsync(hotel);
        return new CreateHotelResponse { HotelId = hotel.Id };
    }
}
```

---

### 3. Persistence Layer (Data Access)
**Purpose:** Database operations

**Contains:**
- DbContext (EF Core)
- Entity configurations
- Repository implementations
- Migrations
- Seed data

**Depends on:** Domain, Application (for interfaces)

**Example:**
```csharp
// EF Core configuration
public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);

        // Value object mapping
        builder.OwnsOne(h => h.Address, address =>
        {
            address.Property(a => a.City).HasColumnName("City");
            address.Property(a => a.Country).HasColumnName("Country");
        });
    }
}
```

---

### 4. Infrastructure Layer (External)
**Purpose:** External services and integrations

**Contains:**
- External API clients
- Email services
- File storage
- Third-party integrations

**Depends on:** Application

---

### 5. API Layer (Entry Point)
**Purpose:** HTTP endpoints and configuration

**Contains:**
- Controllers or Minimal API endpoints
- Middleware
- Filters
- Dependency Injection setup
- Configuration

**Depends on:** All layers

**Example:**
```csharp
// Minimal API endpoint
app.MapPost("/api/hotels", async (
    CreateHotelCommand command,
    IMediator mediator) =>
{
    var response = await mediator.Send(command);
    return Results.Created($"/api/hotels/{response.HotelId}", response);
});
```

---

## Key Concepts

### CQRS (Command Query Responsibility Segregation)

**Principle:** Separate reads from writes

**Commands (Write):**
- Modify data
- Return simple result (ID, success)
- Can have side effects
- Examples: CreateHotel, UpdateHotel, DeleteHotel

**Queries (Read):**
- Read data only
- Return DTOs
- No side effects
- Examples: GetHotelById, GetHotelsList

**Benefits:**
- Clear separation of concerns
- Easier to optimize (different models for read/write)
- Better testability
- Scalability (can scale reads/writes independently)

---

### Repository Pattern

**Purpose:** Abstraction over data access

**Benefits:**
- Testability (easy to mock)
- Consistent API for data operations
- Can swap implementations

**Example:**
```csharp
// Interface (in Application layer)
public interface IHotelRepository
{
    Task<Hotel> GetByIdAsync(int id);
    Task<List<Hotel>> GetAllAsync();
    Task<Hotel> AddAsync(Hotel hotel);
    Task UpdateAsync(Hotel hotel);
    Task DeleteAsync(int id);
}

// Implementation (in Persistence layer)
public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public async Task<Hotel> GetByIdAsync(int id)
    {
        return await _context.Hotels
            .Include(h => h.RoomTypes)
            .FirstOrDefaultAsync(h => h.Id == id);
    }
}
```

---

### Value Objects

**What:** Immutable objects defined by their values, not identity

**When to use:**
- Address (Street, City, Country, ZipCode)
- Money (Amount, Currency)
- Email, Phone number

**Benefits:**
- Encapsulates validation
- Reusable across entities
- Immutable = thread-safe

**Example:**
```csharp
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }

    public Address(string street, string city, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required");

        Street = street;
        City = city;
        Country = country;
        ZipCode = zipCode;
    }

    // Two addresses are equal if all values match
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
        yield return ZipCode;
    }
}
```

---

## Implementation Checklist

### Phase 1.1: Project Setup
- [ ] Create project structure
- [ ] Add project references
- [ ] Install NuGet packages
- [ ] Configure Docker for PostgreSQL

### Phase 1.2: Domain Layer
- [ ] Create BaseEntity
- [ ] Create Hotel entity
- [ ] Create RoomType entity
- [ ] Create Address value object
- [ ] Create ContactInfo value object
- [ ] Create enums (HotelStatus, StarRating)
- [ ] Create domain exceptions

### Phase 1.3: Application Layer
- [ ] Install MediatR, FluentValidation, AutoMapper
- [ ] Define repository interfaces
- [ ] Create CreateHotelCommand + Handler + Validator
- [ ] Create UpdateHotelCommand + Handler + Validator
- [ ] Create DeleteHotelCommand + Handler
- [ ] Create GetHotelByIdQuery + Handler
- [ ] Create GetHotelsListQuery + Handler
- [ ] Create AutoMapper profiles
- [ ] Create DTOs

### Phase 1.4: Persistence Layer
- [ ] Install EF Core packages
- [ ] Create HotelDbContext
- [ ] Configure entities with Fluent API
- [ ] Implement repositories
- [ ] Create initial migration
- [ ] Add seed data

### Phase 1.5: API Layer
- [ ] Configure Program.cs
- [ ] Add dependency injection
- [ ] Create endpoints (Minimal API or Controllers)
- [ ] Configure Swagger
- [ ] Add middleware
- [ ] Configure logging (Serilog)

### Phase 1.6: Testing
- [ ] Write unit tests for handlers
- [ ] Write unit tests for validators
- [ ] Write integration tests for API
- [ ] Write architecture tests

### Phase 1.7: Docker
- [ ] Create Dockerfile
- [ ] Create docker-compose.yml
- [ ] Test local deployment

---

## Next Steps

After completing Phase 1, you'll move to Phase 2: Inventory Service, where you'll learn:
- Redis caching
- Distributed locking
- Hangfire background jobs
- Concurrency control

---

## Resources

- [Clean Architecture (Uncle Bob)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
