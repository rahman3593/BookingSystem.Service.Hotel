# Hotel Booking System - Hotel Service

A production-ready microservice for managing hotels, built with Clean Architecture, CQRS, and .NET 9.

---

## 📋 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technologies Used](#technologies-used)
- [Project Structure](#project-structure)
- [Features](#features)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
- [Database](#database)
- [Key Concepts](#key-concepts)
- [Documentation](#documentation)

---

## 🎯 Overview

This is the **Hotel Service** microservice of a complete Hotel Booking System. It provides full CRUD operations for managing hotels with features like:

- Create, Read, Update, Delete hotels
- Soft delete functionality (data preservation)
- Input validation
- Automatic query filtering
- RESTful API design
- Swagger/OpenAPI documentation

---

## 🏗️ Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
┌─────────────────────────────────────────────────┐
│                  API Layer                      │
│  (Controllers, Program.cs, Swagger)             │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│            Application Layer                    │
│  (Commands, Queries, Handlers, DTOs,            │
│   Validators, Mappings, Interfaces)             │
└──────────────────┬──────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────┐
│             Domain Layer                        │
│  (Entities, Enums, Value Objects,               │
│   Domain Exceptions, Business Rules)            │
└─────────────────────────────────────────────────┘
                   ▲
┌──────────────────┴──────────────────────────────┐
│           Persistence Layer                     │
│  (DbContext, Configurations, Repositories,      │
│   Migrations)                                   │
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities:

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **API** | HTTP endpoints, routing, Swagger | Application |
| **Application** | Use cases, business logic orchestration | Domain |
| **Domain** | Business entities, rules, exceptions | None (pure) |
| **Persistence** | Database access, EF Core | Domain, Application (interfaces) |

---

## 🛠️ Technologies Used

### Core Framework
- **.NET 9** - Latest .NET version
- **C# 13** - Modern C# features

### Patterns & Architecture
- **Clean Architecture** - Separation of concerns
- **CQRS** - Command Query Responsibility Segregation
- **Repository Pattern** - Data access abstraction
- **Mediator Pattern** - Request handling with MediatR

### Database & ORM
- **PostgreSQL** - Relational database
- **Entity Framework Core 9** - ORM
- **Npgsql** - PostgreSQL provider

### Libraries & Tools
- **MediatR** - Mediator pattern implementation
- **AutoMapper** - Object-to-object mapping
- **FluentValidation** - Input validation
- **Swashbuckle** - Swagger/OpenAPI documentation

---

## 📁 Project Structure

```
BookingSystem.Service.Hotel/
│
├── src/
│   ├── BookingSystem.Service.Hotel.Domain/
│   │   ├── Entities/
│   │   │   ├── BaseEntity.cs
│   │   │   ├── Hotel.cs
│   │   │   └── RoomType.cs
│   │   ├── Enums/
│   │   │   ├── HotelStatus.cs
│   │   │   └── StarRating.cs
│   │   └── Exceptions/
│   │       ├── DomainException.cs
│   │       ├── HotelNotFoundException.cs
│   │       └── RoomTypeNotFoundException.cs
│   │
│   ├── BookingSystem.Service.Hotel.Application/
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   └── IHotelRepository.cs
│   │   │   └── Mappings/
│   │   │       └── MappingProfile.cs
│   │   ├── DTOs/
│   │   │   └── HotelDto.cs
│   │   ├── Features/
│   │   │   └── Hotels/
│   │   │       ├── Commands/
│   │   │       │   ├── CreateHotel/
│   │   │       │   │   ├── CreateHotelCommand.cs
│   │   │       │   │   ├── CreateHotelCommandHandler.cs
│   │   │       │   │   └── CreateHotelCommandValidator.cs
│   │   │       │   ├── UpdateHotel/
│   │   │       │   │   ├── UpdateHotelCommand.cs
│   │   │       │   │   ├── UpdateHotelCommandHandler.cs
│   │   │       │   │   └── UpdateHotelCommandValidator.cs
│   │   │       │   └── DeleteHotel/
│   │   │       │       ├── DeleteHotelCommand.cs
│   │   │       │       └── DeleteHotelCommandHandler.cs
│   │   │       └── Queries/
│   │   │           ├── GetHotelById/
│   │   │           │   ├── GetHotelByIdQuery.cs
│   │   │           │   └── GetHotelByIdQueryHandler.cs
│   │   │           └── GetHotelsList/
│   │   │               ├── GetHotelsListQuery.cs
│   │   │               └── GetHotelsListQueryHandler.cs
│   │   └── DependencyInjection.cs
│   │
│   ├── BookingSystem.Service.Hotel.Persistence/
│   │   ├── Contexts/
│   │   │   └── HotelDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── HotelConfiguration.cs
│   │   │   └── RoomTypeConfiguration.cs
│   │   ├── Repositories/
│   │   │   └── HotelRepository.cs
│   │   ├── Migrations/
│   │   │   └── [EF Core migrations]
│   │   └── DependencyInjection.cs
│   │
│   └── BookingSystem.Service.Hotel.API/
│       ├── Controllers/
│       │   └── HotelsController.cs
│       ├── Program.cs
│       └── appsettings.json
│
└── docs/
    ├── PACKAGES_REFERENCE.md
    ├── CSHARP_CLASSES_GUIDE.md
    ├── DOMAIN_DRIVEN_DESIGN.md
    ├── CONSTRUCTOR_PATTERNS.md
    ├── DOMAIN_EXCEPTIONS.md
    ├── MEDIATR_CQRS.md
    ├── ENTITY_FRAMEWORK_CORE.md
    ├── DEPENDENCY_INJECTION.md
    └── ASPNET_CORE_CONTROLLERS.md
```

---

## ✨ Features

### CRUD Operations
- ✅ **Create** - Add new hotels with validation
- ✅ **Read** - Retrieve all hotels or specific hotel by ID
- ✅ **Update** - Modify existing hotel details
- ✅ **Delete** - Soft delete hotels (preserves data)

### Data Validation
- Required fields validation (Name, City, Country)
- Email format validation
- String length constraints
- Enum validation

### Data Persistence
- Soft delete implementation
- Automatic query filtering (excludes deleted records)
- Audit fields (CreatedAt, UpdatedAt)
- Foreign key relationships

### API Features
- RESTful API design
- Swagger/OpenAPI documentation
- Proper HTTP status codes
- Error handling

---

## 🔌 API Endpoints

Base URL: `https://localhost:5001/api`

### Hotels

| Method | Endpoint | Description | Status Codes |
|--------|----------|-------------|--------------|
| GET | `/hotels` | Get all active hotels | 200 OK |
| GET | `/hotels/{id}` | Get hotel by ID | 200 OK, 404 Not Found |
| POST | `/hotels` | Create new hotel | 201 Created, 400 Bad Request |
| PUT | `/hotels/{id}` | Update hotel | 204 No Content, 400 Bad Request, 404 Not Found |
| DELETE | `/hotels/{id}` | Delete hotel (soft) | 204 No Content, 404 Not Found |

### Example Requests

#### Create Hotel
```http
POST /api/hotels
Content-Type: application/json

{
  "name": "Hilton Paris",
  "description": "Luxury 5-star hotel",
  "starRating": 5,
  "city": "Paris",
  "country": "France",
  "street": "123 Champs-Élysées",
  "state": "Île-de-France",
  "zipCode": "75008",
  "email": "contact@hiltonparis.com",
  "phone": "+33 1 23 45 67 89",
  "website": "https://hiltonparis.com"
}
```

#### Update Hotel
```http
PUT /api/hotels/1
Content-Type: application/json

{
  "id": 1,
  "name": "Hilton Paris - Updated",
  "description": "Newly renovated luxury hotel",
  "starRating": 5,
  "status": 1,
  "city": "Paris",
  "country": "France",
  "street": "123 Champs-Élysées",
  "state": "Île-de-France",
  "zipCode": "75008",
  "email": "updated@hiltonparis.com",
  "phone": "+33 1 99 99 99 99",
  "website": "https://hiltonparis-updated.com"
}
```

---

## 🚀 Getting Started

### Prerequisites

- **.NET 9 SDK** - [Download](https://dotnet.microsoft.com/download/dotnet/9.0)
- **PostgreSQL** - [Download](https://www.postgresql.org/download/)
- **IDE** - Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/rahman3593/BookingSystem.Service.Hotel.git
   cd BookingSystem.Service.Hotel
   ```

2. **Update connection string**

   Edit `src/BookingSystem.Service.Hotel.API/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "HotelDatabase": "Host=localhost;Port=5432;Database=HotelBookingDB;Username=postgres;Password=your_password"
     }
   }
   ```

3. **Install dependencies**
   ```bash
   dotnet restore
   ```

4. **Apply database migrations**
   ```bash
   dotnet ef database update --project src/BookingSystem.Service.Hotel.Persistence --startup-project src/BookingSystem.Service.Hotel.API
   ```

5. **Run the application**
   ```bash
   dotnet run --project src/BookingSystem.Service.Hotel.API
   ```

6. **Access Swagger UI**
   ```
   https://localhost:5001/swagger
   ```

---

## 🗄️ Database

### Database Schema

#### Hotels Table
```sql
CREATE TABLE "Hotels" (
    "Id" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000),
    "StarRating" INTEGER NOT NULL,
    "Status" INTEGER NOT NULL,
    "Street" VARCHAR(200),
    "City" VARCHAR(100) NOT NULL,
    "State" VARCHAR(100),
    "Country" VARCHAR(100) NOT NULL,
    "ZipCode" VARCHAR(20),
    "Email" VARCHAR(100),
    "Phone" VARCHAR(20),
    "Website" VARCHAR(200),
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX "IX_Hotels_Name" ON "Hotels" ("Name");
CREATE INDEX "IX_Hotels_City" ON "Hotels" ("City");
CREATE INDEX "IX_Hotels_IsDeleted" ON "Hotels" ("IsDeleted");
```

#### RoomTypes Table
```sql
CREATE TABLE "RoomTypes" (
    "Id" SERIAL PRIMARY KEY,
    "HotelId" INTEGER NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500),
    "MaxOccupancy" INTEGER NOT NULL,
    "BasePrice" NUMERIC(18,2) NOT NULL,
    "Size" NUMERIC(10,2) NOT NULL,
    "BedType" VARCHAR(50) NOT NULL,
    "ViewType" VARCHAR(50),
    "HasBalcony" BOOLEAN NOT NULL DEFAULT FALSE,
    "HasKitchen" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsSmokingAllowed" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL,
    "UpdatedAt" TIMESTAMP,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,

    CONSTRAINT "FK_RoomTypes_Hotels_HotelId"
        FOREIGN KEY ("HotelId")
        REFERENCES "Hotels" ("Id")
        ON DELETE RESTRICT
);

CREATE INDEX "IX_RoomTypes_HotelId" ON "RoomTypes" ("HotelId");
CREATE INDEX "IX_RoomTypes_Name" ON "RoomTypes" ("Name");
CREATE INDEX "IX_RoomTypes_IsDeleted" ON "RoomTypes" ("IsDeleted");
```

### Entity Relationships

```
Hotel (1) ----< (Many) RoomType
```

- One Hotel can have many RoomTypes
- Each RoomType belongs to one Hotel
- Delete behavior: Restrict (prevents accidental deletion)

---

## 🧠 Key Concepts

### 1. Clean Architecture

**Principle:** Separation of concerns with dependency inversion.

**Layers:**
- **Domain:** Pure business logic, no dependencies
- **Application:** Use cases, orchestrates domain logic
- **Persistence:** Database access, implements repositories
- **API:** HTTP endpoints, user interface

**Benefits:**
- Testable
- Maintainable
- Framework-independent
- UI-independent
- Database-independent

**Learn more:** [docs/DOMAIN_DRIVEN_DESIGN.md](docs/DOMAIN_DRIVEN_DESIGN.md)

---

### 2. CQRS (Command Query Responsibility Segregation)

**Principle:** Separate read and write operations.

**Commands (Write):**
- `CreateHotelCommand` - Creates a hotel
- `UpdateHotelCommand` - Updates a hotel
- `DeleteHotelCommand` - Deletes a hotel

**Queries (Read):**
- `GetHotelByIdQuery` - Retrieves one hotel
- `GetHotelsListQuery` - Retrieves all hotels

**Benefits:**
- Clear separation of concerns
- Easier to optimize (different models for read/write)
- Better scalability
- Easier testing

**Learn more:** [docs/MEDIATR_CQRS.md](docs/MEDIATR_CQRS.md)

---

### 3. Repository Pattern

**Principle:** Abstract data access logic.

**Interface (Application Layer):**
```csharp
public interface IHotelRepository
{
    Task<Hotel?> GetByIdAsync(int id);
    Task<List<Hotel>> GetAllAsync();
    Task<Hotel> AddAsync(Hotel hotel);
    Task UpdateAsync(Hotel hotel);
    Task DeleteAsync(int id);
}
```

**Implementation (Persistence Layer):**
```csharp
public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;
    // ... implementation
}
```

**Benefits:**
- Testable (can mock repository)
- Decouples application from database
- Easy to swap implementations
- Follows Clean Architecture

---

### 4. Dependency Injection

**Principle:** Inject dependencies rather than creating them.

**Registration (by layer):**

```csharp
// Application Layer
services.AddApplication();  // MediatR, AutoMapper, Validators

// Persistence Layer
services.AddPersistence(configuration);  // DbContext, Repositories

// API Layer
services.AddControllers();  // Controllers
```

**Service Lifetimes:**
- **Transient:** New instance every time
- **Scoped:** One instance per HTTP request
- **Singleton:** One instance for app lifetime

**Learn more:** [docs/DEPENDENCY_INJECTION.md](docs/DEPENDENCY_INJECTION.md)

---

### 5. Entity Framework Core

**Principle:** ORM for database operations.

**Key Features Used:**

**Fluent API Configuration:**
```csharp
public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.ToTable("Hotels");
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Name).IsRequired().HasMaxLength(200);
        // ... more configuration
    }
}
```

**Query Filters (Soft Delete):**
```csharp
builder.HasQueryFilter(h => !h.IsDeleted);
// Automatically excludes deleted records from all queries
```

**Migrations:**
```bash
# Create migration
dotnet ef migrations add InitialCreate

