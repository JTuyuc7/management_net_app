# Quick Start Guide - Backend Tests

## Quick Commands

### Run All Tests
```bash
dotnet test
```

### Run Tests with Output
```bash
dotnet test --logger "console;verbosity=detailed"
```

### Run Only Unit Tests
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
```

### Run Only Integration Tests
```bash
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

##  What Gets Tested

###  Repository Layer
- CRUD operations (Create, Read, Update, Delete)
- Data persistence and retrieval
- Edge cases (null values, non-existent IDs)
- Ordering and filtering

### Controller Layer
- HTTP status codes (200, 201, 204, 400, 404, 500)
- Request/response handling
- Model validation
- Error handling
- Repository interaction

###  Integration Layer
- Full API endpoints
- Request/response serialization
- End-to-end workflows
- CORS configuration
- Database integration

##  Common Test Scenarios

### Testing Success Cases
```bash
# All GET requests return 200 OK
# POST requests return 201 Created
# PUT/DELETE requests return 204 No Content
```

### Testing Error Cases
```bash
# Invalid data returns 400 Bad Request
# Non-existent resources return 404 Not Found
# Exceptions return 500 Internal Server Error
```

### Testing Workflows
```bash
# Create → Read → Update → Delete
# Multiple operations in sequence
# Data validation and transformation
```
