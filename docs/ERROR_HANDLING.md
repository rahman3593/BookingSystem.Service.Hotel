# Error Handling in ASP.NET Core

This document explains error handling mechanisms in ASP.NET Core and how they work in our Hotel Booking System.

---

## Table of Contents

1. [Request Processing Pipeline](#request-processing-pipeline)
2. [Types of Error Handling](#types-of-error-handling)
3. [JSON Deserialization Errors](#json-deserialization-errors)
4. [Validation Errors](#validation-errors)
5. [Domain Exceptions](#domain-exceptions)
6. [Exception Filter vs Middleware](#exception-filter-vs-middleware)
7. [Implementation in Our Project](#implementation-in-our-project)

---

## Request Processing Pipeline

Understanding where errors occur in the ASP.NET Core pipeline is crucial for proper error handling:

```
Incoming HTTP Request
        │
        ▼
 ┌───────────────────────────┐
 │ Global Middleware         │   ← Catches exceptions thrown anywhere in pipeline
 └───────────────────────────┘
        │
        ▼
 ┌───────────────────────────┐
 │ Routing Middleware        │   ← Matches URL to controller/action
 └───────────────────────────┘
        │
        ▼
 ┌───────────────────────────┐
 │ MVC Middleware            │
 │   ┌─────────────────────┐ │
 │   │ Model Binding       │ │
 │   │   ┌───────────────┐ │ │
 │   │   │ Input Formatter│ │ │ ← JSON deserialization happens here
 │   │   └───────────────┘ │ │   (errors converted into ModelState, not exceptions)
 │   └─────────────────────┘ │
 │                           │
 │   ┌─────────────────────┐ │
 │   │ Model Validation    │ │ ← DataAnnotations / FluentValidation
 │   └─────────────────────┘ │
 │                           │
 │   ┌─────────────────────┐ │
 │   │ Exception Filters   │ │ ← Catch exceptions during controller execution
 │   └─────────────────────┘ │
 │                           │
 │   ┌─────────────────────┐ │
 │   │ Controller Action   │ │ ← Your controller method executes here
 │   └─────────────────────┘ │
 └───────────────────────────┘
        │
        ▼
   Response returned
```

---

## Types of Error Handling

### 1. Model State Errors (Automatic)

**When:** JSON deserialization fails or model validation fails
**Status Code:** 400 Bad Request
**Handled By:** ASP.NET Core automatically with `[ApiController]` attribute

**Example:**
```json
// Request
{
  "status": "InvalidStatus"
}

// Response
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "$.status": [
      "The JSON value could not be converted to BookingSystem.Service.Hotel.Domain.Enums.HotelStatus."
    ]
  }
}
```

---

### 2. FluentValidation Errors

**When:** Business rule validation fails
**Status Code:** 400 Bad Request
**Handled By:** FluentValidation automatically with `[ApiController]`

**Example:**
```json
// Request
{
  "name": "",
  "city": "Paris",
  "country": "France"
}

// Response
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": [
      "Hotel name is required"
    ]
  }
}
```

---

### 3. Domain Exceptions

**When:** Business rules violated in domain layer
**Status Code:** 400 Bad Request (or custom)
**Handled By:** Exception Filter

**Example:**
```csharp
// In domain layer
throw new HotelNotFoundException(id);

// Response
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Not Found",
  "status": 404,
  "detail": "Hotel with ID 999 not found"
}
```

---

### 4. Unhandled Exceptions

**When:** Unexpected errors (null reference, database connection, etc.)
**Status Code:** 500 Internal Server Error
**Handled By:** Global exception handler or middleware

---

## JSON Deserialization Errors

### The Problem

When JSON deserialization fails, it happens **during model binding**, which converts errors to **ModelState errors**, not exceptions.

```
JSON → Input Formatter → Model Binding → ModelState
```

**This means:**
- ❌ Middleware cannot catch these (they're not exceptions)
- ❌ Exception filters cannot catch these (they're not exceptions)
- ✅ `[ApiController]` automatically returns 400 Bad Request

---

### Example: Invalid Enum Value

**Request:**
```json
{
  "status": "InvalidStatus"
}
```

**What happens:**

1. **JSON deserializer tries to parse** `"InvalidStatus"` → `HotelStatus` enum
2. **Parsing fails** (not a valid enum value)
3. **Error added to ModelState** (not thrown as exception)
4. **`[ApiController]` checks ModelState** and returns 400

**Response:**
```json
{
  "errors": {
    "$.status": [
      "The JSON value could not be converted to HotelStatus"
    ]
  }
}
```

---

### Why Custom Error Handling for JSON Errors is Hard

The deserialization error is **buried in ModelState** by the time it reaches your code. To customize it, you need to:

**Option 1: Configure InvalidModelStateResponseFactory**

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    e => e.Key,
                    e => e.Value?.Errors.Select(x => x.ErrorMessage).ToArray()
                );

            var response = new
            {
                title = "Invalid request data",
                status = 400,
                errors = errors
            };

            return new BadRequestObjectResult(response);
        };
    });
```

**Option 2: Exception Filter (if exceptions are thrown)**

Some JSON errors DO throw exceptions (like malformed JSON). These can be caught:

```csharp
public class JsonExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is JsonException jsonException)
        {
            // Handle JSON exception
            context.Result = new BadRequestObjectResult(new
            {
                title = "Invalid JSON",
                detail = jsonException.Message
            });
            context.ExceptionHandled = true;
        }
    }
}
```

---

## Validation Errors

### FluentValidation Integration

FluentValidation automatically integrates with `[ApiController]` to return validation errors:

```csharp
public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Hotel name is required");

        RuleFor(x => x.StarRating)
            .IsInEnum()
            .WithMessage("Star rating must be between 1 and 5");
    }
}
```

**When validation fails:**

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Hotel name is required"],
    "StarRating": ["Star rating must be between 1 and 5"]
  }
}
```

