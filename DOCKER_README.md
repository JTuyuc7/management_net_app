# 🐳 Docker Setup Guide

This guide explains how to run the Task Management Application backend using Docker.

## Prerequisites

- Docker installed on your machine ([Download Docker](https://www.docker.com/products/docker-desktop))
- Docker Compose (included with Docker Desktop)

## Quick Start

### Option 1: Using Docker Compose (Recommended)

```bash
# Build and start the backend
docker-compose up --build

# Or run in detached mode (background)
docker-compose up -d --build

# View logs
docker-compose logs -f backend

# Stop the containers
docker-compose down
```

### Option 2: Using Docker CLI

```bash
# Navigate to the backend directory
cd backend

# Build the Docker image
docker build -t task-management-backend .

# Run the container
docker run -p 5083:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name task-backend \
  task-management-backend

# Stop the container
docker stop task-backend

# Remove the container
docker rm task-backend
```

## Accessing the Application

Once the container is running, you can access:

- **API Base URL**: http://localhost:5083
- **Scalar API Documentation**: http://localhost:5083/scalar/v1
- **OpenAPI Specification**: http://localhost:5083/openapi/v1.json
- **Tasks Endpoint**: http://localhost:5083/api/tasks

## Testing the API

### Using curl

```bash
# Get all tasks
curl http://localhost:5083/api/tasks

# Create a new task
curl -X POST http://localhost:5083/api/tasks \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Test Task",
    "description": "This is a test task",
    "dueDate": "2026-12-31T23:59:59Z",
    "priority": "High"
  }'

# Get a specific task (replace {id} with actual task ID)
curl http://localhost:5083/api/tasks/{id}

# Update a task
curl -X PUT http://localhost:5083/api/tasks/{id} \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Task",
    "description": "Updated description",
    "isCompleted": true,
    "dueDate": "2026-12-31T23:59:59Z",
    "priority": "Medium"
  }'

# Delete a task
curl -X DELETE http://localhost:5083/api/tasks/{id}
```

## Docker Configuration

### Environment Variables

You can customize the application behavior using environment variables:

```yaml
environment:
  - ASPNETCORE_ENVIRONMENT=Development  # or Production
  - ASPNETCORE_URLS=http://+:8080
```

### Port Mapping

- **Host Port**: 5083 (matches your local development setup)
- **Container Port**: 8080 (ASP.NET Core default)

To change the host port, modify the `docker-compose.yml`:

```yaml
ports:
  - "YOUR_PORT:8080"
```

## Docker Image Details

### Multi-Stage Build

The Dockerfile uses a multi-stage build for optimal image size:

1. **Build Stage**: Uses `mcr.microsoft.com/dotnet/sdk:10.0` (~1GB) to compile the application
2. **Runtime Stage**: Uses `mcr.microsoft.com/dotnet/aspnet:10.0` (~200MB) for the final image

### Security Features

- ✅ Runs as non-root user (`appuser`)
- ✅ Minimal runtime image (no SDK tools)
- ✅ Only published artifacts included

## Troubleshooting

### Container won't start

```bash
# Check container logs
docker-compose logs backend

# Or for standalone container
docker logs task-backend
```

### Port already in use

If port 5083 is already in use, either:
1. Stop the process using that port
2. Change the port mapping in `docker-compose.yml`

### Build fails

```bash
# Clean build with no cache
docker-compose build --no-cache

# Or for standalone
docker build --no-cache -t task-management-backend ./backend
```

### Health check failing

The health check pings `/api/tasks`. If it fails:
1. Check if the application started successfully
2. Verify the endpoint is accessible inside the container
3. Adjust the health check timing in `docker-compose.yml`

## Development Workflow

### Rebuilding after code changes

```bash
# Stop, rebuild, and restart
docker-compose down
docker-compose up --build
```

### Viewing real-time logs

```bash
docker-compose logs -f backend
```

### Accessing the container shell

```bash
# For debugging
docker exec -it task-management-backend /bin/bash
```

## Production Deployment

For production deployment:

1. Change `ASPNETCORE_ENVIRONMENT` to `Production`
2. Consider adding a reverse proxy (nginx)
3. Use proper secrets management for sensitive data
4. Add persistent database (PostgreSQL/MySQL) instead of In-Memory
5. Configure proper logging and monitoring

## Next Steps

- [ ] Add Angular frontend container
- [ ] Add database container (PostgreSQL/MySQL)
- [ ] Add nginx reverse proxy
- [ ] Set up CI/CD pipeline
- [ ] Add volume mounts for development

## Notes

- **Database**: Currently using In-Memory database. Data will be lost when container restarts.
- **CORS**: Configured to allow requests from `http://localhost:4200` (Angular default port)
- **Environment**: Set to Development by default in docker-compose.yml

## Useful Commands

```bash
# List running containers
docker ps

# List all containers (including stopped)
docker ps -a

# List images
docker images

# Remove all stopped containers
docker container prune

# Remove unused images
docker image prune

# View container resource usage
docker stats task-management-backend
```
