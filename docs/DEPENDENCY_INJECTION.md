# Dependency Injection in .NET

This document explains Dependency Injection (DI) concepts used in our Hotel Booking System.

---

## Table of Contents

1. [What is Dependency Injection?](#what-is-dependency-injection)
2. [Why Use Dependency Injection?](#why-use-dependency-injection)
3. [Service Lifetimes](#service-lifetimes)
4. [Registration Methods](#registration-methods)
5. [Extension Methods Pattern](#extension-methods-pattern)
6. [Layer-Specific DI Setup](#layer-specific-di-setup)
7. [Constructor Injection](#constructor-injection)
8. [Testing with DI](#testing-with-di)
9. [Common Patterns](#common-patterns)

---

## What is Dependency Injection?

**Dependency Injection (DI)** is a design pattern where objects receive their dependencies from external sources rather than creating them internally.

### Without DI (Bad):

```csharp
public class CreateHotelCommandHandler
{
    private readonly HotelRepository _repository;

    public CreateHotelCommandHandler()
    {
        // Creating dependency inside the class
        var connectionString = "Server=localhost;Database=HotelDB;";
        var options = new DbContextOptions<HotelDbContext>();
        var context = new HotelDbContext(options);
        _repository = new HotelRepository(context);  // ❌ Tightly coupled
    }
}
```

**Problems:**
- ❌ Hard to test (can't replace with mock)
- ❌ Tightly coupled to specific implementation
- ❌ Hard to change database or repository
- ❌ Creates new instance every time
- ❌ Hard-coded dependencies

---

### With DI (Good):

```csharp
public class CreateHotelCommandHandler
{
    private readonly IHotelRepository _repository;

    public CreateHotelCommandHandler(IHotelRepository repository)
    {
        _repository = repository;  // ✅ Injected from outside
    }
}
```

**Benefits:**
- ✅ Easy to test (inject mocks)
- ✅ Loosely coupled (depends on interface)
- ✅ Easy to swap implementations
- ✅ Framework manages lifetime
- ✅ Configuration-based dependencies

---

## Why Use Dependency Injection?

### 1. **Testability**

**Without DI:**
```csharp
public class OrderService
{
    private readonly PaymentGateway _gateway = new PaymentGateway();  // ❌

    public void ProcessOrder()
    {
        _gateway.Charge(100);  // Charges real credit card in tests!
    }
}
```

**With DI:**
```csharp
public class OrderService
{
    private readonly IPaymentGateway _gateway;

    public OrderService(IPaymentGateway gateway)
    {
        _gateway = gateway;  // ✅ Can inject mock
    }

    public void ProcessOrder()
    {
        _gateway.Charge(100);  // Uses mock in tests
    }
}

// In tests:
var mockGateway = new Mock<IPaymentGateway>();
var service = new OrderService(mockGateway.Object);
```

---

### 2. **Loose Coupling**

**Without DI:**
```csharp
public class HotelService
{
    private SqlServerRepository _repo = new SqlServerRepository();  // ❌ Locked to SQL Server

    // Hard to switch to PostgreSQL
}
```

**With DI:**
```csharp
public class HotelService
{
    private readonly IHotelRepository _repo;

    public HotelService(IHotelRepository repo)
    {
        _repo = repo;  // ✅ Can be any implementation
    }
}

// Easy to switch:
// services.AddScoped<IHotelRepository, SqlServerRepository>();
// services.AddScoped<IHotelRepository, PostgreSqlRepository>();
// services.AddScoped<IHotelRepository, MongoDbRepository>();
```

---

### 3. **Single Responsibility**

Classes focus on their core logic, not on creating dependencies.

**Without DI:**
```csharp
public class EmailService
{
    private SmtpClient _client;

    public EmailService()
    {
        // Responsible for configuration AND email sending
        _client = new SmtpClient
        {
            Host = "smtp.gmail.com",
            Port = 587,
            Credentials = new NetworkCredential("user", "pass")
        };
    }

    public void Send(string to, string message)
    {
        // Send email
    }
}
```

**With DI:**
```csharp
public class EmailService
{
    private readonly ISmtpClient _client;

    public EmailService(ISmtpClient client)
    {
        _client = client;  // Configuration handled elsewhere
    }

    public void Send(string to, string message)
    {
        // Only responsible for email sending
    }
}
```

---

## Service Lifetimes

.NET Core provides three service lifetimes:

### 1. Transient (`AddTransient`)

**Creates a NEW instance every time it's requested.**

```csharp
services.AddTransient<IEmailService, EmailService>();
```

**Lifetime Diagram:**
```
Request 1:
  Controller → EmailService (Instance A)
  Controller → EmailService (Instance B)  ← Different instance

Request 2:
  Controller → EmailService (Instance C)  ← New request, new instance
```

**When to use:**
- Lightweight, stateless services
- Services that are cheap to create
- No shared state needed

**Example:**
```csharp
public interface IEmailService
{
    void Send(string to, string message);
}

// No state, safe to create many times
public class EmailService : IEmailService
{
    public void Send(string to, string message)
    {
        // Send email
    }
}
```

---

### 2. Scoped (`AddScoped`)

**Creates ONE instance per HTTP request (or scope).**

```csharp
services.AddScoped<IHotelRepository, HotelRepository>();
services.AddScoped<HotelDbContext>();
```

**Lifetime Diagram:**
```
Request 1:
  Controller → HotelRepository (Instance A)
  Service    → HotelRepository (Instance A)  ← Same instance within request

Request 2:
  Controller → HotelRepository (Instance B)  ← New request, new instance
```

**When to use:**
- Database contexts (DbContext)
- Repositories
- Services that maintain state during a request
- Most application services

**Example:**
```csharp
public class HotelDbContext : DbContext
{
    // Tracks changes during the request
    // Should be scoped to ensure one context per request
}

public class HotelRepository : IHotelRepository
{
    private readonly HotelDbContext _context;

    public HotelRepository(HotelDbContext context)
    {
        _context = context;  // Same context throughout request
    }
}
```

---

### 3. Singleton (`AddSingleton`)

**Creates ONE instance for the ENTIRE application lifetime.**

```csharp
services.AddSingleton<ICacheService, RedisCacheService>();
```

**Lifetime Diagram:**
```
Request 1:
  Controller → CacheService (Instance A)

Request 2:
  Controller → CacheService (Instance A)  ← Same instance

Request 3:
  Controller → CacheService (Instance A)  ← Same instance
```

**When to use:**
- Configuration objects
- Caching services
- Logging services
- Stateless services used frequently
- Thread-safe services

**Example:**
```csharp
public interface ICacheService
{
    void Set(string key, object value);
    object Get(string key);
}

// Thread-safe, maintains cache for entire app
public class RedisCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, object> _cache = new();

    public void Set(string key, object value)
    {
        _cache[key] = value;
    }

    public object Get(string key)
    {
        _cache.TryGetValue(key, out var value);
        return value;
    }
}
```

---

### Lifetime Comparison:

| Lifetime | Instance Creation | Use Case | Example |
|----------|------------------|----------|---------|
| **Transient** | Every time requested | Stateless, lightweight | Email service, validators |
| **Scoped** | Once per request | Database operations | DbContext, repositories |
| **Singleton** | Once per application | Shared state, configuration | Cache, logger, config |

---

### ⚠️ Warning: Captive Dependencies

**DON'T inject scoped services into singletons!**

```csharp
// ❌ BAD: Singleton capturing scoped dependency
services.AddSingleton<CacheService>();
services.AddScoped<HotelDbContext>();

public class CacheService
{
    private readonly HotelDbContext _context;  // ❌ Scoped injected into singleton

    public CacheService(HotelDbContext context)
    {
        _context = context;  // This context will live forever!
    }
}
```

**Problem:**
- Singleton lives for entire app lifetime
- DbContext is designed to be short-lived (per request)
- Leads to memory leaks, stale data, threading issues

**Solution:**
```csharp
// ✅ GOOD: Use IServiceProvider to get scoped services when needed
public class CacheService
{
    private readonly IServiceProvider _serviceProvider;

    public CacheService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void DoWork()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        // Use context
    }
}
```

---

## Registration Methods

### 1. **AddScoped / AddTransient / AddSingleton**

```csharp
// Register interface → implementation
services.AddScoped<IHotelRepository, HotelRepository>();
services.AddScoped<IEmailService, EmailService>();

// Register concrete class
services.AddScoped<HotelDbContext>();
```

---

### 2. **AddDbContext** (Special for EF Core)

```csharp
services.AddDbContext<HotelDbContext>(options =>
    options.UseNpgsql(configuration.GetConnectionString("HotelDatabase"))
);
```

**What this does:**
- Registers `HotelDbContext` as **scoped** (one per request)
- Configures database provider (PostgreSQL)
- Reads connection string from configuration

**Equivalent to:**
```csharp
services.AddScoped<HotelDbContext>(provider =>
{
    var options = new DbContextOptionsBuilder<HotelDbContext>()
        .UseNpgsql(configuration.GetConnectionString("HotelDatabase"))
        .Options;
    return new HotelDbContext(options);
});
```

---

### 3. **Factory Registration**

```csharp
services.AddScoped<IEmailService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var smtpHost = config["Email:SmtpHost"];
    return new EmailService(smtpHost);
});
```

**When to use:**
- Complex object creation
- Need to resolve other services first
- Conditional logic for instantiation

---

### 4. **Multiple Implementations**

```csharp
// Register multiple implementations
services.AddScoped<INotificationService, EmailNotificationService>();
services.AddScoped<INotificationService, SmsNotificationService>();

// Inject all implementations
public class NotificationHandler
{
    private readonly IEnumerable<INotificationService> _services;

    public NotificationHandler(IEnumerable<INotificationService> services)
    {
        _services = services;  // Gets both Email and SMS services
    }

    public void NotifyAll()
    {
        foreach (var service in _services)
        {
            service.Send();
        }
    }
}
```

---

## Extension Methods Pattern

**Extension methods** organize DI registrations by layer.

### Why Use Extension Methods?

**Without Extension Methods (Messy):**
```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

// Everything mixed together
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
builder.Services.AddDbContext<HotelDbContext>(options => options.UseNpgsql("..."));
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
// ... 50 more registrations
```

**Problems:**
- Hard to read
- Hard to maintain
- Mixes concerns
- Violates Clean Architecture (API layer knows about all implementations)

---

**With Extension Methods (Clean):**

```csharp
// In Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();  // ✅ Clean and organized
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
```

**Benefits:**
- ✅ Clean and readable
- ✅ Organized by layer
- ✅ Encapsulates registration logic
- ✅ Follows .NET conventions
- ✅ Easy to test individual layers

---

### Creating Extension Methods:

#### Application Layer (`Application/DependencyInjection.cs`):

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Service.Hotel.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // Register MediatR
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            // Register AutoMapper
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            // Register FluentValidation
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
```

---

#### Persistence Layer (`Persistence/DependencyInjection.cs`):

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BookingSystem.Service.Hotel.Persistence
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<HotelDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("HotelDatabase"),
                    b => b.MigrationsAssembly(typeof(HotelDbContext).Assembly.FullName)
                )
            );

            // Register Repositories
            services.AddScoped<IHotelRepository, HotelRepository>();
            services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();

            return services;
        }
    }
}
```

---

### Extension Method Breakdown:

```csharp
public static IServiceCollection AddPersistence(
    this IServiceCollection services,  // ← "this" makes it an extension method
    IConfiguration configuration)      // ← Parameters the method needs
{
    // Registration logic here

    return services;  // ← Return services for method chaining
}
```

**Key Points:**
1. **`this IServiceCollection services`** - Makes it an extension method
2. **`IConfiguration configuration`** - Pass in dependencies
3. **`return services`** - Allows method chaining

**Method Chaining:**
```csharp
builder.Services
    .AddApplication()
    .AddPersistence(configuration)
    .AddInfrastructure(configuration);
```

---

## Layer-Specific DI Setup

### Application Layer

**What to register:**
- MediatR (CQRS handlers)
- AutoMapper (mappings)
- FluentValidation (validators)

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
```

---

### Persistence Layer

**What to register:**
- DbContext
- Repositories (interface → implementation)

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HotelDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("HotelDatabase")));

        services.AddScoped<IHotelRepository, HotelRepository>();

        return services;
    }
}
```

---

### Infrastructure Layer

**What to register:**
- External services (email, SMS, payment gateways)
- Caching services
- File storage services

```csharp
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IEmailService, SendGridEmailService>();
        services.AddSingleton<ICacheService, RedisCacheService>();
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        return services;
    }
}
```

---

### API Layer (Program.cs)

**What to register:**
- Controllers
- Swagger
- CORS
- Authentication/Authorization
- Layer registrations

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add layer services
builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

// Add API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
```

---

## Constructor Injection

**Constructor Injection** is the primary way to receive dependencies.

### Basic Constructor Injection:

```csharp
public class CreateHotelCommandHandler : IRequestHandler<CreateHotelCommand, int>
{
    private readonly IHotelRepository _repository;

    public CreateHotelCommandHandler(IHotelRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> Handle(CreateHotelCommand request, CancellationToken cancellationToken)
    {
        var hotel = new Hotel(...);
        await _repository.AddAsync(hotel);
        return hotel.Id;
    }
}
```

**DI Container automatically:**
1. Sees `IHotelRepository` in constructor
2. Looks up registration: `services.AddScoped<IHotelRepository, HotelRepository>()`
3. Creates `HotelRepository` instance
4. Injects it into `CreateHotelCommandHandler`

---

### Multiple Dependencies:

```csharp
public class HotelService
{
    private readonly IHotelRepository _hotelRepository;
    private readonly IRoomTypeRepository _roomTypeRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<HotelService> _logger;

    public HotelService(
        IHotelRepository hotelRepository,
        IRoomTypeRepository roomTypeRepository,
        IMapper mapper,
        ILogger<HotelService> logger)
    {
        _hotelRepository = hotelRepository;
        _roomTypeRepository = roomTypeRepository;
        _mapper = mapper;
        _logger = logger;
    }
}
```

**DI Container resolves all dependencies automatically.**

---

### Property Injection (Not Recommended):

```csharp
public class HotelService
{
    [Inject]
    public IHotelRepository Repository { get; set; }  // ❌ Avoid this
}
```

**Why avoid?**
- Less explicit
- Can create invalid objects (if property not set)
- Harder to test
- Constructor injection is preferred in .NET

---

## Testing with DI

### Why DI Makes Testing Easy:

**Without DI:**
```csharp
public class HotelService
{
    private HotelRepository _repo = new HotelRepository();  // ❌ Can't replace

    public void UpdateHotel(Hotel hotel)
    {
        _repo.UpdateAsync(hotel);  // Hits real database in tests!
    }
}
```

---

**With DI:**
```csharp
public class HotelService
{
    private readonly IHotelRepository _repo;

    public HotelService(IHotelRepository repo)
    {
        _repo = repo;  // ✅ Can inject mock
    }

    public void UpdateHotel(Hotel hotel)
    {
        _repo.UpdateAsync(hotel);  // Uses mock in tests
    }
}
```

---

### Unit Test Example:

```csharp
[Test]
public async Task CreateHotel_ShouldReturnHotelId()
{
    // Arrange
    var mockRepository = new Mock<IHotelRepository>();
    mockRepository
        .Setup(r => r.AddAsync(It.IsAny<Hotel>()))
        .ReturnsAsync((Hotel h) => { h.Id = 123; return h; });

    var handler = new CreateHotelCommandHandler(mockRepository.Object);
    var command = new CreateHotelCommand { Name = "Test Hotel" };

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    Assert.AreEqual(123, result);
    mockRepository.Verify(r => r.AddAsync(It.IsAny<Hotel>()), Times.Once);
}
```

**Key Points:**
- Mock the interface (`IHotelRepository`)
- Inject mock into handler
- No real database needed
- Fast, isolated tests

---

## Common Patterns

### 1. **Options Pattern**

For configuration objects:

```csharp
// appsettings.json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587
  }
}

// Configuration class
public class EmailSettings
{
    public string SmtpHost { get; set; }
    public int SmtpPort { get; set; }
}

// Register
services.Configure<EmailSettings>(configuration.GetSection("Email"));

// Inject
public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }
}
```

---

### 2. **Factory Pattern**

When you need to create instances dynamically:

```csharp
public interface INotificationFactory
{
    INotificationService Create(NotificationType type);
}

public class NotificationFactory : INotificationFactory
{
    private readonly IServiceProvider _serviceProvider;

    public NotificationFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public INotificationService Create(NotificationType type)
    {
        return type switch
        {
            NotificationType.Email => _serviceProvider.GetRequiredService<IEmailService>(),
            NotificationType.Sms => _serviceProvider.GetRequiredService<ISmsService>(),
            _ => throw new ArgumentException("Invalid notification type")
        };
    }
}
```

---

### 3. **Decorator Pattern**

Add behavior to existing services:

```csharp
// Base service
public class HotelRepository : IHotelRepository
{
    public async Task<Hotel> GetByIdAsync(int id) { ... }
}

// Decorator adds caching
public class CachedHotelRepository : IHotelRepository
{
    private readonly IHotelRepository _inner;
    private readonly ICacheService _cache;

    public CachedHotelRepository(IHotelRepository inner, ICacheService cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<Hotel> GetByIdAsync(int id)
    {
        var cacheKey = $"hotel_{id}";
        var cached = _cache.Get<Hotel>(cacheKey);
        if (cached != null) return cached;

        var hotel = await _inner.GetByIdAsync(id);
        _cache.Set(cacheKey, hotel);
        return hotel;
    }
}

// Register with decorator
services.AddScoped<IHotelRepository, HotelRepository>();
services.Decorate<IHotelRepository, CachedHotelRepository>();
```

---

## Summary

| Concept | Purpose | Key Point |
|---------|---------|-----------|
| **DI** | Inversion of control | Objects receive dependencies |
| **Transient** | New instance each time | Stateless services |
| **Scoped** | One per request | DbContext, repositories |
| **Singleton** | One per application | Cache, logger, config |
| **Extension Methods** | Organize registrations | Clean, layer-based setup |
| **Constructor Injection** | Primary injection method | Explicit dependencies |
| **Interface Segregation** | Program to interfaces | Loose coupling |

---

## Best Practices

1. **Always inject interfaces**, not concrete types
2. **Use scoped lifetime for DbContext** and repositories
3. **Use extension methods** to organize DI by layer
4. **Prefer constructor injection** over property injection
5. **Avoid captive dependencies** (scoped in singleton)
6. **Register in the correct layer** (Application, Persistence, Infrastructure)
7. **Test with mocks** using injected interfaces
8. **Use IOptions<T>** for configuration objects

---

## Further Reading

- [.NET Dependency Injection](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection)
- [Service Lifetimes](https://docs.microsoft.com/en-us/dotnet/core/extensions/dependency-injection#service-lifetimes)
- [Options Pattern](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options)
- [Testing with DI](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)