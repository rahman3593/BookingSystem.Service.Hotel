# Domain-Driven Design (DDD) Guide

This document explains Domain-Driven Design concepts and how they apply to our Hotel Booking System.

---

## Table of Contents

1. [What is Domain-Driven Design?](#what-is-domain-driven-design)
2. [DDD Building Blocks](#ddd-building-blocks)
3. [Entities vs Value Objects](#entities-vs-value-objects)
4. [Aggregate Roots](#aggregate-roots)
5. [Domain Services](#domain-services)
6. [Domain Events](#domain-events)
7. [Repositories](#repositories)
8. [DDD in Clean Architecture](#ddd-in-clean-architecture)
9. [Real Examples from Our System](#real-examples-from-our-system)

---

## What is Domain-Driven Design?

**Domain-Driven Design (DDD)** is an approach to software development that focuses on:

1. **Understanding the business domain** - The real-world problem you're solving
2. **Modeling that domain in code** - Using rich, behavior-focused objects
3. **Using Ubiquitous Language** - Same terminology as business experts use

### Core Principle:
Your code should reflect how the business thinks and talks about the problem.

### Example:
```
Business Expert: "A hotel has multiple room types. Each room type has a base price."
Code:
public class Hotel {
    private List<RoomType> _roomTypes;
}
```

---

## DDD Building Blocks

DDD defines several types of domain objects:

| Building Block | Description | Example |
|---------------|-------------|---------|
| **Entity** | Object with unique identity | Hotel, Booking, User |
| **Value Object** | Object without identity | Address, Money, Email |
| **Aggregate Root** | Main entity controlling access | Hotel (controls RoomTypes) |
| **Domain Service** | Logic that doesn't fit entities | PricingService |
| **Domain Event** | Something that happened | HotelCreatedEvent |
| **Repository** | Persistence abstraction | IHotelRepository |

---

## Entities vs Value Objects

### Entity

**Definition:** An object that has a unique identity and persists over time.

**Characteristics:**
- ✅ Has an `Id` property (identity)
- ✅ Identity persists even if properties change
- ✅ Mutable (properties can change)
- ✅ Equality based on Id
- ✅ Has a lifecycle (created, modified, deleted)

**Examples:**
```csharp
// Hotel is an ENTITY
public class Hotel : BaseEntity
{
    public int Id { get; protected set; }        // ← Identity
    public string Name { get; private set; }     // Can change
    public Address Address { get; private set; } // Can change

    public void UpdateName(string newName)
    {
        Name = newName;  // Hotel is still the same hotel (same Id)
        UpdateTimestamp();
    }
}

// Usage:
var hotel1 = new Hotel { Id = 1, Name = "Hilton" };
var hotel2 = new Hotel { Id = 2, Name = "Hilton" };

// Different entities (different Ids)
hotel1 == hotel2  // False

// Change name, still the same hotel
hotel1.UpdateName("Hilton Grand");
hotel1.Id  // Still 1 (same entity)
```

**Real-World Entity Examples:**
- **Hotel** - Each hotel has unique identity
- **Booking** - Each booking is unique
- **User** - Each user is unique
- **RoomType** - Each room type is unique
- **Payment** - Each payment transaction is unique

---

### Value Object

**Definition:** An object that represents a descriptive aspect of the domain, with no identity.

**Characteristics:**
- ❌ No `Id` property (no identity)
- ✅ Immutable (cannot be changed after creation)
- ✅ Equality based on values (all properties)
- ✅ Interchangeable (two objects with same values are identical)

**Examples:**
```csharp
// Address is a VALUE OBJECT
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    // Immutable - set only in constructor
    public Address(string street, string city, string country)
    {
        Street = street;
        City = city;
        Country = country;
    }

    // No methods to change values!
    // To change, must create new Address object

    // Value equality
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
    }
}

// Usage:
var address1 = new Address("123 Main St", "Paris", "France");
var address2 = new Address("123 Main St", "Paris", "France");

// Same value object (same values)
address1 == address2  // True

// Cannot modify:
// address1.City = "London";  ❌ No setter!

// Must create new object:
var newAddress = new Address("123 Main St", "London", "France");  ✅
```

**Real-World Value Object Examples:**
- **Address** - "123 Main St, Paris, France" is the same address everywhere
- **Money** - "$100 USD" is the same value everywhere
- **Email** - "user@example.com" is the same email everywhere
- **DateRange** - "Jan 1 - Jan 5" is the same range everywhere
- **PhoneNumber** - "+33 1 23 45 67 89" is the same number everywhere

---

### Comparison Table: Entity vs Value Object

| Feature | Entity | Value Object |
|---------|--------|--------------|
| **Has Identity (Id)** | ✅ Yes | ❌ No |
| **Equality** | By Id | By all property values |
| **Mutable** | Yes (can change) | No (immutable) |
| **Lifespan** | Has lifecycle | Describes state |
| **Replaceability** | Cannot replace | Fully replaceable |
| **Database Table** | Usually has own table | Often embedded in entity table |
| **Base Class** | `BaseEntity` | `ValueObject` |

### How to Decide: Entity or Value Object?

**Ask yourself:**
1. **Does it need a unique identifier?**
   - Yes → Entity
   - No → Value Object

2. **If two instances have the same property values, are they the same thing?**
   - Yes → Value Object
   - No → Entity

3. **Does it need to track changes over time?**
   - Yes → Entity
   - No → Value Object

**Examples:**

| Concept | Question | Answer | Type |
|---------|----------|--------|------|
| Hotel | Two hotels with name "Hilton" - same hotel? | No, different hotels | Entity |
| Address | Two addresses "123 Main St, Paris" - same address? | Yes, same address | Value Object |
| Booking | Two bookings for same dates - same booking? | No, different bookings | Entity |
| Money | Two amounts of "$100" - same value? | Yes, same money value | Value Object |

---

## Aggregate Roots

### Definition:
An **Aggregate** is a cluster of related entities and value objects treated as a single unit. The **Aggregate Root** is the main entity that controls access to the entire aggregate.

### Rules:
1. **External objects can only reference the Aggregate Root** (not internal entities)
2. **All changes go through the Aggregate Root** (enforces business rules)
3. **Maintains consistency boundaries** (all-or-nothing changes)
4. **Only Aggregate Root has a repository** (not internal entities)

### Example:

```csharp
// Hotel is an AGGREGATE ROOT
public class Hotel : BaseEntity
{
    public string Name { get; private set; }

    // RoomTypes are part of the Hotel aggregate
    // Private list - cannot be modified directly from outside
    private readonly List<RoomType> _roomTypes = new();

    // ReadOnly collection - can read but not modify
    public IReadOnlyCollection<RoomType> RoomTypes => _roomTypes.AsReadOnly();

    // ALL changes go through Hotel (enforces business rules)
    public void AddRoomType(RoomType roomType)
    {
        // Business rule: No duplicate room type names
        if (_roomTypes.Any(rt => rt.Name.Equals(roomType.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Room type '{roomType.Name}' already exists");

        // Business rule: Price must be positive
        if (roomType.BasePrice <= 0)
            throw new InvalidOperationException("Room type price must be positive");

        _roomTypes.Add(roomType);
        UpdateTimestamp();
    }

    public void RemoveRoomType(int roomTypeId)
    {
        var roomType = _roomTypes.FirstOrDefault(rt => rt.Id == roomTypeId);
        if (roomType != null)
        {
            _roomTypes.Remove(roomType);
            UpdateTimestamp();
        }
    }

    public void UpdateRoomTypePrice(int roomTypeId, decimal newPrice)
    {
        var roomType = _roomTypes.FirstOrDefault(rt => rt.Id == roomTypeId);
        if (roomType == null)
            throw new InvalidOperationException("Room type not found");

        roomType.UpdatePrice(newPrice);
        UpdateTimestamp();
    }
}

// RoomType is an ENTITY, but part of Hotel aggregate
public class RoomType : BaseEntity
{
    public int HotelId { get; private set; }  // Belongs to a Hotel
    public string Name { get; private set; }
    public decimal BasePrice { get; private set; }

    internal void UpdatePrice(decimal newPrice)  // internal - only Hotel can call
    {
        if (newPrice <= 0)
            throw new ArgumentException("Price must be positive");

        BasePrice = newPrice;
    }
}
```

### Usage:

```csharp
// ✅ CORRECT: Go through aggregate root
var hotel = await hotelRepository.GetByIdAsync(1);
var roomType = new RoomType("Deluxe Suite", 299.99m);
hotel.AddRoomType(roomType);  // Hotel enforces business rules
await hotelRepository.UpdateAsync(hotel);

// ❌ WRONG: Don't bypass aggregate root
var roomType = new RoomType("Deluxe Suite", 299.99m);
await roomTypeRepository.AddAsync(roomType);  // Bypasses Hotel's validation!

// ❌ WRONG: Don't modify internal entities directly
hotel.RoomTypes[0].BasePrice = -100;  // Can't do this! (no setter, plus would bypass validation)

// ✅ CORRECT: All changes through aggregate root
hotel.UpdateRoomTypePrice(roomTypeId, 350.00m);  // Hotel controls the change
```

### Benefits of Aggregate Roots:
1. **Encapsulation** - Business rules enforced in one place
2. **Consistency** - All related changes happen together
3. **Simplicity** - One entry point for operations
4. **Transaction boundary** - All-or-nothing persistence

---

## Domain Services

### Definition:
Operations that don't naturally belong to a specific entity or value object, but are part of the domain logic.

### When to Use:
- Logic involves multiple entities
- Operation doesn't fit naturally in one entity
- Stateless operations

### Example:

```csharp
// Domain Service
public class PricingService
{
    public decimal CalculateBookingPrice(
        RoomType roomType,
        DateTime checkIn,
        DateTime checkOut,
        List<PricingRule> activeRules)
    {
        // This logic doesn't belong in RoomType or Booking
        // It involves complex calculations with external rules

        int nights = (checkOut - checkIn).Days;
        decimal basePrice = roomType.BasePrice * nights;

        // Apply seasonal adjustments
        decimal seasonalMultiplier = GetSeasonalMultiplier(checkIn);
        basePrice *= seasonalMultiplier;

        // Apply day-of-week adjustments
        foreach (var date in GetDateRange(checkIn, checkOut))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                basePrice *= 1.2m; // 20% weekend surcharge
        }

        // Apply active rules
        foreach (var rule in activeRules)
        {
            basePrice = rule.Apply(basePrice);
        }

        return basePrice;
    }

    private decimal GetSeasonalMultiplier(DateTime date)
    {
        // High season: June-August
        if (date.Month >= 6 && date.Month <= 8)
            return 1.5m;

        // Low season: November-February
        if (date.Month >= 11 || date.Month <= 2)
            return 0.8m;

        return 1.0m;
    }
}

// Usage in Application layer:
public class CreateBookingCommandHandler
{
    private readonly PricingService _pricingService;

    public async Task<int> Handle(CreateBookingCommand command)
    {
        var roomType = await _roomTypeRepository.GetByIdAsync(command.RoomTypeId);
        var rules = await _pricingRuleRepository.GetActiveRulesAsync();

        // Use domain service
        decimal totalPrice = _pricingService.CalculateBookingPrice(
            roomType,
            command.CheckIn,
            command.CheckOut,
            rules
        );

        var booking = new Booking(roomType, command.CheckIn, command.CheckOut, totalPrice);
        await _bookingRepository.AddAsync(booking);

        return booking.Id;
    }
}
```

---

## Domain Events

### Definition:
Something significant that happened in the domain that other parts of the system might care about.

### Characteristics:
- Immutable (event already happened, cannot change)
- Past tense naming (e.g., `HotelCreated`, not `CreateHotel`)
- Contains relevant data about what happened
- Can trigger side effects in other parts of the system

### Example:

```csharp
// Domain Event
public class HotelCreatedEvent : IDomainEvent
{
    public int HotelId { get; }
    public string HotelName { get; }
    public string City { get; }
    public DateTime CreatedAt { get; }

    public HotelCreatedEvent(int hotelId, string hotelName, string city)
    {
        HotelId = hotelId;
        HotelName = hotelName;
        City = city;
        CreatedAt = DateTime.UtcNow;
    }
}

// Entity raises domain events
public class Hotel : BaseEntity
{
    private List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public Hotel(string name, Address address)
    {
        Name = name;
        Address = address;

        // Raise domain event
        _domainEvents.Add(new HotelCreatedEvent(this.Id, this.Name, address.City));
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// Event Handler (in Application layer)
public class HotelCreatedEventHandler : INotificationHandler<HotelCreatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;

    public async Task Handle(HotelCreatedEvent notification)
    {
        // Side effect 1: Send notification email
        await _emailService.SendAsync(
            to: "admin@hotel.com",
            subject: "New Hotel Added",
            body: $"Hotel '{notification.HotelName}' was created in {notification.City}"
        );

        // Side effect 2: Initialize inventory for new hotel
        await _inventoryService.InitializeInventoryAsync(notification.HotelId);

        // Side effect 3: Update search index
        await _searchService.IndexHotelAsync(notification.HotelId);
    }
}
```

### Benefits:
- **Loose coupling** - Parts of system don't directly depend on each other
- **Scalability** - Can process events asynchronously
- **Auditability** - Event log provides history
- **Integration** - Easy to integrate with other systems

---

## Repositories

### Definition:
Abstraction for accessing and persisting aggregates. Acts like an in-memory collection.

### Characteristics:
- One repository per Aggregate Root (not per entity)
- Methods use domain language (e.g., `GetActiveHotels`, not `SelectWhereIsDeletedFalse`)
- Returns domain entities, not data models
- Hides database implementation details

### Example:

```csharp
// Repository Interface (in Domain/Application layer)
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(int id);
    Task<List<Hotel>> GetAllAsync();
    Task<List<Hotel>> GetActiveHotelsAsync();
    Task<List<Hotel>> GetHotelsByCityAsync(string city);
    Task<Hotel> AddAsync(Hotel hotel);
    Task UpdateAsync(Hotel hotel);
    Task DeleteAsync(int id);
}

// Repository Implementation (in Persistence layer)
public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public HotelRepository(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<Hotel?> GetByIdAsync(int id)
    {
        return await _context.Hotels
            .Include(h => h.RoomTypes)  // Load entire aggregate
            .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);
    }

    public async Task<List<Hotel>> GetActiveHotelsAsync()
    {
        return await _context.Hotels
            .Where(h => !h.IsDeleted)
            .Include(h => h.RoomTypes)
            .ToListAsync();
    }

    public async Task<Hotel> AddAsync(Hotel hotel)
    {
        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();
        return hotel;
    }
}

// Usage (in Application layer):
public class GetHotelQueryHandler
{
    private readonly IHotelRepository _repository;

    public async Task<HotelDto> Handle(GetHotelQuery query)
    {
        // Repository abstracts database access
        var hotel = await _repository.GetByIdAsync(query.HotelId);

        if (hotel == null)
            throw new HotelNotFoundException(query.HotelId);

        return MapToDto(hotel);
    }
}
```

---

## DDD in Clean Architecture

### How DDD maps to Clean Architecture layers:

```
┌────────────────────────────────────────────────┐
│           API Layer (Presentation)             │
│  - Controllers/Endpoints                       │
│  - Entry point for HTTP requests               │
└────────────────────────────────────────────────┘
                      ↓
┌────────────────────────────────────────────────┐
│       Application Layer (Use Cases)            │
│  - Commands & Queries (CQRS)                   │
│  - Command/Query Handlers                      │
│  - DTOs                                        │
│  - Validators                                  │
│  - Mapping Profiles                            │
│  - Repository Interfaces                       │
└────────────────────────────────────────────────┘
                      ↓
┌────────────────────────────────────────────────┐
│         Domain Layer (Core - DDD)              │  ← All DDD concepts here
│  ✓ Entities (Hotel, RoomType, Booking)        │
│  ✓ Value Objects (Address, Money)             │
│  ✓ Aggregate Roots (Hotel controls RoomTypes) │
│  ✓ Domain Services (PricingService)           │
│  ✓ Domain Events (HotelCreatedEvent)          │
│  ✓ Domain Exceptions                           │
│  ✓ Business Rules                              │
│                                                │
│  NO external dependencies!                     │
│  Pure C# - no frameworks, no database          │
└────────────────────────────────────────────────┘
                      ↓
┌────────────────────────────────────────────────┐
│      Persistence Layer (Infrastructure)        │
│  - Repository Implementations                  │
│  - DbContext                                   │
│  - Entity Configurations                       │
│  - Migrations                                  │
└────────────────────────────────────────────────┘
                      ↓
┌────────────────────────────────────────────────┐
│             Database (PostgreSQL)              │
└────────────────────────────────────────────────┘
```

---

## Real Examples from Our System

### Example 1: Hotel and RoomType (Aggregate)

```csharp
// AGGREGATE ROOT
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }  // Value Object
    public ContactInfo ContactInfo { get; private set; }  // Value Object

    private readonly List<RoomType> _roomTypes = new();
    public IReadOnlyCollection<RoomType> RoomTypes => _roomTypes.AsReadOnly();

    public Hotel(string name, Address address, ContactInfo contactInfo)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name is required");

        Name = name;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        ContactInfo = contactInfo;
    }

    public void AddRoomType(RoomType roomType)
    {
        if (_roomTypes.Any(rt => rt.Name.Equals(roomType.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Room type '{roomType.Name}' already exists");

        _roomTypes.Add(roomType);
        UpdateTimestamp();
    }
}

// ENTITY (part of aggregate)
public class RoomType : BaseEntity
{
    public int HotelId { get; private set; }
    public string Name { get; private set; }
    public decimal BasePrice { get; private set; }
}

// VALUE OBJECT
public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    public Address(string street, string city, string country)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required");

        Street = street;
        City = city;
        Country = country;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return Country;
    }
}
```

### Example 2: Complete Flow

```csharp
// 1. Command (Application Layer)
public class CreateHotelCommand : IRequest<int>
{
    public string Name { get; set; }
    public string Street { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
}

// 2. Handler (Application Layer)
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository;

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // Create value object
        var address = new Address(request.Street, request.City, request.Country);

        // Create entity (domain logic)
        var hotel = new Hotel(request.Name, address, null);

        // Persist through repository
        await _repository.AddAsync(hotel);

        // Domain events are published here (if configured)

        return hotel.Id;
    }
}

// 3. Repository (Persistence Layer)
public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public async Task<Hotel> AddAsync(Hotel hotel)
    {
        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();
        return hotel;
    }
}
```

---

## Summary

### DDD Building Blocks in Our System:

| DDD Concept | Implementation | Purpose |
|-------------|----------------|---------|
| **Entity** | `Hotel`, `RoomType`, `Booking` | Objects with identity |
| **Value Object** | `Address`, `ContactInfo`, `Money` | Descriptive objects without identity |
| **Aggregate Root** | `Hotel` controls `RoomType` | Enforce business rules and consistency |
| **Domain Service** | `PricingService` | Logic that doesn't fit in entities |
| **Domain Event** | `HotelCreatedEvent` | Significant happenings |
| **Repository** | `IHotelRepository` | Abstract data access |
| **Base Entity** | `BaseEntity` | Common identity and tracking |
| **Base Value Object** | `ValueObject` | Common value equality |

### Key Principles:

1. **Domain Layer is pure** - No dependencies on frameworks or databases
2. **Business rules in domain** - Not in controllers or database
3. **Aggregate Roots control access** - Enforce consistency
4. **Value Objects are immutable** - Replace, don't modify
5. **Repository per Aggregate Root** - Not per entity
6. **Domain Events for side effects** - Loose coupling

---

## Further Reading

- [Domain-Driven Design by Eric Evans](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)
- [Implementing Domain-Driven Design by Vaughn Vernon](https://www.amazon.com/Implementing-Domain-Driven-Design-Vaughn-Vernon/dp/0321834577)
- [Microsoft DDD Documentation](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)