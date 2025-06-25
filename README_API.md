# Potential Sales API

A comprehensive REST API for managing potential sales prospects, activities, and related data based on the JJs PotentialSales domain model.

## Features

- **Prospects Management**: Full CRUD operations for sales prospects
- **Activities Tracking**: Track all sales activities and interactions
- **Lookup Data Management**: Manage prospect types, sources, and waste products
- **Dashboard Analytics**: Get insights into sales pipeline
- **Swagger Documentation**: Interactive API documentation
- **Entity Framework Integration**: SQLite database with automatic migrations

## Running the Application

### As Web API (Default)
```bash
dotnet run
```
The API will be available at `https://localhost:5001` with Swagger UI at the root.

### As MCP Server
```bash
dotnet run --mcp
```

## API Endpoints

### Prospects
- `GET /api/prospects` - Get all prospects with pagination and filtering
- `GET /api/prospects/{id}` - Get a specific prospect
- `POST /api/prospects` - Create a new prospect
- `PUT /api/prospects/{id}` - Update an existing prospect
- `DELETE /api/prospects/{id}` - Delete a prospect
- `PATCH /api/prospects/{id}/status` - Update prospect status
- `GET /api/prospects/dashboard` - Get dashboard analytics

### Activities
- `GET /api/activities` - Get all activities with pagination and filtering
- `GET /api/activities/{id}` - Get a specific activity
- `POST /api/activities` - Create a new activity
- `PUT /api/activities/{id}` - Update an existing activity
- `DELETE /api/activities/{id}` - Delete an activity
- `GET /api/activities/prospects/{prospectId}` - Get activities for a prospect

### Lookup Data
- `GET /api/prospect-types` - Get all prospect types
- `POST /api/prospect-types` - Create a new prospect type
- `PUT /api/prospect-types/{id}` - Update a prospect type

- `GET /api/sources` - Get all sources
- `POST /api/sources` - Create a new source
- `PUT /api/sources/{id}` - Update a source

- `GET /api/waste-products` - Get all waste products
- `POST /api/waste-products` - Create a new waste product
- `PUT /api/waste-products/{id}` - Update a waste product

- `GET /api/prospects/{prospectId}/waste-products` - Get waste products for a prospect
- `POST /api/prospects/{prospectId}/waste-products` - Add waste product to prospect
- `DELETE /api/prospects/{prospectId}/waste-products/{wasteProductId}` - Remove waste product from prospect

### Health Check
- `GET /health` - Health check endpoint

## Example Usage

### Create a New Prospect
```json
POST /api/prospects
{
  "potentialSaleNumber": "PSL0001234",
  "siteNumber": "SITE001",
  "assignee": "John Smith",
  "prospectTypeId": 1,
  "sourceId": 2,
  "customerStatus": "NewCustomer",
  "tradingName": "ABC Waste Services",
  "interest": "High",
  "addressLine": "123 Business St, Industrial Park",
  "contactFirstName": "Jane",
  "contactLastName": "Doe",
  "contactEmail": "jane.doe@abcwaste.com",
  "contactPhone": "555-0123",
  "description": "Potential client interested in waste management services"
}
```

### Add an Activity to a Prospect
```json
POST /api/activities
{
  "prospectId": 1,
  "type": "PhoneCall",
  "method": "Phone",
  "contactName": "Jane Doe",
  "date": "2025-06-25T10:30:00Z",
  "description": "Initial contact call to discuss waste management needs"
}
```

### Get Prospects with Filtering
```
GET /api/prospects?status=Open&assignee=John&page=1&pageSize=10
```

## Data Models

### Prospect Status Enum
- `Open` (1) - Active prospect
- `Cancelled` (2) - Cancelled prospect
- `Won` (3) - Successfully converted
- `Lost` (4) - Lost opportunity

### Customer Status Enum
- `NewCustomer` (1) - New customer
- `ExistingCustomer` (2) - Existing customer
- `FormerCustomer` (3) - Former customer

### Interest Level Enum
- `Low` (1) - Low interest
- `Medium` (2) - Medium interest
- `High` (3) - High interest
- `VeryHigh` (4) - Very high interest

### Activity Type Enum
- `PhoneCall` (1) - Phone call
- `Email` (2) - Email communication
- `Meeting` (3) - In-person meeting
- `SiteVisit` (4) - Site visit
- `QuotePreparation` (5) - Quote preparation
- `FollowUp` (6) - Follow-up activity

### Activity Method Enum
- `InPerson` (1) - In person
- `Phone` (2) - Phone
- `Email` (3) - Email
- `VideoCall` (4) - Video call
- `Letter` (5) - Letter

## Database

The application uses SQLite with Entity Framework Core. The database is automatically created with seed data when the application starts.

## Configuration

Configuration can be modified in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=potentialsales.db"
  }
}
```

## Development

The API includes:
- Comprehensive data validation
- Entity relationships with navigation properties
- Computed properties for business logic
- Proper HTTP status codes
- Error handling
- CORS support for web clients
- Swagger/OpenAPI documentation

## Model Validation

The underlying domain models are validated using the MCP Model Validator. Run validation using:

```bash
dotnet run --mcp
# Then use MCP tools to validate models
```
