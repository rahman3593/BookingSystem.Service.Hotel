# Domain Exceptions Guide

This document explains why and how to use custom domain exceptions in our Hotel Booking System.

---

## Table of Contents

1. [What are Domain Exceptions?](#what-are-domain-exceptions)
2. [Why Use Domain Exceptions?](#why-use-domain-exceptions)
3. [Benefits vs General Exceptions](#benefits-vs-general-exceptions)
4. [How to Implement](#how-to-implement)
5. [Real-World Examples](#real-world-examples)
6. [Best Practices](#best-practices)

---

## What are Domain Exceptions?

**Domain Exceptions** are custom exception classes that represent specific errors in your business logic/domain layer.

```csharp
// Domain Exception
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }

    public DomainException(string message, Exception innerException)
        : base(message, innerException) { }
}

// Specific Domain Exceptions
public class HotelNotFoundException : DomainException
{
    public HotelNotFoundException(int hotelId)
        : base($"Hotel with ID {hotelId} was not found") { }
}
```

---

## Why Use Domain Exceptions?

### Problem with General Exceptions:

```csharp
// Using general Exception class ❌
public Hotel GetHotel(int id)
{
    var hotel = _repository.GetById(id);

    if (hotel == null)
        throw new Exception("Hotel not found");  // Generic, unclear

    return hotel;
}

// Catching is unclear
try
{
    var hotel = service.GetHotel(123);
}
catch (Exception ex)  // Catches EVERYTHING - too broad!
{
    // Is it hotel not found?
    // Is it database error?
    // Is it network error?
    // We don't know!
}
```

### Solution with Domain Exceptions:

```csharp
// Using custom exception ✅
public Hotel GetHotel(int id)
{
    var hotel = _repository.GetById(id);

    if (hotel == null)
        throw new HotelNotFoundException(id);  // Specific, clear

    return hotel;
}

// Catching is precise
try
{
    var hotel = service.GetHotel(123);
}
catch (HotelNotFoundException ex)  // Specific exception
{
    // We know exactly what happened!
    return NotFound(ex.Message);
}
catch (DomainException ex)  // Other domain errors
{
    return BadRequest(ex.Message);
}
catch (Exception ex)  // Unexpected system errors
{
    return StatusCode(500, "Internal error");
}
```

---

## Benefits vs General Exceptions

### Comparison Table:

| Feature | General Exception | Domain Exception |
|---------|------------------|------------------|
| **Specificity** | ❌ Generic | ✅ Specific error types |
| **Catching** | ❌ Catches everything | ✅ Catch specific errors |
| **Error Messages** | ❌ Manual string formatting | ✅ Formatted automatically |
| **Type Safety** | ❌ No compile-time checking | ✅ Compiler helps |
| **Testability** | ❌ Hard to test specific errors | ✅ Easy to test |
| **Maintainability** | ❌ Scattered error handling | ✅ Centralized error types |
| **Documentation** | ❌ Unclear what can fail | ✅ Clear error scenarios |
| **API Responses** | ❌ Generic 500 errors | ✅ Proper HTTP status codes |

---

### Detailed Comparison:

#### 1. Specificity

**General Exception:**
```csharp
throw new Exception("Error");  // What kind of error?
throw new Exception("Not found");  // What wasn't found?
throw new Exception("Invalid");  // What's invalid?
```

**Domain Exception:**
```csharp
throw new HotelNotFoundException(123);  // Clear: Hotel ID 123 not found
throw new RoomTypeNotFoundException(456);  // Clear: Room type ID 456 not found
throw new InvalidPriceException(-50);  // Clear: Price -50 is invalid
```

---

#### 2. Error Handling

**General Exception:**
```csharp
// BAD: Must catch everything
try
{
    var hotel = GetHotel(id);
}
catch (Exception ex)  // Catches ALL exceptions
{
    if (ex.Message.Contains("not found"))
        return NotFound();  // String matching - fragile!
    else if (ex.Message.Contains("invalid"))
        return BadRequest();
    else
        return StatusCode(500);
}
```

**Domain Exception:**
```csharp
// GOOD: Catch specific exceptions
try
{
    var hotel = GetHotel(id);
}
catch (HotelNotFoundException ex)
{
    return NotFound(ex.Message);  // 404
}
catch (InvalidOperationException ex)
{
    return BadRequest(ex.Message);  // 400
}
catch (DomainException ex)
{
    return BadRequest(ex.Message);  // 400
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500);  // 500
}
```

---

#### 3. Testability

**General Exception:**
```csharp
// HARD TO TEST
[Fact]
public void GetHotel_WhenNotFound_ThrowsException()
{
    // Arrange
    var service = new HotelService();

    // Act & Assert
    var exception = Assert.Throws<Exception>(() => service.GetHotel(999));

    // Fragile: Depends on exact string
    Assert.Contains("not found", exception.Message);
}
```

**Domain Exception:**
```csharp
// EASY TO TEST
[Fact]
public void GetHotel_WhenNotFound_ThrowsHotelNotFoundException()
{
    // Arrange
    var service = new HotelService();

    // Act & Assert
    var exception = Assert.Throws<HotelNotFoundException>(
        () => service.GetHotel(999)
    );

    // Type-safe, reliable
    Assert.Equal(999, exception.HotelId);
}
```

---

#### 4. API Response Mapping

**General Exception:**
```csharp
// POOR: All errors return 500
[HttpGet("{id}")]
public IActionResult GetHotel(int id)
{
    try
    {
        var hotel = _service.GetHotel(id);
        return Ok(hotel);
    }
    catch (Exception ex)  // Everything is 500!
    {
        return StatusCode(500, ex.Message);
    }
}
```

**Domain Exception:**
```csharp
// GOOD: Proper HTTP status codes
[HttpGet("{id}")]
public IActionResult GetHotel(int id)
{
    try
    {
        var hotel = _service.GetHotel(id);
        return Ok(hotel);  // 200
    }
    catch (HotelNotFoundException ex)
    {
        return NotFound(ex.Message);  // 404 - Correct!
    }
    catch (DomainException ex)
    {
        return BadRequest(ex.Message);  // 400 - Business rule violation
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Unexpected error");
        return StatusCode(500, "Internal error");  // 500 - System error
    }
}
```

---

## How to Implement

### Step 1: Create Base Domain Exception

```csharp
namespace BookingSystem.Service.Hotel.Domain.Exceptions;

public class DomainException : Exception
{
    // Constructor 1: Simple message
    public DomainException(string message) : base(message)
    {
    }

    // Constructor 2: Message + inner exception
    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

**What this does:**
- Base class for all domain exceptions
- Inherits from `Exception`
- Two constructors for flexibility

---

### Step 2: Create Specific Exceptions

```csharp
namespace BookingSystem.Service.Hotel.Domain.Exceptions;

// Hotel Not Found
public class HotelNotFoundException : DomainException
{
    public int HotelId { get; }

    public HotelNotFoundException(int hotelId)
        : base($"Hotel with ID {hotelId} was not found")
    {
        HotelId = hotelId;
    }
}

// Room Type Not Found
public class RoomTypeNotFoundException : DomainException
{
    public int RoomTypeId { get; }

    public RoomTypeNotFoundException(int roomTypeId)
        : base($"Room type with ID {roomTypeId} was not found")
    {
        RoomTypeId = roomTypeId;
    }
}

// Invalid Price
public class InvalidPriceException : DomainException
{
    public decimal Price { get; }

    public InvalidPriceException(decimal price)
        : base($"Price {price} is invalid. Price must be greater than zero.")
    {
        Price = price;
    }
}

// Duplicate Hotel Name
public class DuplicateHotelNameException : DomainException
{
    public string HotelName { get; }

    public DuplicateHotelNameException(string hotelName)
        : base($"Hotel with name '{hotelName}' already exists")
    {
        HotelName = hotelName;
    }
}
```

---

### Step 3: Use in Domain Layer

```csharp
public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public decimal BasePrice { get; private set; }

    public void UpdatePrice(decimal newPrice)
    {
        // Domain rule: Price must be positive
        if (newPrice <= 0)
            throw new InvalidPriceException(newPrice);

        BasePrice = newPrice;
        UpdateTimestamp();
    }

    public void UpdateName(string newName)
    {
        // Domain rule: Name is required
        if (string.IsNullOrWhiteSpace(newName))
            throw new DomainException("Hotel name is required");

        Name = newName;
        UpdateTimestamp();
    }
}
```

---

### Step 4: Use in Application Layer

```csharp
public class GetHotelQueryHandler : IRequestHandler<GetHotelQuery, HotelDto>
{
    private readonly IHotelRepository _repository;

    public async Task<HotelDto> Handle(GetHotelQuery request, CancellationToken cancellationToken)
    {
        var hotel = await _repository.GetByIdAsync(request.HotelId);

        if (hotel == null)
            throw new HotelNotFoundException(request.HotelId);

        return MapToDto(hotel);
    }
}
```

---

### Step 5: Handle in API Layer

```csharp
[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetHotel(int id)
    {
        try
        {
            var query = new GetHotelQuery { HotelId = id };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (HotelNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error getting hotel {HotelId}", id);
            return StatusCode(500, new { error = "An error occurred" });
        }
    }

    [HttpPut("{id}/price")]
    public async Task<IActionResult> UpdatePrice(int id, [FromBody] UpdatePriceRequest request)
    {
        try
        {
            var command = new UpdateHotelPriceCommand
            {
                HotelId = id,
                NewPrice = request.Price
            };
            await _mediator.Send(command);
            return NoContent();
        }
        catch (HotelNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidPriceException ex)
        {
            return BadRequest(new { error = ex.Message, invalidPrice = ex.Price });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating price");
            return StatusCode(500, new { error = "An error occurred" });
        }
    }
}
```

---

### Step 6: Global Exception Handler (Optional)

```csharp
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var response = exception switch
        {
            HotelNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            RoomTypeNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            InvalidPriceException ex => (StatusCodes.Status400BadRequest, ex.Message),
            DomainException ex => (StatusCodes.Status400BadRequest, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "An error occurred")
        };

        httpContext.Response.StatusCode = response.Item1;
        await httpContext.Response.WriteAsJsonAsync(new { error = response.Item2 }, cancellationToken);
        return true;
    }
}

// Register in Program.cs
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

---

## Real-World Examples

### Example 1: Hotel CRUD Operations

```csharp
// Domain Layer
public class Hotel : BaseEntity
{
    public void UpdateDetails(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Hotel name is required");

        Name = name;
        Description = description;
        UpdateTimestamp();
    }
}

// Application Layer
public class UpdateHotelCommandHandler : IRequestHandler<UpdateHotelCommand>
{
    public async Task Handle(UpdateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = await _repository.GetByIdAsync(request.HotelId);

        if (hotel == null)
            throw new HotelNotFoundException(request.HotelId);

        hotel.UpdateDetails(request.Name, request.Description);
        await _repository.UpdateAsync(hotel);
    }
}

// API Layer
[HttpPut("{id}")]
public async Task<IActionResult> UpdateHotel(int id, UpdateHotelRequest request)
{
    try
    {
        await _mediator.Send(new UpdateHotelCommand { HotelId = id, ...request });
        return NoContent();
    }
    catch (HotelNotFoundException ex)
    {
        return NotFound(ex.Message);  // 404: Hotel not found
    }
    catch (DomainException ex)
    {
        return BadRequest(ex.Message);  // 400: Validation error
    }
}
```

---

### Example 2: Price Validation

```csharp
public class RoomType : BaseEntity
{
    public decimal BasePrice { get; private set; }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new InvalidPriceException(newPrice);

        if (newPrice > 10000)
            throw new DomainException("Price cannot exceed 10,000");

        BasePrice = newPrice;
        UpdateTimestamp();
    }
}

// Test
[Fact]
public void UpdatePrice_WithNegativePrice_ThrowsInvalidPriceException()
{
    // Arrange
    var roomType = new RoomType(...);

    // Act & Assert
    var exception = Assert.Throws<InvalidPriceException>(
        () => roomType.UpdatePrice(-50)
    );

    Assert.Equal(-50, exception.Price);
    Assert.Contains("invalid", exception.Message, StringComparison.OrdinalIgnoreCase);
}
```

---

### Example 3: Inner Exception

```csharp
public class HotelRepository : IHotelRepository
{
    public async Task<Hotel> AddAsync(Hotel hotel)
    {
        try
        {
            await _context.Hotels.AddAsync(hotel);
            await _context.SaveChangesAsync();
            return hotel;
        }
        catch (DbUpdateException dbEx)
        {
            // Wrap database exception with domain context
            throw new DomainException(
                $"Failed to create hotel '{hotel.Name}'",
                dbEx  // Preserve original exception
            );
        }
    }
}

// Logging preserves full error details
catch (DomainException ex)
{
    _logger.LogError(ex, "Domain error: {Message}", ex.Message);
    // Logs both DomainException and inner DbUpdateException
}
```

---

## Best Practices

### 1. Create Specific Exceptions

```csharp
// ✅ GOOD: Specific
throw new HotelNotFoundException(id);
throw new InvalidPriceException(price);

// ❌ BAD: Generic
throw new DomainException("Error");
```

---

### 2. Include Context in Exception

```csharp
// ✅ GOOD: Includes relevant data
public class HotelNotFoundException : DomainException
{
    public int HotelId { get; }

    public HotelNotFoundException(int hotelId)
        : base($"Hotel with ID {hotelId} was not found")
    {
        HotelId = hotelId;  // Store for later use
    }
}

// ❌ BAD: No context
public class HotelNotFoundException : DomainException
{
    public HotelNotFoundException()
        : base("Hotel not found") { }
}
```

---

### 3. Use Meaningful Messages

```csharp
// ✅ GOOD: Clear, actionable
throw new DomainException("Hotel name is required. Please provide a valid name.");

// ❌ BAD: Vague
throw new DomainException("Invalid input");
```

---

### 4. Don't Overuse Custom Exceptions

```csharp
// ✅ GOOD: Use standard exceptions when appropriate
if (name == null)
    throw new ArgumentNullException(nameof(name));

if (price < 0)
    throw new ArgumentException("Price must be positive", nameof(price));

// ❌ BAD: Unnecessary custom exception
throw new NameNullException();
```

---

### 5. Catch from Most Specific to General

```csharp
// ✅ GOOD: Order matters
try { }
catch (HotelNotFoundException ex) { }      // Most specific
catch (RoomTypeNotFoundException ex) { }   // Specific
catch (DomainException ex) { }             // General domain
catch (Exception ex) { }                   // Most general

// ❌ BAD: Wrong order
try { }
catch (Exception ex) { }           // Catches everything!
catch (DomainException ex) { }     // Never reached
catch (HotelNotFoundException ex) { } // Never reached
```

---

## Summary

### Key Benefits:

| Benefit | Impact |
|---------|--------|
| **Type Safety** | Compiler catches missing handlers |
| **Clarity** | Clear what errors can occur |
| **Testability** | Easy to test specific scenarios |
| **Maintainability** | Centralized error definitions |
| **API Responses** | Proper HTTP status codes |
| **Debugging** | Clear error context |

### When to Use:

- ✅ Business rule violations
- ✅ Entity not found scenarios
- ✅ Domain validation failures
- ❌ System/infrastructure errors (use standard exceptions)
- ❌ Null checks (use `ArgumentNullException`)

### Exception Hierarchy:

```
Exception (C# built-in)
    ↓
DomainException (Your base)
    ↓
    ├── HotelNotFoundException
    ├── RoomTypeNotFoundException
    ├── InvalidPriceException
    └── DuplicateHotelNameException
```

---

## Further Reading

- [Microsoft: Exception Handling Best Practices](https://docs.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)
- [Domain-Driven Design: Exceptions](https://enterprisecraftsmanship.com/posts/exceptions-for-flow-control/)
- [Clean Code: Error Handling](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)