---

## Domain Exceptions

### Custom Domain Exceptions

Domain exceptions represent business rule violations:

```csharp
public class HotelNotFoundException : DomainException
{
    public HotelNotFoundException(int hotelId)
        : base($"Hotel with ID {hotelId} was not found")
    {
    }
}
```

### Handling Domain Exceptions

Use an exception filter to convert domain exceptions to proper HTTP responses:

```csharp
public class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is HotelNotFoundException notFoundEx)
        {
            context.Result = new NotFoundObjectResult(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                title = "Not Found",
                status = 404,
                detail = notFoundEx.Message
            });
            context.ExceptionHandled = true;
        }
        else if (context.Exception is DomainException domainEx)
        {
            context.Result = new BadRequestObjectResult(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "Business Rule Violation",
                status = 400,
                detail = domainEx.Message
            });
            context.ExceptionHandled = true;
        }
    }
}
```

---

## Exception Filter vs Middleware

### When to Use Exception Filter ✅

**Exception filters** catch exceptions that occur **during controller execution**:

```
✅ Controller action throws exception
✅ Handler throws exception
✅ Repository throws exception
❌ Model binding errors (converted to ModelState)
❌ Exceptions in other middleware
```

**Example:**
```csharp
public class MyExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // Catches exceptions from controller and below
    }
}

// Register
services.AddControllers(options =>
{
    options.Filters.Add<MyExceptionFilter>();
});
```

---

### When to Use Middleware ✅

**Middleware** catches exceptions that occur **anywhere in the pipeline**:

```
✅ Exceptions in any middleware
✅ Exceptions in controllers
✅ Exceptions in filters
✅ Unhandled exceptions
❌ Model binding errors (not exceptions)
```

**Example:**
```csharp
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Catches all exceptions
            await HandleExceptionAsync(context, ex);
        }
    }
}

// Register
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

---

### Comparison Table

| Feature | Exception Filter | Middleware |
|---------|-----------------|------------|
| **Scope** | Controller actions only | Entire pipeline |
| **Registration** | In AddControllers | In app pipeline |
| **Access to MVC context** | ✅ Yes (ExceptionContext) | ❌ No |
| **Catches routing errors** | ❌ No | ✅ Yes |
| **Catches model binding errors** | ❌ No (ModelState) | ❌ No (ModelState) |
| **Catches controller exceptions** | ✅ Yes | ✅ Yes |
| **Order of execution** | After model binding | Before routing |
| **Best for** | Business exceptions | Global error handling |

---

## Implementation in Our Project

### Current Setup

#### 1. Automatic Model Validation

```csharp
[ApiController]  // ← Enables automatic model state validation
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    // Model binding errors automatically return 400
}
```

---

#### 2. FluentValidation

```csharp
// In validator
public class CreateHotelCommandValidator : AbstractValidator<CreateHotelCommand>
{
    public CreateHotelCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.StarRating).IsInEnum();
    }
}

// Registered in Application layer
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

---

#### 3. Domain Exceptions (Recommended to Add)

