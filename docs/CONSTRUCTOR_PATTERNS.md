# Constructor Patterns in C#

This document explains constructor patterns used in our Hotel Booking System, especially the **private constructor pattern** for Entity Framework Core.

---

## Table of Contents

1. [Why Multiple Constructors?](#why-multiple-constructors)
2. [Private Constructor Pattern](#private-constructor-pattern)
3. [Constructor Types](#constructor-types)
4. [EF Core and Constructors](#ef-core-and-constructors)
5. [Real Examples](#real-examples)

---

## Why Multiple Constructors?

In our entities, you'll often see **two constructors**:

```csharp
public class Hotel : BaseEntity
{
    // Constructor 1: Private parameterless constructor
    private Hotel() { }

    // Constructor 2: Public constructor with parameters
    public Hotel(string name, string city, string country)
    {
        // Validation and initialization
    }
}
```

**Why?**
1. EF Core needs a parameterless constructor to read from database
2. Our code needs a constructor with validation for creating new entities

---

## Private Constructor Pattern

### Definition:
A **private parameterless constructor** that only Entity Framework Core can use.

### Purpose:
```csharp
private Hotel() { }
```

This constructor exists ONLY for EF Core to create instances when reading from the database.

### How EF Core Uses It:

```csharp
// When you query the database:
var hotel = await dbContext.Hotels.FindAsync(1);

// EF Core internally does:
// 1. var hotel = new Hotel();      ← Uses private constructor
// 2. hotel.Id = 1;                 ← Sets properties via reflection
// 3. hotel.Name = "Hilton";        ← Bypasses private setters
// 4. hotel.City = "Paris";         ← Directly sets values
// 5. Returns fully populated hotel
```

---

## Constructor Types

### 1. Private Parameterless Constructor

**Syntax:**
```csharp
private Hotel() { }
```

**Purpose:**
- Only for EF Core to read from database
- Not accessible in your code

**Example:**
```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public string City { get; private set; }

    // For EF Core only
    private Hotel() { }

    // For your code
    public Hotel(string name, string city)
    {
        Name = name;
        City = city;
    }
}

// Usage:
var hotel = new Hotel("Hilton", "Paris");  ✅ Works
var hotel = new Hotel();                   ❌ Error: 'Hotel.Hotel()' is inaccessible
```

---

### 2. Public Constructor with Parameters

**Syntax:**
```csharp
public Hotel(string name, string city, string country)
{
    // Validation
    // Initialization
}
```

**Purpose:**
- Create new entities in your code
- Enforce business rules
- Validate required fields

**Example:**
```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    private Hotel() { }

    public Hotel(string name, string city, string country)
    {
        // Business rule: Name is required
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name is required", nameof(name));

        // Business rule: City is required
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        // Business rule: Country is required
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        Name = name;
        City = city;
        Country = country;
    }
}

// Usage in Application layer:
var hotel = new Hotel("Hilton", "Paris", "France");  ✅ Valid
var hotel = new Hotel("", "Paris", "France");        ❌ Throws exception
```

---

### 3. Protected Constructor

**Syntax:**
```csharp
protected Hotel() { }
```

**Purpose:**
- For EF Core (like private)
- Also allows derived classes to use it

**When to use:**
- When you plan to inherit from the class
- Most of the time, `private` is better

**Example:**
```csharp
public class Hotel : BaseEntity
{
    protected Hotel() { }
}

// Derived class can use protected constructor
public class LuxuryHotel : Hotel
{
    public LuxuryHotel() : base()  // Can call protected constructor
    {
        // Additional initialization
    }
}
```

---

## EF Core and Constructors

### What EF Core Needs:

EF Core requires a **parameterless constructor** to create instances when reading from database.

### Three Options:

#### Option 1: Private Constructor (Recommended ✅)
```csharp
public class Hotel : BaseEntity
{
    private Hotel() { }

    public Hotel(string name) { ... }
}
```

**Pros:**
- ✅ EF Core can read from database
- ✅ Forces validation for new objects
- ✅ Cannot create invalid objects

**Cons:**
- None

---

#### Option 2: Public Parameterless Constructor (Not Recommended ❌)
```csharp
public class Hotel : BaseEntity
{
    public Hotel() { }

    public Hotel(string name) { ... }
}
```

**Pros:**
- ✅ EF Core can read from database

**Cons:**
- ❌ Anyone can create invalid objects:
  ```csharp
  var hotel = new Hotel();  // No validation!
  // hotel.Name is null - invalid state!
  ```

---

#### Option 3: No Parameterless Constructor (Breaks EF Core ❌)
```csharp
public class Hotel : BaseEntity
{
    // Only one constructor
    public Hotel(string name) { ... }
}
```

**Pros:**
- ✅ Forces validation

**Cons:**
- ❌ EF Core cannot read from database
- ❌ Error: `No suitable constructor found for entity type 'Hotel'`

---

## Real Examples

### Example 1: Simple Entity

```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }

    // For EF Core
    private Hotel() { }

    // For creating new hotels
    public Hotel(string name, string city, string country)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        Name = name;
        City = city;
        Country = country;
    }
}
```

---

### Example 2: Entity with Complex Validation

```csharp
public class RoomType : BaseEntity
{
    public int HotelId { get; private set; }
    public string Name { get; private set; }
    public int MaxOccupancy { get; private set; }
    public decimal BasePrice { get; private set; }

    // For EF Core
    private RoomType() { }

    // For creating new room types
    public RoomType(
        int hotelId,
        string name,
        int maxOccupancy,
        decimal basePrice)
    {
        // Validation
        if (hotelId <= 0)
            throw new ArgumentException("Hotel ID is required", nameof(hotelId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name is required", nameof(name));

        if (maxOccupancy <= 0)
            throw new ArgumentException("Max occupancy must be greater than zero", nameof(maxOccupancy));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative", nameof(basePrice));

        // Assignment
        HotelId = hotelId;
        Name = name;
        MaxOccupancy = maxOccupancy;
        BasePrice = basePrice;
    }
}
```

---

### Example 3: Usage in Application Layer

```csharp
// Command Handler (creating new hotel)
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository;

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        // Uses PUBLIC constructor - validation happens
        var hotel = new Hotel(
            request.Name,
            request.City,
            request.Country
        );

        // Persist
        await _repository.AddAsync(hotel);

        return hotel.Id;
    }
}

// Repository (reading from database)
public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public async Task<Hotel> GetByIdAsync(int id)
    {
        // EF Core uses PRIVATE constructor - no validation needed
        var hotel = await _context.Hotels.FindAsync(id);

        return hotel;
    }
}
```

---

## Comparison: With vs Without Private Constructor

### Without Private Constructor:

```csharp
public class Hotel : BaseEntity
{
    public string Name { get; set; }  // Public setter
    public string City { get; set; }

    public Hotel() { }  // Public constructor
}

// Problems:
var hotel = new Hotel();           // No validation
hotel.Name = "";                   // Invalid state allowed
hotel.City = null;                 // Invalid state allowed
await repository.AddAsync(hotel);  // Saving invalid data!
```

---

### With Private Constructor:

```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }  // Private setter
    public string City { get; private set; }

    private Hotel() { }  // For EF Core only

    public Hotel(string name, string city)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required");

        Name = name;
        City = city;
    }
}

// Benefits:
var hotel = new Hotel();                      // ❌ Error: Cannot access private constructor
var hotel = new Hotel("", "Paris");           // ❌ Exception: Name required
var hotel = new Hotel("Hilton", "Paris");     // ✅ Valid object created
hotel.Name = "Changed";                       // ❌ Error: Private setter
```

---

## Summary

| Constructor Type | Visibility | Purpose | Who Uses It |
|-----------------|------------|---------|-------------|
| `private Hotel()` | Private | EF Core reads from DB | EF Core only |
| `public Hotel(...)` | Public | Create new entities | Your application code |
| `protected Hotel()` | Protected | EF Core + inheritance | EF Core + derived classes |

---

## Best Practices

1. **Always use private parameterless constructor** for entities with EF Core
2. **Always validate** in public constructors
3. **Use private setters** on properties to enforce encapsulation
4. **Initialize all required fields** in public constructor
5. **Don't expose parameterless constructor** publicly

---

## Common Mistakes

### ❌ Mistake 1: No Private Constructor
```csharp
public class Hotel : BaseEntity
{
    public Hotel(string name) { }
}
// Error: EF Core cannot create instances
```

### ❌ Mistake 2: Public Parameterless Constructor
```csharp
public class Hotel : BaseEntity
{
    public Hotel() { }  // Anyone can create invalid objects
    public Hotel(string name) { }
}
```

### ❌ Mistake 3: Public Setters
```csharp
public class Hotel : BaseEntity
{
    public string Name { get; set; }  // Bypasses validation
}
```

---

## Correct Pattern ✅

```csharp
public class Hotel : BaseEntity
{
    // Properties with private setters
    public string Name { get; private set; }
    public string City { get; private set; }

    // Private constructor for EF Core
    private Hotel() { }

    // Public constructor with validation
    public Hotel(string name, string city)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        Name = name;
        City = city;
    }

    // Business methods for changes
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Name is required");

        Name = newName;
        UpdateTimestamp();
    }
}
```

---

## Additional Resources

- [EF Core Documentation: Entity Types](https://docs.microsoft.com/en-us/ef/core/modeling/constructors)
- [C# Constructors](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/constructors)
- [Domain-Driven Design Patterns](https://martinfowler.com/bliki/EvansClassification.html)