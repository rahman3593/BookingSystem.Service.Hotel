# ASP.NET Core Controllers & Action Results

This document explains ASP.NET Core Controllers, ActionResult, and API design concepts used in our Hotel Booking System.

---

## Table of Contents

1. [What is a Controller?](#what-is-a-controller)
2. [Controller Attributes](#controller-attributes)
3. [ActionResult Explained](#actionresult-explained)
4. [HTTP Action Methods](#http-action-methods)
5. [Response Types](#response-types)
6. [Model Binding](#model-binding)
7. [Content Negotiation](#content-negotiation)
8. [Best Practices](#best-practices)

---

## What is a Controller?

A **Controller** is a class that handles HTTP requests and returns HTTP responses.

### Purpose:
- Receives HTTP requests from clients (browsers, mobile apps, etc.)
- Routes requests to appropriate handlers (via MediatR in our case)
- Returns HTTP responses with appropriate status codes and data

### Controller Flow:

```
Client Request
     ↓
[Route: POST /api/hotels]
     ↓
HotelsController.CreateHotel()
     ↓
MediatR.Send(CreateHotelCommand)
     ↓
CreateHotelCommandHandler
     ↓
HotelRepository.AddAsync()
     ↓
Database
     ↓
Return Hotel ID
     ↓
Controller returns 201 Created
     ↓
Client receives response
```

---

## Controller Attributes

### 1. **[ApiController]**

```csharp
[ApiController]
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    // ...
}
```

**What it does:**
- Enables automatic model validation
- Binds complex types from request body automatically
- Returns 400 Bad Request for validation errors
- Provides better error responses

**Without [ApiController]:**
```csharp
public class HotelsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
    {
        // ❌ You have to manually check ModelState
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Process command...
    }
}
```

**With [ApiController]:**
```csharp
[ApiController]
public class HotelsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
    {
        // ✅ Automatic validation - returns 400 if invalid
        // No need to check ModelState manually

        var hotelId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, hotelId);
    }
}
```

---

### 2. **[Route]**

Defines the URL pattern for the controller or action.

#### Controller-level routing:
```csharp
[Route("api/[controller]")]
public class HotelsController : ControllerBase
{
    // Base route: /api/hotels
}
```

**[controller]** is replaced with controller name minus "Controller":
- `HotelsController` → `/api/hotels`
- `UsersController` → `/api/users`

#### Action-level routing:
```csharp
[HttpGet("{id}")]  // /api/hotels/{id}
public async Task<ActionResult<HotelDto>> GetHotelById(int id)

[HttpGet("search")]  // /api/hotels/search
public async Task<ActionResult<List<HotelDto>>> SearchHotels(string city)
```

#### Route parameters:
```csharp
[HttpGet("{id}")]           // /api/hotels/5
[HttpGet("{id}/rooms")]     // /api/hotels/5/rooms
[HttpGet("{hotelId}/rooms/{roomId}")]  // /api/hotels/5/rooms/10
```

---

### 3. **HTTP Verb Attributes**

Specifies which HTTP method the action responds to.

```csharp
[HttpGet]     // GET request - Retrieve data
[HttpPost]    // POST request - Create new resource
[HttpPut]     // PUT request - Update entire resource
[HttpPatch]   // PATCH request - Partial update
[HttpDelete]  // DELETE request - Delete resource
```

**Example:**
```csharp
[HttpGet]                    // GET /api/hotels
public async Task<ActionResult<List<HotelDto>>> GetAllHotels()

[HttpGet("{id}")]            // GET /api/hotels/5
public async Task<ActionResult<HotelDto>> GetHotelById(int id)

[HttpPost]                   // POST /api/hotels
public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)

[HttpPut("{id}")]            // PUT /api/hotels/5
public async Task<ActionResult> UpdateHotel(int id, [FromBody] UpdateHotelCommand command)

[HttpDelete("{id}")]         // DELETE /api/hotels/5
public async Task<ActionResult> DeleteHotel(int id)
```

---

### 4. **[ProducesResponseType]**

Documents possible HTTP responses for Swagger/OpenAPI.

```csharp
[HttpGet("{id}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    // Can return 200 OK or 404 Not Found
}
```

**Benefits:**
- Auto-generates API documentation
- Swagger UI shows possible responses
- Helps API consumers understand the API

**Swagger documentation generated:**
```
GET /api/hotels/{id}

Responses:
  200 - Success
    Content-Type: application/json
    Schema: HotelDto

  404 - Not Found
    Content-Type: application/json
    Schema: string
```

---

## ActionResult Explained

**ActionResult** represents the result of an action method.

### Why Use ActionResult?

#### Without ActionResult (Limited):
```csharp
[HttpGet("{id}")]
public async Task<HotelDto> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });

    // ❌ Problem: What if hotel is null?
    // Can only return HotelDto, not different status codes
    return hotel;  // Returns null as 200 OK - confusing!
}
```

**Problems:**
- Always returns 200 OK
- Can't return 404 Not Found
- Can't return error messages
- Poor API design

---

#### With ActionResult (Flexible):
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });

    if (hotel == null)
    {
        return NotFound($"Hotel with ID {id} not found.");  // ✅ 404
    }

    return Ok(hotel);  // ✅ 200 OK with data
}
```

**Benefits:**
- ✅ Can return different HTTP status codes
- ✅ Can return error messages
- ✅ Better API design
- ✅ More control over responses

---

### ActionResult vs ActionResult<T>

#### **ActionResult** (No data returned):
```csharp
[HttpDelete("{id}")]
public async Task<ActionResult> DeleteHotel(int id)
{
    await _mediator.Send(new DeleteHotelCommand { Id = id });
    return NoContent();  // 204 No Content - just status, no data
}
```

#### **ActionResult<T>** (Returns status + data):
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });
    return Ok(hotel);  // 200 OK with HotelDto
}
```

---

### Common ActionResult Methods

#### 1. **Ok()** - 200 OK

```csharp
return Ok(hotel);

// Response:
// HTTP/1.1 200 OK
// Content-Type: application/json
//
// {
//   "id": 1,
//   "name": "Hilton Paris",
//   ...
// }
```

**When to use:** Request succeeded, returning data.

---

#### 2. **Created() / CreatedAtAction()** - 201 Created

```csharp
var hotelId = await _mediator.Send(command);
return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, hotelId);

// Response:
// HTTP/1.1 201 Created
// Location: /api/hotels/1
// Content-Type: application/json
//
// 1
```

**When to use:** New resource created (POST requests).

**Parameters:**
- `nameof(GetHotelById)` - Action name to generate Location URL
- `new { id = hotelId }` - Route values for the URL
- `hotelId` - Response body

---

#### 3. **NoContent()** - 204 No Content

```csharp
return NoContent();

// Response:
// HTTP/1.1 204 No Content
// (no body)
```

**When to use:** Operation succeeded but no data to return (UPDATE, DELETE).

---

#### 4. **BadRequest()** - 400 Bad Request

```csharp
return BadRequest("Hotel name is required");

// Response:
// HTTP/1.1 400 Bad Request
// Content-Type: application/json
//
// "Hotel name is required"
```

**When to use:** Client sent invalid data.

---

#### 5. **NotFound()** - 404 Not Found

```csharp
return NotFound($"Hotel with ID {id} not found");

// Response:
// HTTP/1.1 404 Not Found
// Content-Type: application/json
//
// "Hotel with ID 999 not found"
```

**When to use:** Requested resource doesn't exist.

---

#### 6. **Unauthorized()** - 401 Unauthorized

```csharp
return Unauthorized();

// Response:
// HTTP/1.1 401 Unauthorized
```

**When to use:** User not authenticated.

---

#### 7. **Forbid()** - 403 Forbidden

```csharp
return Forbid();

// Response:
// HTTP/1.1 403 Forbidden
```

**When to use:** User authenticated but lacks permission.

---

#### 8. **StatusCode()** - Custom Status Code

```csharp
return StatusCode(503, "Service temporarily unavailable");

// Response:
// HTTP/1.1 503 Service Unavailable
// Content-Type: application/json
//
// "Service temporarily unavailable"
```

**When to use:** Need a specific status code not covered by other methods.

---

## HTTP Action Methods

### RESTful API Design

REST (Representational State Transfer) uses HTTP methods to perform CRUD operations:

| HTTP Method | CRUD Operation | Idempotent? | Safe? |
|-------------|---------------|-------------|-------|
| **GET** | Read | Yes | Yes |
| **POST** | Create | No | No |
| **PUT** | Update (full) | Yes | No |
| **PATCH** | Update (partial) | No | No |
| **DELETE** | Delete | Yes | No |

**Idempotent:** Multiple identical requests have the same effect as a single request
**Safe:** Doesn't modify data

---

### GET - Retrieve Data

```csharp
// Get all hotels
[HttpGet]
public async Task<ActionResult<List<HotelDto>>> GetAllHotels()
{
    var hotels = await _mediator.Send(new GetHotelsListQuery());
    return Ok(hotels);
}

// Get specific hotel
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });

    if (hotel == null)
        return NotFound();

    return Ok(hotel);
}

// Get with query parameters
[HttpGet("search")]
public async Task<ActionResult<List<HotelDto>>> SearchHotels(
    [FromQuery] string city,
    [FromQuery] int? starRating)
{
    var query = new SearchHotelsQuery { City = city, StarRating = starRating };
    var hotels = await _mediator.Send(query);
    return Ok(hotels);
}
```

**Request examples:**
```
GET /api/hotels
GET /api/hotels/5
GET /api/hotels/search?city=Paris&starRating=5
```

---

### POST - Create Resource

```csharp
[HttpPost]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
{
    var hotelId = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, hotelId);
}
```

**Request:**
```
POST /api/hotels
Content-Type: application/json

{
  "name": "Hilton Paris",
  "city": "Paris",
  "country": "France",
  ...
}
```

**Response:**
```
HTTP/1.1 201 Created
Location: /api/hotels/1
Content-Type: application/json

1
```

---

### PUT - Update Entire Resource

```csharp
[HttpPut("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult> UpdateHotel(int id, [FromBody] UpdateHotelCommand command)
{
    if (id != command.Id)
        return BadRequest("ID mismatch");

    await _mediator.Send(command);
    return NoContent();
}
```

**Request:**
```
PUT /api/hotels/5
Content-Type: application/json

{
  "id": 5,
  "name": "Hilton Paris Updated",
  "city": "Paris",
  "country": "France",
  ...  (all fields required)
}
```

**Response:**
```
HTTP/1.1 204 No Content
```

---

### PATCH - Partial Update

```csharp
[HttpPatch("{id}")]
public async Task<ActionResult> PartialUpdateHotel(int id, [FromBody] JsonPatchDocument<UpdateHotelCommand> patchDoc)
{
    // Apply only specific fields
    // More complex - typically used with JsonPatch
}
```

**Request:**
```
PATCH /api/hotels/5
Content-Type: application/json

{
  "name": "Hilton Paris - New Name"
}
```

---

### DELETE - Remove Resource

```csharp
[HttpDelete("{id}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult> DeleteHotel(int id)
{
    await _mediator.Send(new DeleteHotelCommand { Id = id });
    return NoContent();
}
```

**Request:**
```
DELETE /api/hotels/5
```

**Response:**
```
HTTP/1.1 204 No Content
```

---

## Response Types

### Status Code Categories

| Range | Category | Meaning |
|-------|----------|---------|
| 1xx | Informational | Request received, continuing |
| 2xx | Success | Request successfully received and accepted |
| 3xx | Redirection | Further action needed |
| 4xx | Client Error | Request contains bad syntax or can't be fulfilled |
| 5xx | Server Error | Server failed to fulfill valid request |

---

### Common Status Codes

#### Success (2xx):
- **200 OK** - Request succeeded, returning data
- **201 Created** - Resource created successfully
- **204 No Content** - Request succeeded, no data returned

#### Client Errors (4xx):
- **400 Bad Request** - Invalid request data
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Authenticated but no permission
- **404 Not Found** - Resource doesn't exist
- **409 Conflict** - Request conflicts with current state (e.g., duplicate)
- **422 Unprocessable Entity** - Validation errors

#### Server Errors (5xx):
- **500 Internal Server Error** - Unexpected server error
- **503 Service Unavailable** - Server temporarily unavailable

---

## Model Binding

**Model Binding** maps HTTP request data to action method parameters.

### Binding Sources

#### 1. **[FromBody]** - From request body (JSON/XML)

```csharp
[HttpPost]
public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
{
    // Binds JSON body to CreateHotelCommand object
}
```

**Request:**
```
POST /api/hotels
Content-Type: application/json

{
  "name": "Hilton Paris",
  "city": "Paris"
}
```

---

#### 2. **[FromRoute]** - From URL route

```csharp
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById([FromRoute] int id)
{
    // Binds {id} from URL to int id parameter
}
```

**Request:**
```
GET /api/hotels/5
```

**Note:** `[FromRoute]` is implicit for route parameters, so it's usually omitted.

---

#### 3. **[FromQuery]** - From query string

```csharp
[HttpGet("search")]
public async Task<ActionResult<List<HotelDto>>> SearchHotels(
    [FromQuery] string city,
    [FromQuery] int? starRating)
{
    // Binds query parameters to method parameters
}
```

**Request:**
```
GET /api/hotels/search?city=Paris&starRating=5
```

---

#### 4. **[FromHeader]** - From HTTP header

```csharp
[HttpGet]
public async Task<ActionResult<List<HotelDto>>> GetHotels(
    [FromHeader(Name = "X-API-Version")] string apiVersion)
{
    // Binds X-API-Version header to apiVersion parameter
}
```

**Request:**
```
GET /api/hotels
X-API-Version: 2.0
```

---

#### 5. **[FromForm]** - From form data

```csharp
[HttpPost("upload")]
public async Task<ActionResult> UploadPhoto(
    [FromForm] IFormFile photo,
    [FromForm] int hotelId)
{
    // Binds form data (file uploads)
}
```

**Request:**
```
POST /api/hotels/upload
Content-Type: multipart/form-data

photo: [file]
hotelId: 5
```

---

### Default Binding Behavior

Without explicit attributes, ASP.NET Core uses these defaults:

| Parameter Type | Default Binding Source |
|---------------|------------------------|
| Simple types (int, string, etc.) | Route → Query → Header |
| Complex types (objects) | Body ([FromBody]) |
| IFormFile | Form ([FromForm]) |

**Example:**
```csharp
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    // id binds from route automatically (no [FromRoute] needed)
}

[HttpPost]
public async Task<ActionResult<int>> CreateHotel(CreateHotelCommand command)
{
    // command binds from body automatically (no [FromBody] needed with [ApiController])
}
```

---

## Content Negotiation

**Content Negotiation** allows clients to request different response formats.

### How it works:

Client sends `Accept` header:
```
GET /api/hotels/1
Accept: application/json
```

Server responds with matching format:
```
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 1,
  "name": "Hilton Paris"
}
```

### Supported Formats:

By default, ASP.NET Core supports:
- **application/json** (JSON)
- **application/xml** (XML)
- **text/plain** (Plain text)

---

## Best Practices

### 1. Use Appropriate HTTP Methods

```csharp
// ✅ Good - RESTful
[HttpGet]     // Retrieve data
[HttpPost]    // Create new resource
[HttpPut]     // Update entire resource
[HttpDelete]  // Delete resource

// ❌ Bad - Using POST for everything
[HttpPost("get-hotel")]     // Should be GET
[HttpPost("delete-hotel")]  // Should be DELETE
```

---

### 2. Return Appropriate Status Codes

```csharp
// ✅ Good
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });

    if (hotel == null)
        return NotFound();  // 404

    return Ok(hotel);  // 200
}

// ❌ Bad - Always returns 200 OK
[HttpGet("{id}")]
public async Task<HotelDto> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });
    return hotel;  // Returns null as 200 OK
}
```

---

### 3. Use CreatedAtAction for POST

```csharp
// ✅ Good - Returns location of created resource
[HttpPost]
public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
{
    var hotelId = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, hotelId);
}

// ❌ Bad - Just returns 200 OK
[HttpPost]
public async Task<int> CreateHotel([FromBody] CreateHotelCommand command)
{
    var hotelId = await _mediator.Send(command);
    return hotelId;
}
```

---

### 4. Document with ProducesResponseType

```csharp
// ✅ Good - Documented for Swagger
[HttpGet("{id}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)

// ❌ Bad - No documentation
[HttpGet("{id}")]
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
```

---

### 5. Use Async Methods

```csharp
// ✅ Good - Non-blocking
public async Task<ActionResult<HotelDto>> GetHotelById(int id)
{
    var hotel = await _mediator.Send(new GetHotelByIdQuery { Id = id });
    return Ok(hotel);
}

// ❌ Bad - Blocking
public ActionResult<HotelDto> GetHotelById(int id)
{
    var hotel = _mediator.Send(new GetHotelByIdQuery { Id = id }).Result;  // Blocks thread!
    return Ok(hotel);
}
```

---

### 6. Keep Controllers Thin

```csharp
// ✅ Good - Thin controller, logic in handlers
public class HotelsController : ControllerBase
{
    private readonly IMediator _mediator;

    [HttpPost]
    public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
    {
        var hotelId = await _mediator.Send(command);  // Logic in handler
        return CreatedAtAction(nameof(GetHotelById), new { id = hotelId }, hotelId);
    }
}

// ❌ Bad - Fat controller with business logic
public class HotelsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<int>> CreateHotel([FromBody] CreateHotelCommand command)
    {
        // ❌ Validation logic in controller
        if (string.IsNullOrEmpty(command.Name))
            return BadRequest("Name is required");

        // ❌ Business logic in controller
        var hotel = new Hotel(command.Name, command.City, ...);
        hotel.UpdateAddress(...);
        hotel.UpdateContactInfo(...);

        // ❌ Data access in controller
        await _context.Hotels.AddAsync(hotel);
        await _context.SaveChangesAsync();

        return CreatedAtAction(...);
    }
}
```

---

### 7. Use Meaningful Route Names

```csharp
// ✅ Good - Clear and RESTful
[Route("api/hotels")]
[HttpGet]                         // GET /api/hotels
[HttpGet("{id}")]                 // GET /api/hotels/5
[HttpGet("{id}/rooms")]           // GET /api/hotels/5/rooms
[HttpPost]                        // POST /api/hotels

// ❌ Bad - Unclear routes
[Route("api/hotel-list")]
[Route("api/get-hotel-by-id")]
[Route("api/create-new-hotel")]
```

---

## Summary

| Concept | Purpose | Key Point |
|---------|---------|-----------|
| **Controller** | Handles HTTP requests | Thin, delegates to handlers |
| **[ApiController]** | Enables API features | Automatic validation, binding |
| **ActionResult** | Represents response | Different status codes |
| **ActionResult<T>** | Response with data | Status + data |
| **HTTP Methods** | RESTful operations | GET, POST, PUT, DELETE |
| **Model Binding** | Maps request to parameters | FromBody, FromRoute, FromQuery |
| **Status Codes** | HTTP response status | 200, 201, 400, 404, etc. |
| **ProducesResponseType** | Documents responses | Swagger/OpenAPI |

---

## Further Reading

- [ASP.NET Core Controllers](https://docs.microsoft.com/en-us/aspnet/core/web-api/)
- [Action Return Types](https://docs.microsoft.com/en-us/aspnet/core/web-api/action-return-types)
- [Routing](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing)
- [Model Binding](https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)
- [HTTP Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Status)