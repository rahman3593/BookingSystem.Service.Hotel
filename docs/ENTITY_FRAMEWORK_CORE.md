# Entity Framework Core - Essential Concepts

This document explains key Entity Framework Core concepts used in our Hotel Booking System.

---

## Table of Contents

1. [What is Entity Framework Core?](#what-is-entity-framework-core)
2. [DbContext](#dbcontext)
3. [Entity Configuration (Fluent API)](#entity-configuration-fluent-api)
4. [Query Filters (Soft Delete)](#query-filters-soft-delete)
5. [Foreign Keys and Relationships](#foreign-keys-and-relationships)
6. [Delete Behaviors](#delete-behaviors)
7. [Repository Pattern](#repository-pattern)
8. [Async Operations](#async-operations)
9. [Migrations](#migrations)

---

## What is Entity Framework Core?

**Entity Framework Core (EF Core)** is an Object-Relational Mapper (ORM) for .NET.

### What's an ORM?

An ORM translates between two worlds:
- **Your C# code** (objects, classes, properties)
- **Database** (tables, columns, rows)

### Without ORM (Raw SQL):
```csharp
// You write SQL manually
string sql = "SELECT * FROM Hotels WHERE Id = @id AND IsDeleted = 0";
var command = new SqlCommand(sql, connection);
command.Parameters.AddWithValue("@id", 10);
var reader = command.ExecuteReader();
// Manually map results to objects...
```

### With EF Core:
```csharp
// EF Core generates SQL automatically
var hotel = await _context.Hotels.FindAsync(10);
```

**EF Core generates this SQL for you:**
```sql
SELECT * FROM Hotels WHERE Id = 10 AND IsDeleted = 0
```

---

## DbContext

**DbContext** is the main class you use to interact with the database.

### What is DbContext?

Think of `DbContext` as:
- **Database Connection Manager** - Opens/closes connections
- **Query Builder** - Translates LINQ to SQL
- **Change Tracker** - Tracks which entities are modified
- **Transaction Manager** - Handles database transactions

### Our HotelDbContext:

```csharp
public class HotelDbContext : DbContext
{
    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options) { }

    // DbSet = a table in the database
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<RoomType> RoomTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically discover and apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
    }
}
```

### DbSet Explained:

```csharp
public DbSet<Hotel> Hotels { get; set; }
//         ↑        ↑
//      Entity    Property name (usually plural)
```

**What is DbSet?**
- Represents a **table** in the database
- Allows you to **query** and **save** entities
- Provides LINQ methods: `Where()`, `Select()`, `FirstOrDefault()`, etc.

**Example:**
```csharp
// Query
var activeHotels = await _context.Hotels
    .Where(h => h.Status == HotelStatus.Active)
    .ToListAsync();

// Add
var newHotel = new Hotel(...);
await _context.Hotels.AddAsync(newHotel);
await _context.SaveChangesAsync();
```

---

## Entity Configuration (Fluent API)

**Fluent API** is how you configure how entities map to database tables.

### Why Configure?

Without configuration, EF Core makes assumptions:
- Property names = Column names
- String properties = unlimited length (NVARCHAR(MAX))
- No indexes
- Basic relationships

**We configure to:**
- Set column constraints (length, required, etc.)
- Define relationships explicitly
- Add indexes for performance
- Set default values
- Configure data types

### Configuration Options:

#### Option 1: Data Annotations (Not Recommended)
```csharp
public class Hotel : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; }
}
```

**Problems:**
- Mixes domain logic with database concerns
- Violates Clean Architecture
- Less flexible

#### Option 2: Fluent API (Recommended ✅)
```csharp
public class HotelConfiguration : IEntityTypeConfiguration<Hotel>
{
    public void Configure(EntityTypeBuilder<Hotel> builder)
    {
        builder.Property(h => h.Name)
            .IsRequired()
            .HasMaxLength(200);
    }
}
```

**Benefits:**
- Separation of concerns
- More powerful and flexible
- Keeps domain entities clean

### Common Fluent API Methods:

```csharp
public void Configure(EntityTypeBuilder<Hotel> builder)
{
    // Table name
    builder.ToTable("Hotels");

    // Primary key
    builder.HasKey(h => h.Id);

    // Required field (NOT NULL)
    builder.Property(h => h.Name)
        .IsRequired();

    // String length
    builder.Property(h => h.Name)
        .HasMaxLength(200);

    // Decimal precision
    builder.Property(h => h.BasePrice)
        .HasColumnType("decimal(18,2)");

    // Enum to integer conversion
    builder.Property(h => h.Status)
        .HasConversion<int>();

    // Default value
    builder.Property(h => h.IsDeleted)
        .HasDefaultValue(false);

    // Index for performance
    builder.HasIndex(h => h.Name);

    // Composite index
    builder.HasIndex(h => new { h.City, h.Country });
}
```

### ApplyConfigurationsFromAssembly:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);
}
```

**What does this do?**

Instead of manually registering each configuration:
```csharp
// Manual way (tedious)
modelBuilder.ApplyConfiguration(new HotelConfiguration());
modelBuilder.ApplyConfiguration(new RoomTypeConfiguration());
modelBuilder.ApplyConfiguration(new BookingConfiguration());
// ... 50 more configurations
```

**Automatic discovery:**
```csharp
// Automatic way
modelBuilder.ApplyConfigurationsFromAssembly(typeof(HotelDbContext).Assembly);

// EF Core automatically finds all classes implementing IEntityTypeConfiguration<T>
// in the Persistence assembly and applies them
```

**How it works:**
1. EF Core scans the Persistence assembly
2. Finds all classes implementing `IEntityTypeConfiguration<T>`
3. Creates instances and calls `Configure()` method
4. Applies all configurations automatically

---

## Query Filters (Soft Delete)

**Query Filters** automatically add WHERE clauses to every query.

### The Problem: Soft Delete Without Filters

```csharp
// You have to manually check IsDeleted everywhere
public async Task<Hotel?> GetByIdAsync(int id)
{
    return await _context.Hotels
        .Where(h => !h.IsDeleted)  // ← Easy to forget!
        .FirstOrDefaultAsync(h => h.Id == id);
}

public async Task<List<Hotel>> GetAllAsync()
{
    return await _context.Hotels
        .Where(h => !h.IsDeleted)  // ← Easy to forget!
        .ToListAsync();
}
```

**Problem:** If you forget, you'll get deleted records!

### The Solution: Query Filter

```csharp
// In HotelConfiguration.cs
builder.HasQueryFilter(h => !h.IsDeleted);
```

**Now you can write:**
```csharp
public async Task<Hotel?> GetByIdAsync(int id)
{
    return await _context.Hotels
        .FirstOrDefaultAsync(h => h.Id == id);
    // EF Core automatically adds: WHERE IsDeleted = 0
}
```

### How Query Filters Work:

**Your Code:**
```csharp
var hotels = await _context.Hotels
    .Where(h => h.City == "Paris")
    .ToListAsync();
```

**Generated SQL:**
```sql
SELECT * FROM Hotels
WHERE City = 'Paris'
  AND IsDeleted = 0  ← Automatically added!
```

### Database State:
```
Hotels Table
┌────┬────────────────┬────────┬───────────┐
│ Id │ Name           │ City   │ IsDeleted │
├────┼────────────────┼────────┼───────────┤
│ 10 │ Hilton Paris   │ Paris  │ 0         │ ✅ Returned
│ 20 │ Old Paris      │ Paris  │ 1         │ ❌ Filtered out
│ 30 │ Marriott Tokyo │ Tokyo  │ 0         │ ❌ Different city
└────┴────────────────┴────────┴───────────┘
```

Only Hilton Paris is returned (not soft-deleted AND matches city).

### Bypassing Query Filter (When Needed):

```csharp
// Get ALL hotels including deleted ones
var allHotels = await _context.Hotels
    .IgnoreQueryFilters()  // ← Disables query filter
    .ToListAsync();
```

**Use cases:**
- Admin panel showing deleted records
- Restoring soft-deleted data
- Audit reports

---

## Foreign Keys and Relationships

**Foreign Keys** link tables together.

### What is a Foreign Key?

```csharp
public class RoomType : BaseEntity
{
    public int HotelId { get; private set; }  // ← Foreign Key
    // This room belongs to a specific hotel
}
```

### Database Representation:

```
Hotels Table (Parent)
┌────┬────────────────┐
│ Id │ Name           │
├────┼────────────────┤
│ 10 │ Hilton Paris   │
│ 20 │ Marriott Tokyo │
└────┴────────────────┘
     ↑
     │ Referenced by HotelId
     │
RoomTypes Table (Child)
┌────┬─────────────┬──────────┐
│ Id │ Name        │ HotelId  │
├────┼─────────────┼──────────┤
│ 1  │ Deluxe Room │ 10       │ ← Belongs to Hilton Paris
│ 2  │ Suite       │ 10       │ ← Belongs to Hilton Paris
│ 3  │ Standard    │ 20       │ ← Belongs to Marriott Tokyo
└────┴─────────────┴──────────┘
```

### Configuring Relationships:

```csharp
// In RoomTypeConfiguration.cs
builder.HasOne<Hotel>()           // RoomType has ONE Hotel
    .WithMany()                    // Hotel has MANY RoomTypes
    .HasForeignKey(rt => rt.HotelId)  // Using HotelId property
    .OnDelete(DeleteBehavior.Restrict);  // Delete behavior
```

### Relationship Types:

#### One-to-Many (Our case):
```csharp
// One Hotel has Many RoomTypes
Hotel (1) ----< (Many) RoomType
```

**Configuration:**
```csharp
builder.HasOne<Hotel>()
    .WithMany()
    .HasForeignKey(rt => rt.HotelId);
```

#### One-to-One:
```csharp
// One Hotel has One Address
Hotel (1) ---< (1) Address
```

**Configuration:**
```csharp
builder.HasOne<Address>()
    .WithOne()
    .HasForeignKey<Address>(a => a.HotelId);
```

#### Many-to-Many:
```csharp
// Many Hotels have Many Amenities
Hotel (Many) ---< (Many) Amenity
```

**Configuration:**
```csharp
builder.HasMany<Amenity>()
    .WithMany()
    .UsingEntity(j => j.ToTable("HotelAmenities"));
```

---

## Delete Behaviors

Controls what happens when you delete a parent record.

### DeleteBehavior.Restrict (Our choice ✅)

**Prevents deletion if child records exist.**

```csharp
.OnDelete(DeleteBehavior.Restrict)
```

**Example:**
```csharp
// Try to delete Hotel with RoomTypes
var hotel = await _context.Hotels.FindAsync(10);
_context.Hotels.Remove(hotel);

try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateException)
{
    // ❌ ERROR: Cannot delete Hotel
    // The DELETE statement conflicted with the REFERENCE constraint
    Console.WriteLine("Delete all room types first!");
}
```

**Why use Restrict?**
- **Data Safety** - Prevents accidental data loss
- **Explicit Control** - Forces you to handle cleanup explicitly
- **Business Logic** - You decide what happens to child records

---

### DeleteBehavior.Cascade

**Automatically deletes all child records.**

```csharp
.OnDelete(DeleteBehavior.Cascade)
```

**Example:**
```csharp
// Delete Hotel
var hotel = await _context.Hotels.FindAsync(10);
_context.Hotels.Remove(hotel);
await _context.SaveChangesAsync();

// ✅ SUCCESS
// Hotel AND all its RoomTypes are deleted automatically
```

**Dangerous!** You might lose data you didn't intend to delete.

---

### DeleteBehavior.SetNull

**Sets foreign key to NULL when parent deleted.**

```csharp
.OnDelete(DeleteBehavior.SetNull)
```

**Example:**
```csharp
// Delete Hotel
// All RoomTypes with HotelId = 10 now have HotelId = NULL
```

**Problem:** Only works if foreign key is nullable. Our `HotelId` is required, so this would fail.

---

### Comparison:

| Behavior | What Happens | Use Case |
|----------|-------------|----------|
| **Restrict** | Error if children exist | Critical data, explicit cleanup |
| **Cascade** | Auto-delete children | Logs, temporary data |
| **SetNull** | Set FK to NULL | Optional relationships |
| **NoAction** | Database handles it | Let DB enforce rules |

---

## Repository Pattern

**Repository Pattern** abstracts data access logic.

### Why Use Repository Pattern?

**Without Repository:**
```csharp
// DbContext used directly everywhere
public class CreateHotelCommandHandler
{
    private readonly HotelDbContext _context;

    public async Task<int> Handle(...)
    {
        var hotel = new Hotel(...);
        await _context.Hotels.AddAsync(hotel);  // ← Direct EF Core usage
        await _context.SaveChangesAsync();
        return hotel.Id;
    }
}
```

**Problems:**
- Application layer knows about EF Core
- Hard to test (need actual database)
- Violates Clean Architecture

---

**With Repository:**
```csharp
// Interface in Application layer
public interface IHotelRepository
{
    Task<Hotel> AddAsync(Hotel hotel);
}

// Implementation in Persistence layer
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

// Handler uses interface
public class CreateHotelCommandHandler
{
    private readonly IHotelRepository _repository;  // ← Interface, not DbContext

    public async Task<int> Handle(...)
    {
        var hotel = new Hotel(...);
        await _repository.AddAsync(hotel);  // ← No EF Core knowledge
        return hotel.Id;
    }
}
```

**Benefits:**
- ✅ Application layer doesn't know about EF Core
- ✅ Easy to test (mock the interface)
- ✅ Can swap implementations (SQL Server → PostgreSQL)
- ✅ Follows Clean Architecture

---

### Repository Implementation:

```csharp
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
            .FirstOrDefaultAsync(h => h.Id == id);
    }

    public async Task<Hotel> AddAsync(Hotel hotel)
    {
        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();  // Commits to database
        return hotel;  // hotel.Id is now populated
    }

    public async Task UpdateAsync(Hotel hotel)
    {
        _context.Hotels.Update(hotel);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var hotel = await GetByIdAsync(id);
        if (hotel == null)
            throw new HotelNotFoundException(id);

        hotel.MarkAsDeleted();  // Soft delete
        await _context.SaveChangesAsync();
    }
}
```

---

## Async Operations

**Why Async?**

### Synchronous (Blocking):
```csharp
// Thread is blocked waiting for database
var hotel = _context.Hotels.FirstOrDefault(h => h.Id == 10);
// ← Thread waits here (wasted resources)
```

**Problem:** Thread is blocked. Can't handle other requests.

---

### Asynchronous (Non-blocking):
```csharp
// Thread is released while waiting
var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == 10);
// ← Thread can handle other requests while waiting
```

**Benefit:** Better performance and scalability.

---

### Common Async Methods:

```csharp
// Query single result
var hotel = await _context.Hotels.FirstOrDefaultAsync(h => h.Id == 10);
var hotel = await _context.Hotels.SingleOrDefaultAsync(h => h.Id == 10);
var hotel = await _context.Hotels.FindAsync(10);

// Query multiple results
var hotels = await _context.Hotels.ToListAsync();
var hotels = await _context.Hotels.Where(h => h.City == "Paris").ToListAsync();

// Count
var count = await _context.Hotels.CountAsync();

// Check existence
var exists = await _context.Hotels.AnyAsync(h => h.Name == "Hilton");

// Add
await _context.Hotels.AddAsync(hotel);

// Save changes
await _context.SaveChangesAsync();
```

---

## Migrations

**Migrations** track database schema changes.

### What are Migrations?

Migrations are like **Git commits for your database schema**.

**Each migration contains:**
- Changes to make (Up method)
- How to undo changes (Down method)

### Migration Workflow:

```bash
# 1. Create initial migration
dotnet ef migrations add InitialCreate --project src/BookingSystem.Service.Hotel.Persistence --startup-project src/BookingSystem.Service.Hotel.API

# 2. Review generated migration file
# 3. Apply migration to database
dotnet ef database update --project src/BookingSystem.Service.Hotel.Persistence --startup-project src/BookingSystem.Service.Hotel.API
```

### What Gets Generated:

```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Hotels",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                City = table.Column<string>(maxLength: 100, nullable: false),
                // ... other columns
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Hotels", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Hotels");
    }
}
```

---

## Summary

| Concept | Purpose | Key Point |
|---------|---------|-----------|
| **DbContext** | Database connection manager | Main class for EF Core |
| **DbSet** | Represents a table | Allows queries and saves |
| **Fluent API** | Configure entities | Separation of concerns |
| **Query Filter** | Auto-filter queries | Soft delete made easy |
| **Foreign Key** | Link tables | Relationships between entities |
| **Delete Behavior** | Control cascading | Restrict = safest |
| **Repository** | Abstract data access | Clean Architecture compliance |
| **Async** | Non-blocking operations | Better performance |
| **Migrations** | Track schema changes | Version control for database |

---

## Best Practices

1. **Always use async methods** for database operations
2. **Configure entities with Fluent API**, not data annotations
3. **Use Query Filters** for soft delete
4. **Use Repository Pattern** to abstract EF Core
5. **Use Restrict delete behavior** for important relationships
6. **Create migrations** for every schema change
7. **Use `SaveChangesAsync()`** to commit changes
8. **Test with actual database** (not in-memory provider)

---

## Further Reading

- [EF Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [Fluent API Configuration](https://docs.microsoft.com/en-us/ef/core/modeling/)
- [Query Filters](https://docs.microsoft.com/en-us/ef/core/querying/filters)
- [Relationships](https://docs.microsoft.com/en-us/ef/core/modeling/relationships)
- [Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)