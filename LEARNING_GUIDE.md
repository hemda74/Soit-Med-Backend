# Learning Guide for SoitMed Backend

## 📚 Overview

This guide provides a structured learning path to understand and work with the SoitMed backend. The backend is built with **ASP.NET Core 8.0** using modern .NET patterns and practices.

---

## 🎯 Prerequisites

Before diving deep, you should have:

- Basic understanding of **C#** programming language
- Familiarity with **object-oriented programming (OOP)** concepts
- Basic knowledge of **SQL** and databases
- Understanding of **HTTP** and **REST APIs**

---

## 📖 Core Technologies to Learn (In Order)

### 1. **C# Fundamentals** ⭐⭐⭐⭐⭐

**Priority: CRITICAL**

**What to learn:**

- C# syntax and language features
- LINQ (Language Integrated Query) - heavily used in this codebase
- Async/await patterns (all services use async methods)
- Generics (used extensively in repositories)
- Nullable reference types
- Extension methods

**Resources:**

- [Microsoft C# Documentation](https://learn.microsoft.com/en-us/dotnet/csharp/)
- [C# 8.0+ Features](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8)
- Practice: Write simple console apps using LINQ and async/await

**Why it's important:** Every file in this backend uses C#. Understanding LINQ is crucial since Entity Framework Core uses it extensively.

---

### 2. **ASP.NET Core Fundamentals** ⭐⭐⭐⭐⭐

**Priority: CRITICAL**

**What to learn:**

- ASP.NET Core architecture and request pipeline
- Dependency Injection (DI) - used throughout the codebase
- Middleware (see `Program.cs` for examples)
- Configuration system (`appsettings.json`)
- Model binding and validation
- Action filters and attributes

**Key files to study:**

- `Program.cs` - Shows how the application is configured
- `BaseController.cs` - Base class for all controllers
- `Middleware/GlobalExceptionMiddleware.cs` - Custom middleware example

**Resources:**

- [ASP.NET Core Fundamentals](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/)
- [Dependency Injection in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Middleware in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware/)

**Why it's important:** This is the framework the entire backend is built on. Understanding DI is essential since all services, repositories, and controllers use it.

---

### 3. **Entity Framework Core** ⭐⭐⭐⭐⭐

**Priority: CRITICAL**

**What to learn:**

- DbContext and DbSet
- Code-First approach (this project uses migrations)
- LINQ queries with EF Core
- Relationships (one-to-many, many-to-many)
- Migrations (see `Migrations/` folder)
- Change tracking
- Eager loading vs Lazy loading
- Raw SQL queries (used in some repositories)

**Key files to study:**

- `Models/Context.cs` - The main DbContext
- `Models/*.cs` - Entity models
- `Migrations/*.cs` - Database migration files
- Any repository file (e.g., `Repositories/ActivityLogRepository.cs`)

**Resources:**

- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- [EF Core Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [EF Core Relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)

**Why it's important:** All database operations go through EF Core. The `Context.cs` file shows all entities, and repositories use EF Core to query data.

---

### 4. **Repository Pattern & Unit of Work** ⭐⭐⭐⭐

**Priority: HIGH**

**What to learn:**

- Repository pattern concept
- Generic repositories
- Unit of Work pattern
- Interface-based design
- Transaction management

**Key files to study:**

- `Repositories/BaseRepository.cs` - Generic repository implementation
- `Repositories/UnitOfWork.cs` - Unit of Work implementation
- `Repositories/IUnitOfWork.cs` - Interface definition
- `Services/SalesmanStatsService.cs` - Shows how services use UnitOfWork

**Resources:**

- [Repository Pattern in C#](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Unit of Work Pattern](https://martinfowler.com/eaaCatalog/unitOfWork.html)

**Why it's important:** This is the core data access pattern used throughout the backend. Services use `IUnitOfWork` to access repositories, not direct DbContext access.

---

### 5. **ASP.NET Core Identity & JWT Authentication** ⭐⭐⭐⭐

**Priority: HIGH**

**What to learn:**

- ASP.NET Core Identity system
- User and Role management
- JWT (JSON Web Tokens) authentication
- Authorization attributes (`[Authorize]`, `[AllowAnonymous]`)
- Role-based authorization
- Claims and user context

**Key files to study:**

- `Program.cs` (lines 180-320) - Identity and JWT configuration
- `Models/Identity/ApplicationUser.cs` - Custom user model
- `Common/BaseController.cs` - Shows how to get current user
- Any controller with `[Authorize]` attribute

**Resources:**

- [ASP.NET Core Identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [JWT Authentication in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [Authorization in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/introduction)

**Why it's important:** All API endpoints require authentication. Understanding how JWT works and how to access the current user is essential.

---

### 6. **RESTful API Design** ⭐⭐⭐⭐

**Priority: HIGH**

**What to learn:**

- HTTP methods (GET, POST, PUT, DELETE, PATCH)
- REST conventions
- API routing
- Status codes
- Request/Response patterns
- API versioning (if used)

**Key files to study:**

- Any file in `Controllers/` folder
- `Common/ResponseHelper.cs` - Standardized response format
- `Common/BaseController.cs` - Helper methods for responses

**Resources:**

- [REST API Best Practices](https://restfulapi.net/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api/)

**Why it's important:** All endpoints follow REST conventions. Understanding HTTP methods and status codes is crucial.

---

### 7. **DTOs (Data Transfer Objects) & Validation** ⭐⭐⭐

**Priority: MEDIUM**

**What to learn:**

- DTO pattern and why it's used
- Mapping between entities and DTOs
- FluentValidation library
- Input validation
- Model validation

**Key files to study:**

- Any file in `DTO/` folder
- `Validators/*.cs` - FluentValidation examples
- `Common/ValidationHelper.cs` - Validation utilities

**Resources:**

- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
- [DTO Pattern](https://martinfowler.com/eaaCatalog/dataTransferObject.html)

**Why it's important:** All API endpoints use DTOs for request/response. Validation ensures data integrity.

---

### 8. **SignalR (Real-time Communication)** ⭐⭐⭐

**Priority: MEDIUM**

**What to learn:**

- SignalR hubs
- Real-time messaging
- Client-server communication
- WebSocket connections

**Key files to study:**

- `Hubs/NotificationHub.cs` - SignalR hub implementation
- `Program.cs` (line 214, 516) - SignalR configuration

**Resources:**

- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)

**Why it's important:** Used for real-time notifications in the system.

---

### 9. **SQL Server** ⭐⭐⭐

**Priority: MEDIUM**

**What to learn:**

- SQL Server basics
- T-SQL queries
- Stored procedures (if used)
- Database design principles
- Indexes and performance

**Resources:**

- [SQL Server Documentation](https://learn.microsoft.com/en-us/sql/sql-server/)
- [T-SQL Fundamentals](https://learn.microsoft.com/en-us/sql/t-sql/fundamentals/)

**Why it's important:** The backend uses SQL Server as the database. Understanding SQL helps with debugging and optimization.

---

### 10. **Advanced Topics** ⭐⭐

**Priority: LOW (Learn as needed)**

**What to learn:**

- **Swagger/OpenAPI** - API documentation (see `Program.cs` lines 323-375)
- **Health Checks** - Application monitoring (see `Program.cs` lines 130-141)
- **Rate Limiting** - API throttling (see `Program.cs` lines 229-267)
- **File Upload Handling** - See `Program.cs` lines 93-122
- **Logging** - See `Program.cs` lines 32-70
- **Exception Handling** - See `Middleware/GlobalExceptionMiddleware.cs`

---

## 🗺️ Recommended Learning Path

### **Week 1-2: Foundation**

1. Review C# fundamentals (especially LINQ and async/await)
2. Learn ASP.NET Core basics
3. Understand Dependency Injection
4. Study `Program.cs` to see how everything is wired together

### **Week 3-4: Data Access**

1. Learn Entity Framework Core
2. Study the Repository Pattern
3. Understand Unit of Work
4. Review `Models/Context.cs` and a few repository files
5. Learn about migrations

### **Week 5-6: API Development**

1. Learn RESTful API design
2. Study controller examples in `Controllers/` folder
3. Understand DTOs and validation
4. Learn about `BaseController` and response helpers

### **Week 7-8: Security & Advanced Features**

1. Learn ASP.NET Core Identity
2. Understand JWT authentication
3. Study authorization patterns
4. Learn SignalR basics
5. Review middleware and advanced features

---

## 🔍 How to Explore This Codebase

### **Start Here:**

1. **`Program.cs`** - The entry point. Shows all services, middleware, and configuration
2. **`Models/Context.cs`** - Database schema overview
3. **`Common/BaseController.cs`** - Base functionality for all controllers
4. **`Services/SalesmanStatsService.cs`** - Good example of service layer

### **Then Explore:**

1. Pick a simple controller (e.g., `DepartmentController.cs`)
2. Trace the flow: Controller → Service → Repository → Database
3. Look at the corresponding DTOs
4. Check the entity model
5. Review the repository implementation

### **Understanding the Flow:**

```
HTTP Request
    ↓
Controller (handles HTTP, authorization)
    ↓
Service (business logic)
    ↓
UnitOfWork (transaction management)
    ↓
Repository (data access)
    ↓
Entity Framework Core
    ↓
SQL Server Database
```

---

## 📝 Key Patterns Used in This Codebase

### 1. **Dependency Injection**

All services, repositories, and controllers are registered in `Program.cs` and injected via constructors.

### 2. **Repository Pattern**

- Generic `BaseRepository<T>` for common operations
- Specific repositories for complex queries
- All accessed through `IUnitOfWork`

### 3. **Service Layer**

- Business logic in services (not controllers)
- Services use `IUnitOfWork` to access data
- Services return DTOs, not entities

### 4. **DTO Pattern**

- Separate classes for API communication
- Entities stay in the data layer
- Mapping between entities and DTOs

### 5. **Async/Await**

- All I/O operations are async
- Controllers, services, and repositories use async methods
- Improves scalability

---

## 🛠️ Practical Exercises

### **Exercise 1: Understand a Simple Endpoint**

1. Find `DepartmentController.cs`
2. Trace a GET endpoint from controller to database
3. Understand how the response is formatted

### **Exercise 2: Add a New Feature**

1. Create a new entity model
2. Create a migration
3. Create a repository interface and implementation
4. Register in UnitOfWork
5. Create a service
6. Create a controller endpoint
7. Create DTOs
8. Test with Swagger

### **Exercise 3: Understand Authentication**

1. Find a protected endpoint
2. See how `[Authorize]` works
3. Understand how `BaseController.GetCurrentUserId()` works
4. Trace JWT token validation

---

## 📺 Arabic YouTube Tutorials (دروس عربية على يوتيوب)

### **قنوات موصى بها للتعلم بالعربية:**

#### 1. **قناة عبدالله عيد (Abdullah Eid)**

- **المحتوى:** C#، ASP.NET Core، Entity Framework
- **البحث:** "عبدالله عيد ASP.NET Core" أو "Abdullah Eid C#"
- **مميزات:** شرح واضح ومنظم، مشاريع عملية

#### 2. **قناة مصطفى الرضايدة**

- **المحتوى:** ASP.NET MVC، Entity Framework، C#
- **البحث:** "مصطفى الرضايدة ASP.NET" أو "Learn ASP.Net MVC & Entity Framework in Arabic"
- **مميزات:** سلسلة دروس متكاملة

#### 3. **قناة Elzero Web School (الزيرو)**

- **المحتوى:** برمجة عامة، قد تحتوي على دروس C#
- **البحث:** "Elzero C#" أو "الزيرو C#"
- **مميزات:** شرح مبسط للمبتدئين

#### 4. **قناة Korsat Code (كورسات كود)**

- **المحتوى:** ASP.NET Core، C#، Entity Framework
- **البحث:** "Korsat Code ASP.NET" أو "كورسات كود"
- **مميزات:** دورات شاملة ومفصلة

#### 5. **قناة Codezilla (كودزيلا)**

- **المحتوى:** C#، .NET، ASP.NET Core
- **البحث:** "Codezilla C#" أو "كودزيلا"
- **مميزات:** محتوى عربي جيد

### **كلمات بحث موصى بها على YouTube:**

**لـ C#:**

- "تعلم C# من الصفر بالعربي"
- "دورة C# كاملة بالعربية"
- "C# للمبتدئين بالعربي"

**لـ ASP.NET Core:**

- "ASP.NET Core بالعربي"
- "تعلم ASP.NET Core من الصفر"
- "دورة ASP.NET Core Web API بالعربية"
- "ASP.NET Core REST API بالعربي"

**لـ Entity Framework:**

- "Entity Framework Core بالعربي"
- "تعلم Entity Framework من الصفر"
- "Entity Framework Code First بالعربية"
- "Migrations في Entity Framework بالعربي"

**لـ Repository Pattern:**

- "Repository Pattern C# بالعربي"
- "Unit of Work Pattern بالعربية"
- "Design Patterns C# بالعربي"

**لـ Authentication & Authorization:**

- "JWT Authentication ASP.NET Core بالعربي"
- "ASP.NET Core Identity بالعربية"
- "Authentication Authorization بالعربي"

### **نصائح للبحث:**

1. استخدم كلمات البحث بالعربية والإنجليزية معاً
2. ابحث عن "دورة كاملة" أو "Complete Course" للحصول على سلسلة متكاملة
3. راجع التعليقات لتقييم جودة المحتوى
4. ابحث عن دورات حديثة (2023-2024) للحصول على أحدث الإصدارات

### **دروس محددة موصى بها:**

1. **ASP.NET Core Web API:**

      - ابحث عن: "ASP.NET Core Web API بالعربي"
      - ابحث عن: "REST API C# بالعربية"

2. **Entity Framework Core:**

      - ابحث عن: "Entity Framework Core Migrations بالعربي"
      - ابحث عن: "Code First Entity Framework بالعربية"

3. **Repository Pattern:**

      - ابحث عن: "Repository Pattern Implementation C#"
      - ابحث عن: "Unit of Work Pattern C#"

4. **JWT Authentication:**
      - ابحث عن: "JWT Token ASP.NET Core بالعربي"
      - ابحث عن: "Authentication Authorization بالعربية"

### **قنوات إضافية:**

- **قناة Abdelrahman Gamal** - دروس C# و .NET
- **قناة Mohamed Shalaby** - برمجة وتطوير
- **قناة Ahmed Ibrahim** - ASP.NET Core

### **ملاحظات مهمة:**

- تأكد من أن الدروس تستخدم **ASP.NET Core 6.0+** أو **8.0** (الإصدار المستخدم في هذا المشروع)
- تجنب الدروس القديمة التي تستخدم **ASP.NET Framework** (ليس Core)
- ابحث عن دروس تتضمن **Entity Framework Core** وليس Entity Framework القديم

---

## 📚 Additional Resources

### **Official Documentation:**

- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

### **Books:**

- "Pro ASP.NET Core" by Adam Freeman
- "Entity Framework Core in Action" by Jon Smith

### **Video Courses:**

- Microsoft Learn (free)
- Pluralsight
- Udemy ASP.NET Core courses

### **Practice:**

- Build a small CRUD API from scratch
- Implement Repository Pattern in a simple project
- Practice with Entity Framework Core migrations

---

## 🎯 Quick Reference: File Structure

```
Backend/SoitMed/
├── Controllers/        # API endpoints (HTTP layer)
├── Services/          # Business logic layer
├── Repositories/      # Data access layer
├── Models/            # Entity models (database)
├── DTO/               # Data Transfer Objects (API contracts)
├── Common/            # Shared utilities and base classes
├── Middleware/        # Custom middleware
├── Hubs/              # SignalR hubs
├── Validators/        # FluentValidation validators
├── Migrations/        # Database migrations
└── Program.cs         # Application startup and configuration
```

---

## 💡 Tips for Learning

1. **Read code with a purpose** - Don't just read, try to understand WHY something is done a certain way
2. **Use the debugger** - Set breakpoints and step through code to see the flow
3. **Modify and test** - Make small changes and see what happens
4. **Read error messages** - They often point to the exact issue
5. **Use Swagger** - Test endpoints directly to see how they work
6. **Check the database** - After running migrations, check SQL Server to see the actual tables
7. **Read documentation** - The codebase has extensive documentation files in the `Backend/` folder

---

## ❓ Common Questions

**Q: Where is the database connection string?**
A: `appsettings.json` or `appsettings.Development.json`

**Q: How do I add a new API endpoint?**
A: Add a method to a controller, create/use a service, use UnitOfWork to access data

**Q: How are services registered?**
A: In `Program.cs` using `builder.Services.AddScoped<>()` or `AddApplicationServices()`

**Q: Where is business logic?**
A: In the `Services/` folder, not in controllers

**Q: How do I access the current user?**
A: Use `GetCurrentUserId()` or `GetCurrentUserAsync()` from `BaseController`

---

## 🚀 Next Steps

Once you're comfortable with the basics:

1. Study the more complex modules (Sales, Maintenance, Payment)
2. Understand the workflow systems
3. Learn about SignalR real-time features
4. Explore the notification system
5. Study the file upload handling
6. Understand the role-based authorization system

---

**Good luck with your learning journey! 🎓**