**Create:** `src/BookingSystem.Service.Hotel.API/Filters/DomainExceptionFilter.cs`

```csharp
using BookingSystem.Service.Hotel.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookingSystem.Service.Hotel.Api.Filters
{
    public class DomainExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is HotelNotFoundException notFoundEx)
            {
                context.Result = new NotFoundObjectResult(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    title = "Resource Not Found",
                    status = 404,
                    detail = notFoundEx.Message
                });
                context.ExceptionHandled = true;
            }
            else if (context.Exception is DomainException domainEx)
            {
                context.Result = new BadRequestObjectResult(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Business Rule Violation",
                    status = 400,
                    detail = domainEx.Message
                });
                context.ExceptionHandled = true;
            }
        }
    }
}
```

**Register in Program.cs:**

```csharp
builder.Services.AddControllers(options =>
{
    options.Filters.Add<DomainExceptionFilter>();
});
```

---

## Error Response Format

All errors should follow RFC 7807 (Problem Details) format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "Additional details about the error",
  "errors": {
    "FieldName": [
      "Error message 1",
      "Error message 2"
    ]
  },
  "traceId": "00-trace-id-here"
}
```

### HTTP Status Codes

| Status Code | Meaning | Use Case |
|-------------|---------|----------|
| **400 Bad Request** | Invalid request | Validation errors, malformed JSON |
| **401 Unauthorized** | Not authenticated | Missing or invalid auth token |
| **403 Forbidden** | Not authorized | User lacks permission |
| **404 Not Found** | Resource not found | Hotel doesn't exist |
| **409 Conflict** | Resource conflict | Duplicate hotel name |
| **422 Unprocessable Entity** | Semantic errors | Business rule violation |
| **500 Internal Server Error** | Unexpected error | Database down, null reference |

---

## Best Practices

### ✅ DO:

1. **Use `[ApiController]` attribute** - Automatic model validation
2. **Use FluentValidation** - Input validation
3. **Throw domain exceptions** - Business rule violations
4. **Use exception filters** - Convert exceptions to HTTP responses
5. **Follow RFC 7807** - Consistent error format
6. **Return appropriate status codes** - 400, 404, 500, etc.
7. **Provide helpful error messages** - Tell user what went wrong

### ❌ DON'T:

1. **Don't expose stack traces** - Security risk in production
2. **Don't return 200 OK with error** - Use proper status codes
3. **Don't catch and swallow exceptions** - Let filters handle them
4. **Don't put business logic in controllers** - Keep controllers thin
5. **Don't use exceptions for control flow** - Use return values
6. **Don't log sensitive data** - PII, passwords, tokens

---

## Testing Error Handling

### Test Invalid Enum

```bash
# Invalid status
curl -X PUT http://localhost:5000/api/hotels/1 \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "name": "Test",
    "status": "InvalidStatus",
    "starRating": 5,
    "city": "Paris",
    "country": "France"
  }'

# Expected: 400 Bad Request
```

### Test Validation Error

```bash
# Missing required field
curl -X POST http://localhost:5000/api/hotels \
  -H "Content-Type: application/json" \
  -d '{
    "name": "",
    "city": "Paris",
    "country": "France"
  }'

# Expected: 400 Bad Request with validation errors
```

### Test Not Found

```bash
# Non-existent hotel
curl -X GET http://localhost:5000/api/hotels/99999

# Expected: 404 Not Found
```

---

## Summary

| Error Type | Status Code | Handled By | Example |
|------------|-------------|------------|---------|
| **JSON deserialization** | 400 | `[ApiController]` | Invalid enum value |
| **FluentValidation** | 400 | `[ApiController]` | Required field missing |
| **Domain exception** | 400/404 | Exception Filter | Hotel not found |
| **Unhandled exception** | 500 | Middleware | Database connection failed |

---

## Key Takeaways

1. **Model binding errors are not exceptions** - They're ModelState errors
2. **`[ApiController]` handles most validation** - Automatically
3. **Use exception filters for domain exceptions** - Clean HTTP responses
4. **Use middleware for global error handling** - Catch unexpected errors
5. **Follow RFC 7807** - Consistent error format
6. **Return appropriate status codes** - Tell clients what went wrong

---

## Further Reading

- [ASP.NET Core Error Handling](https://docs.microsoft.com/en-us/aspnet/core/web-api/handle-errors)
- [Exception Filters](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters)
- [Model Validation](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation)
- [RFC 7807 Problem Details](https://tools.ietf.org/html/rfc7807)