# Apply migration
dotnet ef database update
```

**Learn more:** [docs/ENTITY_FRAMEWORK_CORE.md](docs/ENTITY_FRAMEWORK_CORE.md)

---

### 6. Soft Delete

**Principle:** Mark records as deleted instead of removing them.

**Implementation:**
```csharp
public abstract class BaseEntity
{
    public bool IsDeleted { get; protected set; }

    public void MarkAsDeleted()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
```

**Query Filter:**
```csharp
// In configuration
builder.HasQueryFilter(h => !h.IsDeleted);

// Queries automatically exclude soft-deleted records
var hotels = await _context.Hotels.ToListAsync();  // Only active hotels
```

**Benefits:**
- Data preservation
- Audit trail
- Can restore deleted data
- Maintains referential integrity

---

### 7. Validation

**Principle:** Validate input at API boundary.

**FluentValidation:**
```csharp
public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
```

**Automatic Validation:**
- `[ApiController]` attribute enables automatic validation
- Returns 400 Bad Request with validation errors
- Prevents invalid data from reaching business logic

---

### 8. AutoMapper

**Principle:** Separate entity models from DTOs.

**Mapping Configuration:**
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Hotel, HotelDto>()
            .ForMember(dest => dest.Status,
                       opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
```

**Usage:**
```csharp
var hotelDto = _mapper.Map<HotelDto>(hotel);
```

**Benefits:**
- Separates internal models from API contracts
- Reduces boilerplate code
- Type-safe mapping
- Easy to maintain

---

## 📚 Documentation

Detailed documentation available in the `docs/` folder:

### Core Concepts
- **[DOMAIN_DRIVEN_DESIGN.md](docs/DOMAIN_DRIVEN_DESIGN.md)** - DDD concepts, entities, value objects
- **[MEDIATR_CQRS.md](docs/MEDIATR_CQRS.md)** - CQRS pattern with MediatR
- **[ENTITY_FRAMEWORK_CORE.md](docs/ENTITY_FRAMEWORK_CORE.md)** - EF Core, migrations, query filters
- **[DEPENDENCY_INJECTION.md](docs/DEPENDENCY_INJECTION.md)** - DI patterns and service lifetimes

### Implementation Guides
- **[ASPNET_CORE_CONTROLLERS.md](docs/ASPNET_CORE_CONTROLLERS.md)** - Controllers, ActionResult, HTTP methods
- **[CONSTRUCTOR_PATTERNS.md](docs/CONSTRUCTOR_PATTERNS.md)** - Private constructors, EF Core compatibility
- **[DOMAIN_EXCEPTIONS.md](docs/DOMAIN_EXCEPTIONS.md)** - Custom exceptions vs general exceptions

### Reference
- **[PACKAGES_REFERENCE.md](docs/PACKAGES_REFERENCE.md)** - All NuGet packages explained
- **[CSHARP_CLASSES_GUIDE.md](docs/CSHARP_CLASSES_GUIDE.md)** - C# class types and characteristics

---

## 🎯 What We've Built

### Architecture Patterns
✅ Clean Architecture (4 layers)
✅ CQRS with MediatR
✅ Repository Pattern
✅ Dependency Injection
✅ Domain-Driven Design

### Technical Features
✅ Full CRUD operations
✅ Soft delete with query filters
✅ Input validation with FluentValidation
✅ Object mapping with AutoMapper
✅ RESTful API design
✅ Swagger/OpenAPI documentation
✅ Entity Framework Core migrations
✅ PostgreSQL database

### Best Practices
✅ Separation of concerns
✅ Dependency inversion
✅ Single responsibility principle
✅ Async/await operations
✅ Proper error handling
✅ Comprehensive documentation

---

## 🚀 Next Steps

### Week 1-2 (Current) - Complete ✅
- ✅ Hotel Service with full CRUD
- ✅ Clean Architecture setup
- ✅ Database integration
- ✅ API documentation

### Week 3-4 - Booking Service
- [ ] Create Booking microservice
- [ ] Implement booking CRUD
- [ ] Add availability checking
- [ ] Integrate with Hotel Service

### Week 5-6 - API Gateway
- [ ] Set up Ocelot API Gateway
- [ ] Configure routing
- [ ] Add load balancing
- [ ] Implement rate limiting

---

## 📝 License

This project is part of a learning journey to master microservices architecture with .NET.

---

## 👤 Author

**Abdul Rahman**
- GitHub: [@rahman3593](https://github.com/rahman3593)

---

## 🙏 Acknowledgments

- Clean Architecture principles by Robert C. Martin
- CQRS pattern implementation with MediatR
- Entity Framework Core documentation
- .NET community best practices

---

**Built with ❤️ using .NET 9**