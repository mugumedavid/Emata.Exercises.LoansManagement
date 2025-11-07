# Emata.Exercises.LoansManagement

> ⚠️ **CAUTION - FICTITIOUS PROJECT** ⚠️
>
> This is a **fictional project** created exclusively for **interview purposes** and technical assessments.
> It does **NOT** represent any real-world application, actual business requirements, or production systems.
> The scenarios, business logic, and requirements presented here are purely hypothetical and should not be
> considered as reflecting real banking, financial services, or loan management practices.

## Project Overview

A loan management system built with .NET 9, showcasing modular monolith architecture, clean code principles, and modern API development practices.

## Project Structure

```text
Emata.Exercises.LoansManagement/
├── src/
│   ├── Emata.Exercise.LoansManagement.API/           # Main API application
│   │   ├── Program.cs                                # Application entry point
│   │   └── appsettings.json                          # Configuration settings
│   │
│   ├── Emata.Exercise.LoansManagement.Contracts/     # Shared contracts and DTOs
│   │
│   └── modules/                                      # Business domain modules
│       ├── Emata.Exercise.LoansManagement.Borrowers/ # Borrowers module
│       │   ├── Domain/                               # Domain entities
│       │   ├── Infrastructure/Data/                  # DbContext, migrations
│       │   ├── Presentation/                         # API endpoints
│       │   └── UseCases/                             # Business logic
│       │
│       ├── Emata.Exercise.LoansManagement.Repayments/# Payments module
│       │   ├── Domain/                               # Domain entities
│       │   ├── Infrastructure/Data/                  # DbContext, migrations
│       │   └── UseCases/                             # Business logic
│       │
│       ├── Emata.Exercise.LoansManagement.Loans/     # Loans module
│       │   ├── Domain/                               # Domain entities
│       │   ├── Infrastructure/Data/                  # DbContext, migrations
│       │   └── UseCases/                             # Business logic
│       │
│       └── Emata.Exercise.LoansManagement.Shared/    # Shared infrastructure
│           ├── Endpoints/                            # Common endpoint interfaces
│           └── Infrastructure/                       # Common utilities
│
├── tests/
│   └── Emata.Exercise.LoansManagement.Tests/         # Integration tests
│
├── docs/                                             # Documentation
│   ├── architecture.md                               # Architecture overview
│   ├── module-structure.md                           # Module organization
│   └── testing.md                                    # Testing guidelines
│
└── Emata.Exercises.LoansManagement.sln               # Solution file
```

The project follows a modular monolith architecture where each business domain (Borrowers, Loans) is organized as an independent module with its own domain logic, data access, and presentation layers.

## Technology Stack

- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **C#** - Primary programming language

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Git

### Building the Project

```bash
# Clone the repository
git clone <repository-url>

# Navigate to project directory
cd Emata.Exercises.LoansManagement

# Restore dependencies
dotnet restore

# Build the solution
dotnet build
```

### Running the API

```bash
# Navigate to API project
cd src/Emata.Exercises.LoanManagement.API

# Run the application
dotnet run
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

**📋 For detailed testing information, see the [Testing Guide](docs/testing.md)**

## API Endpoints

The API will be available at `https://localhost:7xxx` when running locally. Check the `Properties/launchSettings.json` file for exact port numbers.

For interactive API documentation, navigate to `/scalar/v1` in your browser when running in development mode.

## Documentation

Comprehensive documentation is available in the `/docs` folder:

### 📚 Documentation Menu

- **[🏗️ Architecture Overview](docs/architecture.md)** - Learn about the modular monolith architecture, benefits, communication patterns, and technology stack
- **[📁 Module Structure](docs/module-structure.md)** - Understand how each module is structured with Domain, Infrastructure, and Presentation layers
- **[🧪 Testing Guide](docs/testing.md)** - Comprehensive guide on testing requirements, tools, and how to run tests

### Quick Navigation

| Topic | Description | Link |
|-------|-------------|------|
| **System Architecture** | Modular monolith pattern, modules, communication, tech stack | [architecture.md](docs/architecture.md) |
| **Code Organization** | Domain/Infrastructure/Presentation structure, migrations | [module-structure.md](docs/module-structure.md) |
| **Testing** | Testing tools, requirements, and how to run tests | [testing.md](docs/testing.md) |

## Contributing

We welcome contributions to this project! To ensure consistency and quality, please follow these guidelines:

### Development Conventions

- **Follow Existing Patterns**: Adhere to the architectural patterns and coding conventions already established in the project
- **Modular Structure**: Organize code within the appropriate module (Borrowers, Loans, or Shared). If your feature doesn't fit into any existing module, consider creating a new module following the same structure
- **Naming Conventions**: Follow C# naming conventions and maintain consistency with existing code
- **Code Organization**: Keep domain logic in Domain folders, data access in Infrastructure, and HTTP endpoints in Presentation

### Testing Requirements

- **Integration Tests**: All new features and endpoints must include integration tests
- **Test Naming**: Follow the existing test naming conventions in the test project
- **Test Coverage**: Ensure your tests cover both success and failure scenarios
- **Use Existing Setup**: Leverage the existing test infrastructure and helpers (e.g., `WebApplicationFactory`, test fixtures)

### Before Submitting

1. Ensure all tests pass by running `dotnet test`
2. Build the solution without errors using `dotnet build`
3. Verify your changes work as expected by running the API locally
4. Update documentation if you've added new features or changed existing behavior
