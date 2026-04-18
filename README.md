# Todone

A full-stack todo app built with .NET 10 and Angular 21. Features per-user task isolation, drag-and-drop between Active/Completed columns, emoji task icons, and dark mode.

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 10.0+ |
| Node.js | 18+ |
| Angular CLI | 21+ |

Install Angular CLI globally if you don't have it:

```bash
npm install -g @angular/cli
```

## Getting started

### 1. Clone the repository

```bash
git clone git@github.com:billp/todone.git
cd todone
```

### 2. Run the API

```bash
cd TodoApi
dotnet run
```

The API starts at `http://localhost:5050`. The SQLite database (`todo.db`) is created and migrated automatically on first run.

### 3. Run the frontend

In a separate terminal:

```bash
cd todo-frontend
npm install
ng serve
```

The app is available at `http://localhost:4200`.

## Running tests

```bash
dotnet test
```

Tests use an in-memory SQLite database — no setup required.

## Configuration

API settings are in `TodoApi/appsettings.json`. For production, override the JWT key via environment variable or user secrets — do not commit a real secret to source control.

```json
{
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters"
  }
}
```

## Project structure

```
TodoApp/
├── TodoApi/          # .NET 10 Web API (JWT auth, SQLite, EF Core)
├── TodoApi.Tests/    # Integration tests (xUnit, WebApplicationFactory)
└── todo-frontend/    # Angular 21 SPA (standalone components, CDK DnD)
```
