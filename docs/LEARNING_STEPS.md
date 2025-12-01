# Phase 1 - Step-by-Step Implementation Guide

This document provides detailed steps for implementing the Hotel Service from scratch.

---

## Table of Contents

1. [Step 0: Prerequisites & Setup](#step-0-prerequisites--setup)
2. [Step 1: Project References & NuGet Packages](#step-1-project-references--nuget-packages)
3. [Step 2: Domain Layer](#step-2-domain-layer)
4. [Step 3: Application Layer](#step-3-application-layer)
5. [Step 4: Persistence Layer](#step-4-persistence-layer)
6. [Step 5: Infrastructure Layer](#step-5-infrastructure-layer)
7. [Step 6: API Layer](#step-6-api-layer)
8. [Step 7: Testing](#step-7-testing)
9. [Step 8: Docker & Deployment](#step-8-docker--deployment)

---

## Step 0: Prerequisites & Setup

### Prerequisites

1. **VS Code Extensions Installed:**
   - C# Dev Kit
   - C#
   - NuGet Package Manager
   - Docker
   - REST Client or Thunder Client

2. **.NET 8 SDK Installed:**
   ```bash
   dotnet --version  # Should show 8.0.x
   ```

3. **Docker Desktop Running:**
   ```bash
   docker --version
   docker-compose --version
   ```

### Project Structure Already Created

If you followed the initial setup, you should have:
```
D:\rahman3593\Dotnet\BookingSystem.Service.Hotel\
├── BookingSystem.Service.Hotel.sln
├── src\
│   ├── BookingSystem.Service.Hotel.Domain\
│   ├── BookingSystem.Service.Hotel.Application\
│   ├── BookingSystem.Service.Hotel.Persistence\
│   ├── BookingSystem.Service.Hotel.Infrastructure\
│   └── BookingSystem.Service.Hotel.Api\
└── tests\
    ├── BookingSystem.Service.Hotel.UnitTests\
    ├── BookingSystem.Service.Hotel.IntegrationTests\
    └── BookingSystem.Service.Hotel.ArchitectureTests\
```

---

## Step 1: Project References & NuGet Packages

### 1.1: Add Project References

Run these commands from the solution root:

```bash
cd D:\rahman3593\Dotnet\BookingSystem.Service.Hotel

# Application depends on Domain
dotnet add src/BookingSystem.Service.Hotel.Application reference src/BookingSystem.Service.Hotel.Domain

# Persistence depends on Domain and Application
dotnet add src/BookingSystem.Service.Hotel.Persistence reference src/BookingSystem.Service.Hotel.Domain
dotnet add src/BookingSystem.Service.Hotel.Persistence reference src/BookingSystem.Service.Hotel.Application

# Infrastructure depends on Application
dotnet add src/BookingSystem.Service.Hotel.Infrastructure reference src/BookingSystem.Service.Hotel.Application

# API depends on all layers
dotnet add src/BookingSystem.Service.Hotel.Api reference src/BookingSystem.Service.Hotel.Application
dotnet add src/BookingSystem.Service.Hotel.Api reference src/BookingSystem.Service.Hotel.Infrastructure
dotnet add src/BookingSystem.Service.Hotel.Api reference src/BookingSystem.Service.Hotel.Persistence

# Test projects reference what they test
dotnet add tests/BookingSystem.Service.Hotel.UnitTests reference src/BookingSystem.Service.Hotel.Application
dotnet add tests/BookingSystem.Service.Hotel.UnitTests reference src/BookingSystem.Service.Hotel.Domain

dotnet add tests/BookingSystem.Service.Hotel.IntegrationTests reference src/BookingSystem.Service.Hotel.Api

dotnet add tests/BookingSystem.Service.Hotel.ArchitectureTests reference src/BookingSystem.Service.Hotel.Domain
dotnet add tests/BookingSystem.Service.Hotel.ArchitectureTests reference src/BookingSystem.Service.Hotel.Application
dotnet add tests/BookingSystem.Service.Hotel.ArchitectureTests reference src/BookingSystem.Service.Hotel.Persistence
dotnet add tests/BookingSystem.Service.Hotel.ArchitectureTests reference src/BookingSystem.Service.Hotel.Api
```

### 1.2: Install NuGet Packages

**Domain Layer** (No packages needed - pure C#)

**Application Layer:**
```bash
cd src/BookingSystem.Service.Hotel.Application
dotnet add package MediatR
dotnet add package FluentValidation
dotnet add package FluentValidation.DependencyInjectionExtensions
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
cd ../..
```

**Persistence Layer:**
```bash
cd src/BookingSystem.Service.Hotel.Persistence
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Tools
cd ../..
```

**Infrastructure Layer:**
```bash
cd src/BookingSystem.Service.Hotel.Infrastructure
dotnet add package Microsoft.Extensions.Http
cd ../..
```

**API Layer:**
```bash
cd src/BookingSystem.Service.Hotel.Api
dotnet add package Swashbuckle.AspNetCore
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Microsoft.EntityFrameworkCore.Design
cd ../..
```

**Unit Tests:**
```bash
cd tests/BookingSystem.Service.Hotel.UnitTests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Moq
dotnet add package FluentAssertions
cd ../..
```

**Integration Tests:**
```bash
cd tests/BookingSystem.Service.Hotel.IntegrationTests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package FluentAssertions
dotnet add package Testcontainers
dotnet add package Testcontainers.PostgreSql
cd ../..
```

**Architecture Tests:**
```bash
cd tests/BookingSystem.Service.Hotel.ArchitectureTests
dotnet add package xunit
dotnet add package xunit.runner.visualstudio
dotnet add package Microsoft.NET.Test.Sdk
dotnet add package NetArchTest.Rules
dotnet add package FluentAssertions
cd ../..
```

### 1.3: Verify Installation

```bash
dotnet restore
dotnet build
```

You should see "Build succeeded" with no errors.

---

## Step 2: Domain Layer

The Domain layer is the heart of your application - pure business logic with no external dependencies.

### 2.1: Create BaseEntity

**File:** `src/BookingSystem.Service.Hotel.Domain/Entities/BaseEntity.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Entities;

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
```

**What this does:**
- Base class for all entities
- Provides common properties (Id, timestamps, soft delete)
- `protected set` prevents external modification

### 2.2: Create ValueObject Base Class

**File:** `src/BookingSystem.Service.Hotel.Domain/ValueObjects/ValueObject.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.ValueObjects;

public abstract class ValueObject
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (ValueObject)obj;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null ^ right is null)
            return false;

        return left?.Equals(right) != false;
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
```

**What this does:**
- Base class for all value objects
- Implements equality based on values, not reference
- Two value objects are equal if all their properties match

### 2.3: Create Enums

**File:** `src/BookingSystem.Service.Hotel.Domain/Enums/HotelStatus.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Enums;

public enum HotelStatus
{
    Active = 1,
    Inactive = 2,
    UnderMaintenance = 3
}
```

**File:** `src/BookingSystem.Service.Hotel.Domain/Enums/StarRating.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Enums;

public enum StarRating
{
    OneStar = 1,
    TwoStar = 2,
    ThreeStar = 3,
    FourStar = 4,
    FiveStar = 5
}
```

### 2.4: Create Address Value Object

**File:** `src/BookingSystem.Service.Hotel.Domain/ValueObjects/Address.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.ValueObjects;

public class Address : ValueObject
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }

    private Address() { } // For EF Core

    public Address(string street, string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City is required", nameof(city));

        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country is required", nameof(country));

        Street = street ?? string.Empty;
        City = city;
        State = state ?? string.Empty;
        Country = country;
        ZipCode = zipCode ?? string.Empty;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Street;
        yield return City;
        yield return State;
        yield return Country;
        yield return ZipCode;
    }
}
```

**What this does:**
- Immutable value object for addresses
- Validates required fields (City, Country)
- Private parameterless constructor for EF Core

### 2.5: Create ContactInfo Value Object

**File:** `src/BookingSystem.Service.Hotel.Domain/ValueObjects/ContactInfo.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.ValueObjects;

public class ContactInfo : ValueObject
{
    public string Email { get; private set; }
    public string Phone { get; private set; }
    public string Website { get; private set; }

    private ContactInfo() { } // For EF Core

    public ContactInfo(string email, string phone, string website)
    {
        Email = email ?? string.Empty;
        Phone = phone ?? string.Empty;
        Website = website ?? string.Empty;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Email;
        yield return Phone;
        yield return Website;
    }
}
```

### 2.6: Create Hotel Entity

**File:** `src/BookingSystem.Service.Hotel.Domain/Entities/Hotel.cs`

```csharp
using BookingSystem.Service.Hotel.Domain.Enums;
using BookingSystem.Service.Hotel.Domain.ValueObjects;

namespace BookingSystem.Service.Hotel.Domain.Entities;

public class Hotel : BaseEntity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public StarRating StarRating { get; private set; }
    public HotelStatus Status { get; private set; }
    public Address Address { get; private set; }
    public ContactInfo ContactInfo { get; private set; }

    private readonly List<RoomType> _roomTypes = new();
    public IReadOnlyCollection<RoomType> RoomTypes => _roomTypes.AsReadOnly();

    private Hotel() { } // For EF Core

    public Hotel(
        string name,
        string description,
        StarRating starRating,
        Address address,
        ContactInfo contactInfo)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name is required", nameof(name));

        if (address == null)
            throw new ArgumentNullException(nameof(address));

        Name = name;
        Description = description ?? string.Empty;
        StarRating = starRating;
        Address = address;
        ContactInfo = contactInfo ?? new ContactInfo(string.Empty, string.Empty, string.Empty);
        Status = HotelStatus.Active;
    }

    public void UpdateDetails(string name, string description, StarRating starRating)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hotel name is required", nameof(name));

        Name = name;
        Description = description;
        StarRating = starRating;
        UpdateTimestamp();
    }

    public void UpdateAddress(Address address)
    {
        Address = address ?? throw new ArgumentNullException(nameof(address));
        UpdateTimestamp();
    }

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        UpdateTimestamp();
    }

    public void ChangeStatus(HotelStatus status)
    {
        Status = status;
        UpdateTimestamp();
    }

    public void AddRoomType(RoomType roomType)
    {
        if (roomType == null)
            throw new ArgumentNullException(nameof(roomType));

        if (_roomTypes.Any(rt => rt.Name.Equals(roomType.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Room type '{roomType.Name}' already exists");

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
}
```

**What this does:**
- Aggregate root for Hotel
- Encapsulates business rules
- Private setters prevent direct modification
- Business methods enforce invariants

### 2.7: Create RoomType Entity

**File:** `src/BookingSystem.Service.Hotel.Domain/Entities/RoomType.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Entities;

public class RoomType : BaseEntity
{
    public int HotelId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int MaxOccupancy { get; private set; }
    public decimal BasePrice { get; private set; }
    public decimal Size { get; private set; } // Square meters
    public string BedType { get; private set; }
    public string ViewType { get; private set; }
    public bool HasBalcony { get; private set; }
    public bool HasKitchen { get; private set; }
    public bool IsSmokingAllowed { get; private set; }

    private RoomType() { } // For EF Core

    public RoomType(
        int hotelId,
        string name,
        string description,
        int maxOccupancy,
        decimal basePrice,
        decimal size,
        string bedType,
        string viewType,
        bool hasBalcony = false,
        bool hasKitchen = false,
        bool isSmokingAllowed = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name is required", nameof(name));

        if (maxOccupancy <= 0)
            throw new ArgumentException("Max occupancy must be greater than zero", nameof(maxOccupancy));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative", nameof(basePrice));

        HotelId = hotelId;
        Name = name;
        Description = description ?? string.Empty;
        MaxOccupancy = maxOccupancy;
        BasePrice = basePrice;
        Size = size;
        BedType = bedType ?? string.Empty;
        ViewType = viewType ?? string.Empty;
        HasBalcony = hasBalcony;
        HasKitchen = hasKitchen;
        IsSmokingAllowed = isSmokingAllowed;
    }

    public void UpdateDetails(
        string name,
        string description,
        int maxOccupancy,
        decimal basePrice)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Room type name is required", nameof(name));

        if (maxOccupancy <= 0)
            throw new ArgumentException("Max occupancy must be greater than zero", nameof(maxOccupancy));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative", nameof(basePrice));

        Name = name;
        Description = description;
        MaxOccupancy = maxOccupancy;
        BasePrice = basePrice;
        UpdateTimestamp();
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative", nameof(newPrice));

        BasePrice = newPrice;
        UpdateTimestamp();
    }
}
```

### 2.8: Create Domain Exceptions

**File:** `src/BookingSystem.Service.Hotel.Domain/Exceptions/DomainException.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
```

**File:** `src/BookingSystem.Service.Hotel.Domain/Exceptions/HotelNotFoundException.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Exceptions;

public class HotelNotFoundException : DomainException
{
    public HotelNotFoundException(int hotelId)
        : base($"Hotel with ID {hotelId} was not found")
    {
    }

    public HotelNotFoundException(string message) : base(message)
    {
    }
}
```

**File:** `src/BookingSystem.Service.Hotel.Domain/Exceptions/RoomTypeNotFoundException.cs`

```csharp
namespace BookingSystem.Service.Hotel.Domain.Exceptions;

public class RoomTypeNotFoundException : DomainException
{
    public RoomTypeNotFoundException(int roomTypeId)
        : base($"Room type with ID {roomTypeId} was not found")
    {
    }

    public RoomTypeNotFoundException(string message) : base(message)
    {
    }
}
```

### ✅ Domain Layer Complete!

Your Domain layer now contains:
- ✅ BaseEntity with common properties
- ✅ ValueObject base class
- ✅ Enums (HotelStatus, StarRating)
- ✅ Value Objects (Address, ContactInfo)
- ✅ Entities (Hotel, RoomType)
- ✅ Domain Exceptions

**Test it:**
```bash
dotnet build src/BookingSystem.Service.Hotel.Domain
```

You should see "Build succeeded" with no errors.

---

## Next Steps

In **Step 3: Application Layer**, you'll learn:
- Creating Commands and Queries (CQRS)
- Implementing MediatR handlers
- Adding FluentValidation
- Creating DTOs
- AutoMapper profiles

Ready to continue? Let me know when you've completed Step 2, or if you have any questions!
