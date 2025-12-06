# C# Classes Guide - Complete Reference

This document explains all types of classes in C# and their characteristics, with examples from our Hotel Booking System.

---

## Table of Contents

1. [Abstract Classes](#abstract-classes)
2. [Concrete Classes](#concrete-classes)
3. [Static Classes](#static-classes)
4. [Sealed Classes](#sealed-classes)
5. [Partial Classes](#partial-classes)
6. [Generic Classes](#generic-classes)
7. [Nested Classes](#nested-classes)
8. [Record Classes](#record-classes)
9. [Comparison Table](#comparison-table)

---

## Abstract Classes

### Characteristics:
- **Cannot be instantiated directly** - You cannot do `new BaseEntity()`
- **Must be inherited** - Only derived classes can be instantiated
- **Can have abstract methods** - Methods without implementation that must be overridden
- **Can have concrete methods** - Methods with implementation
- **Can have fields and properties**
- **Can have constructors** - Called when derived class is instantiated
- **Use `abstract` keyword**

### When to Use:
- When you want to provide a common base for multiple classes
- When you want to enforce that certain classes share common behavior
- When you have partial implementation that should be shared

### Example from our project:

```csharp
public abstract class BaseEntity
{
    public int Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
    public bool IsDeleted { get; protected set; }

    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

// Usage: Hotel inherits from BaseEntity
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    // ... other properties
}

// This works:
var hotel = new Hotel(); // Creates instance of derived class
hotel.MarkAsDeleted();   // Can use methods from BaseEntity

// This does NOT work:
// var entity = new BaseEntity(); // ❌ ERROR: Cannot create instance of abstract class
```

### Abstract Methods Example:

```csharp
public abstract class Animal
{
    // Concrete method (has implementation)
    public void Sleep()
    {
        Console.WriteLine("Sleeping...");
    }

    // Abstract method (no implementation, must be overridden)
    public abstract void MakeSound();
}

public class Dog : Animal
{
    // Must implement abstract method
    public override void MakeSound()
    {
        Console.WriteLine("Woof!");
    }
}

public class Cat : Animal
{
    // Must implement abstract method
    public override void MakeSound()
    {
        Console.WriteLine("Meow!");
    }
}
```

---

## Concrete Classes

### Characteristics:
- **Can be instantiated** - You can do `new Hotel()`
- **Default class type** - If you don't specify modifiers, it's concrete
- **Can inherit from abstract classes**
- **Can implement interfaces**
- **Must implement all abstract members from base class**

### When to Use:
- Most of the time - this is the standard class type
- When you want to create actual objects

### Example:

```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Address Address { get; private set; }

    public Hotel(string name, string description, Address address)
    {
        Name = name;
        Description = description;
        Address = address;
    }
}

// Usage:
var hotel = new Hotel("Hilton", "Luxury hotel", address);
```

---

## Static Classes

### Characteristics:
- **Cannot be instantiated** - You cannot do `new UtilityClass()`
- **All members must be static**
- **Cannot inherit or be inherited**
- **Cannot implement interfaces**
- **Use `static` keyword**
- **Loaded once when first accessed**

### When to Use:
- Utility/helper methods
- Extension methods
- Constants
- When you don't need instance-specific data

### Example:

```csharp
public static class BookingReferenceGenerator
{
    public static string Generate(DateTime date)
    {
        return $"BK-{date:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}

// Usage:
string reference = BookingReferenceGenerator.Generate(DateTime.UtcNow);
// Result: "BK-20250102-A1B2C3D4"

// This does NOT work:
// var generator = new BookingReferenceGenerator(); // ❌ ERROR
```

---

## Sealed Classes

### Characteristics:
- **Cannot be inherited** - No class can derive from it
- **Can be instantiated** (unless also abstract, which is invalid)
- **Use `sealed` keyword**
- **Improves performance slightly** (compiler optimizations)

### When to Use:
- When you want to prevent inheritance
- For security reasons
- When class is complete and shouldn't be extended

### Example:

```csharp
public sealed class PaymentProcessor
{
    public void ProcessPayment(decimal amount)
    {
        // Payment logic
    }
}

// Usage:
var processor = new PaymentProcessor(); // ✓ Works

// This does NOT work:
// public class CustomPaymentProcessor : PaymentProcessor // ❌ ERROR: Cannot inherit from sealed class
```

---

## Partial Classes

### Characteristics:
- **Definition split across multiple files**
- **All parts must use `partial` keyword**
- **All parts must be in same namespace**
- **Compiled into single class**
- **Useful for code generation**

### When to Use:
- Large classes that need organization
- Generated code (like EF Core DbContext)
- Separation of concerns within a single class

### Example:

**File: Hotel.cs**
```csharp
public partial class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }
}
```

**File: Hotel.BusinessLogic.cs**
```csharp
public partial class Hotel
{
    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required");

        Name = name;
        UpdateTimestamp();
    }
}
```

**Usage:**
```csharp
var hotel = new Hotel();
hotel.UpdateName("New Name"); // Method from second file works!
```

---

## Generic Classes

### Characteristics:
- **Type parameters** using `<T>`
- **Type-safe** - No casting needed
- **Reusable** for different types
- **Compile-time type checking**

### When to Use:
- Collections
- Repository patterns
- When same logic works for multiple types

### Example:

```csharp
public class Repository<T> where T : BaseEntity
{
    private readonly DbContext _context;

    public async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
}

// Usage:
var hotelRepository = new Repository<Hotel>();
var hotel = await hotelRepository.GetByIdAsync(1);

var roomRepository = new Repository<RoomType>();
var room = await roomRepository.GetByIdAsync(10);
```

---

## Nested Classes

### Characteristics:
- **Class defined inside another class**
- **Can access private members of outer class**
- **Can be private, protected, or public**

### When to Use:
- Helper classes used only by containing class
- Closely related functionality
- Encapsulation

### Example:

```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    private List<Booking> _bookings = new();

    public class Booking
    {
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }

    public void AddBooking(Booking booking)
    {
        _bookings.Add(booking);
    }
}

// Usage:
var hotel = new Hotel();
var booking = new Hotel.Booking
{
    CheckIn = DateTime.Today,
    CheckOut = DateTime.Today.AddDays(3)
};
hotel.AddBooking(booking);
```

---

## Record Classes (C# 9+)

### Characteristics:
- **Immutable by default**
- **Value-based equality** (compares values, not references)
- **Concise syntax**
- **Use `record` keyword**
- **Great for DTOs and Value Objects**

### When to Use:
- DTOs (Data Transfer Objects)
- Value Objects
- When you need immutability
- When you need value-based equality

### Example:

```csharp
// Traditional class
public class HotelDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
}

// Record (shorter, immutable)
public record HotelDto(int Id, string Name, string City);

// Usage:
var dto1 = new HotelDto(1, "Hilton", "Paris");
var dto2 = new HotelDto(1, "Hilton", "Paris");

Console.WriteLine(dto1 == dto2); // True (value equality)

// With expression (create copy with changes)
var dto3 = dto1 with { Name = "Marriott" };
```

---

## Comparison Table

| Feature | Abstract | Concrete | Static | Sealed | Partial | Generic | Record |
|---------|----------|----------|--------|--------|---------|---------|--------|
| **Can instantiate** | ❌ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Can inherit from** | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Can be inherited** | ✅ | ✅ | ❌ | ❌ | ✅ | ✅ | ✅ |
| **Can have abstract methods** | ✅ | ❌ | ❌ | ❌ | ✅ | ✅ | ❌ |
| **Can have instance members** | ✅ | ✅ | ❌ | ✅ | ✅ | ✅ | ✅ |
| **Can have static members** | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| **Split across files** | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ | ❌ |
| **Type parameters** | ❌ | ❌ | ❌ | ❌ | ❌ | ✅ | ❌ |
| **Immutable by default** | ❌ | ❌ | N/A | ❌ | ❌ | ❌ | ✅ |
| **Value equality** | ❌ | ❌ | N/A | ❌ | ❌ | ❌ | ✅ |

---

## Access Modifiers for Classes

### Top-Level Classes (in namespace):
- **`public`** - Accessible from any assembly
- **`internal`** - Accessible only within same assembly (default)

### Nested Classes (inside another class):
- **`public`** - Accessible from anywhere
- **`protected`** - Accessible in class and derived classes
- **`internal`** - Accessible within same assembly
- **`protected internal`** - Accessible in same assembly or derived classes
- **`private`** - Accessible only in containing class (default for nested)
- **`private protected`** - Accessible in containing class or derived classes in same assembly

---

## Property Access Modifiers

### Common Patterns:

```csharp
public class Hotel
{
    // Public getter, private setter (most common in domain entities)
    public string Name { get; private set; }

    // Public getter, protected setter (allows derived classes to set)
    public DateTime CreatedAt { get; protected set; }

    // Public getter and setter (use in DTOs)
    public string Description { get; set; }

    // Private (internal use only)
    private List<RoomType> _roomTypes;

    // Public getter, no setter (read-only, set in constructor)
    public int Id { get; }

    // Init-only setter (can set in constructor or object initializer only)
    public string Code { get; init; }
}
```

---

## Real-World Usage in Our Project

### Domain Layer:
```csharp
// Abstract base class
public abstract class BaseEntity { }

// Concrete entity
public class Hotel : BaseEntity { }

// Value object (could be record)
public class Address : ValueObject { }
```

### Application Layer:
```csharp
// DTOs (use records)
public record CreateHotelRequest(string Name, string City);
public record HotelDto(int Id, string Name, string City);

// Generic handler
public class CommandHandler<TCommand, TResponse> { }
```

### Infrastructure Layer:
```csharp
// Static utility class
public static class DateTimeProvider { }

// Generic repository
public class Repository<T> where T : BaseEntity { }
```

### Test Projects:
```csharp
// Sealed test class (cannot inherit from test classes)
public sealed class HotelServiceTests { }
```

---

## Best Practices

1. **Use abstract classes when:**
   - You have common implementation to share
   - You want to enforce that derived classes exist
   - Example: `BaseEntity`, `ValueObject`

2. **Use concrete classes when:**
   - You need actual objects
   - Most of your code
   - Example: `Hotel`, `RoomType`

3. **Use static classes when:**
   - You have utility/helper methods
   - No instance data needed
   - Example: `BookingReferenceGenerator`, `EmailValidator`

4. **Use sealed classes when:**
   - Security sensitive code
   - Class is complete and shouldn't be extended
   - Example: `PaymentProcessor`, `EncryptionService`

5. **Use partial classes when:**
   - Very large classes
   - Working with generated code
   - Example: `DbContext` extensions

6. **Use generic classes when:**
   - Same logic works for multiple types
   - Type safety is important
   - Example: `Repository<T>`, `Result<T>`

7. **Use record classes when:**
   - DTOs
   - Value objects
   - Immutable data
   - Example: `HotelDto`, `CreateHotelRequest`

---

## Summary

| Use Case | Recommended Class Type |
|----------|----------------------|
| Base class for entities | Abstract Class |
| Domain entities | Concrete Class |
| Helper methods | Static Class |
| Security-sensitive code | Sealed Class |
| Large class organization | Partial Class |
| Repository pattern | Generic Class |
| DTOs and Value Objects | Record Class |
| Nested helpers | Nested Class |

---

## Additional Resources

- [Microsoft Docs: Classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/classes)
- [Abstract Classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/abstract-and-sealed-classes-and-class-members)
- [Static Classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/static-classes-and-static-class-members)
- [Generic Classes](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/)
- [Records](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record)