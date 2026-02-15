# Task Management Application

A full-stack task management application built with ASP.NET Core (backend) and Angular (frontend).

##  Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Prerequisites](#prerequisites)
- [Running Locally](#running-locally)
  - [Option 1: Docker Compose (Recommended)](#option-1-docker-compose-recommended)
  - [Option 2: Manual Setup](#option-2-manual-setup)
- [API Documentation](#api-documentation)
- [Testing the API](#testing-the-api)
- [Docker Configuration](#docker-configuration)
- [Troubleshooting](#troubleshooting)
- [Project Structure](#project-structure)

## Features

-  Create, read, update, and delete tasks
-  Mark tasks as complete/incomplete
-  Task details: title, description, creation date
-  Clean, modern UI with Tailwind CSS
-  RESTful API with proper validation
-  In-memory database (Entity Framework Core)
-  Dockerized for easy deployment
-  Health checks for monitoring

## Tech Stack

### Backend
- ASP.NET Core 10.0
- Entity Framework Core (In-Memory)
- C# 12
- Scalar API Documentation

### Frontend
- Angular 19
- TypeScript
- Tailwind CSS
- RxJS
- Standalone Components

### DevOps
- Docker & Docker Compose
- Nginx (for serving Angular in production)

## Prerequisites

### For Docker Compose (Recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) installed

### For Manual Setup
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`

## Running Locally

### Option 1: Docker Compose (Recommended)

This is the easiest way to run both backend and frontend together.

```bash
# Clone the repository
git clone <repository-url>
cd TaskManagementApp

# Build and start both services
docker-compose up --build

# Or run in detached mode (background)
docker-compose up -d --build
```

**Access the application:**
- **Frontend**: http://localhost:4200
- **Backend API**: http://localhost:5083/api
- **API Documentation**: http://localhost:5083/scalar/v1

**Useful Docker Compose commands:**
```bash
# View logs
docker-compose logs -f

# View logs for specific service
docker-compose logs -f frontend
docker-compose logs -f backend

# Stop services
docker-compose down

# Rebuild after code changes
docker-compose down
docker-compose up --build

# Remove volumes and containers
docker-compose down -v
```

### Option 2: Manual Setup

Run the backend and frontend separately on your local machine.

#### Backend Setup

```bash
# Navigate to backend directory
cd backend

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Or run with watch mode (auto-reload on changes)
dotnet watch run
```

The backend will be available at: http://localhost:5083

#### Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install dependencies
npm install

# Start development server
npm start

# Or use Angular CLI directly
ng serve
```

The frontend will be available at: http://localhost:4200

**Note:** Make sure the backend is running before starting the frontend, as the frontend needs to communicate with the API.

## API Documentation

Once the backend is running, you can access:

- **Scalar API Documentation**: http://localhost:5083/scalar/v1
- **OpenAPI Specification**: http://localhost:5083/openapi/v1.json

### API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tasks` | Get all tasks |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}/complete` | Mark task as complete |
| DELETE | `/api/tasks/{id}` | Delete a task |

## Testing the API

### Using curl

```bash
# Get all tasks
curl http://localhost:5083/api/tasks

# Create a new task
curl -X POST http://localhost:5083/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My First Task",
    "description": "This is a test task"
  }'

# Get a specific task (replace 1 with actual task ID)
curl http://localhost:5083/api/tasks/1

# Update a task
curl -X PUT http://localhost:5083/api/tasks/1 \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Task",
    "description": "Updated description",
    "isCompleted": false
  }'

# Mark task as complete
curl -X PUT http://localhost:5083/api/tasks/1/complete

# Delete a task
curl -X DELETE http://localhost:5083/api/tasks/1
```

### Using the Frontend

1. Open http://localhost:4200 in your browser
2. Click "Create New Task" button
3. Fill in the title and description
4. Click "Create Task"
5. Use the checkbox to mark tasks as complete
6. Use the delete (🗑️) button to remove tasks

## 🐳 Docker Configuration

### Docker Compose Services

The `docker-compose.yml` defines two services:

#### Backend Service
- **Container Name**: `task-management-backend`
- **Port**: 5083:8080 (host:container)
- **Health Check**: Pings `/api/tasks` endpoint
- **Network**: `task-management-network`

#### Frontend Service
- **Container Name**: `task-management-frontend`
- **Port**: 4200:8080 (host:container)
- **Health Check**: Pings `/health` endpoint
- **Network**: `task-management-network`
- **Depends On**: backend

### Environment Variables

You can customize the application using environment variables in `docker-compose.yml`:

**Backend:**
```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development  # or Production
  - ASPNETCORE_URLS=http://+:8080
```

**Frontend:**
```yaml
environment:
  - NODE_ENV=production
```

### Port Configuration

To change the ports, modify `docker-compose.yml`:

```yaml
services:
  backend:
    ports:
      - "YOUR_BACKEND_PORT:8080"
  
  frontend:
    ports:
      - "YOUR_FRONTEND_PORT:8080"
```

**Important:** If you change the backend port, also update `frontend/src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:YOUR_BACKEND_PORT/api'
};
```

##  Troubleshooting

### Docker Issues

**Container won't start:**
```bash
# Check logs
docker-compose logs backend
docker-compose logs frontend

# Check container status
docker ps -a
```

**Port already in use:**
```bash
# Find process using the port (macOS/Linux)
lsof -i :4200
lsof -i :5083

# Kill the process
kill -9 <PID>

# Or change the port in docker-compose.yml
```

**Build fails:**
```bash
# Clean build with no cache
docker-compose build --no-cache
docker-compose up
```

**Health check failing:**
```bash
# Check if services are accessible
curl http://localhost:5083/api/tasks
curl http://localhost:4200/health

# Increase timeout in docker-compose.yml
healthcheck:
  start_period: 60s  # Increase from 40s
```

### Manual Setup Issues

**Backend won't start:**
```bash
# Check .NET version
dotnet --version  # Should be 10.0 or higher

# Clean and rebuild
dotnet clean
dotnet build
dotnet run
```

**Frontend won't start:**
```bash
# Check Node version
node --version  # Should be 20 or higher

# Clear cache and reinstall
rm -rf node_modules package-lock.json
npm install
npm start
```

**CORS errors:**
- Make sure the backend is running on port 5083
- Check that `frontend/src/environments/environment.ts` has the correct API URL
- Verify CORS is configured in `backend/Program.cs`

##  Project Structure

```
TaskManagementApp/
├── backend/                    # ASP.NET Core API
│   ├── Controllers/           # API Controllers
│   ├── Models/                # Data models
│   ├── DTOs/                  # Data Transfer Objects
│   ├── Data/                  # DbContext
│   ├── Repositories/          # Data access layer
│   ├── Dockerfile             # Backend Docker configuration
│   └── Program.cs             # Application entry point
│
├── frontend/                   # Angular Application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/   # Angular components
│   │   │   ├── models/       # TypeScript interfaces
│   │   │   ├── services/     # HTTP services
│   │   │   └── app.component.ts
│   │   └── environments/     # Environment configs
│   ├── Dockerfile            # Frontend Docker configuration
│   ├── nginx.conf            # Nginx configuration
│   └── package.json
│
├── docker-compose.yml         # Docker Compose configuration
└── README.md                  # This file
```

## Security Features

-  Non-root user in Docker containers
-  Input validation on API endpoints
-  Security headers (X-Frame-Options, X-Content-Type-Options, X-XSS-Protection)
-  CORS configuration
-  Minimal Docker images (multi-stage builds)

##  Notes

- **Database**: Currently using In-Memory database. Data will be lost when the application restarts.
- **CORS**: Configured to allow requests from `http://localhost:4200`
- **Production**: For production deployment, consider using a persistent database (PostgreSQL, MySQL, etc.)

##  Development Workflow

### Making Changes

**Backend changes:**
```bash
# If using Docker Compose
docker-compose restart backend

# If running manually
# The app will auto-reload with 'dotnet watch run'
```

**Frontend changes:**
```bash
# If using Docker Compose
docker-compose down
docker-compose up --build frontend

# If running manually
# The app will auto-reload with 'ng serve'
```

### Viewing Logs

```bash
# Docker Compose
docker-compose logs -f

# Individual containers
docker logs -f task-management-backend
docker logs -f task-management-frontend
```

## How does it look?

#### Main Page

#### Task Creation Modal

#### Complete Task

_Develop by:_ ***Jaime Tuyuc***