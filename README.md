# Task Manager API

Task Manager API is a robust system designed to manage tasks with advanced features like authentication, authorization, validation, and more. The API is built with scalability, security, and maintainability in mind, using best practices and modern technologies.

---

## **Project Overview**

The Task Manager API allows users to:
- Create, update, delete, and manage tasks.
- Authenticate using JWT.
- Filter tasks by date range and other parameters.
- Protect sensitive endpoints with rate limiting and brute force protection.
- Localize responses into multiple languages (English and Portuguese).

This API is suitable for learning modern development practices and can be integrated with various frontend frameworks.

---

## **Platform and Technologies**

### **Platform**
- **.NET 8**: A modern and scalable platform for building APIs.
- **Azure App Service**: Hosting platform with free-tier deployment.

### **Technologies**
- **ASP.NET Core**: For building RESTful APIs.
- **Entity Framework Core**: For database interactions.
- **SQL Server**: As the database.
- **FluentValidation**: For advanced request validation.
- **JWT Authentication**: For secure access to endpoints.
- **Swagger**: For API documentation.
- **Serilog**: For logging and diagnostics.
- **Application Insights**: For monitoring and metrics.
- **Rate Limiting**: To prevent abuse and brute force attacks.

---

## **Features**

- **Authentication & Authorization**: JWT-based secure login.
- **Task Management**: CRUD operations with filters and sorting.
- **Localization**: Multi-language support (English, Portuguese).
- **Response Caching**: Improved performance for read endpoints.
- **Middleware**: Centralized error handling and audit logging.
- **Security**: Protection against brute force attacks, XSS, and SQL injection.
- **Automated Deployment**: CI/CD pipeline with GitHub Actions.

---

## **Getting Started**

### **Prerequisites**
1. .NET 8 SDK installed.
2. SQL Server instance running (local or Azure).
3. Azure App Service (optional for deployment).
4. Visual Studio Code or any other IDE.

### **Clone the Repository**
```bash
git clone https://github.com/your-username/task-manager-api.git
cd task-manager-api
```

### **Environment Variables**
1. Copy the .env.example file to .env:
```bash
cp .env.example .env
```
2. Add your environment-specific values:
- JWT_SECRET: A secret key for JWT authentication.
- ConnectionStrings__DefaultConnection: Your SQL Server connection string.
3. Alternatively, set the environment variables directly in your terminal:
```bash
export JWT_SECRET="your-secret-key"
export ConnectionStrings__DefaultConnection="your-connection-string"
```

### **Run Locally**
1. Restore dependencies:
```bash
dotnet restore
```
2. Build the project:
```bash
dotnet build
```
3. Apply database migrations:
```bash
dotnet ef database update
```
4. Run the project:
```bash
dotnet run
```
5. Access the API via Swagger UI: http://localhost:5000/swagger

### **Deployment**
#### **Automated Deployment with GitHub Actions**
The project is configured for automated deployment to Azure App Service using GitHub Actions.
1. Ensure the following secrets are configured in your GitHub repository:
- AZURE_CREDENTIALS: JSON from Azure Service Principal.
- JWT_SECRET: Your JWT secret key.
- CONNECTIONSTRINGS_DEFAULTCONNECTION: Your database connection string.
2. The pipeline performs:
- Dependency restoration.
- Build and tests execution.
- Deployment to Azure App Service.

### **Technical Details**
#### **Architecture**
- Clean Architecture: Separation of concerns with clear boundaries between layers.
- SOLID Principles: For maintainable and extendable code.
- Middleware: Handles logging, error management, and localization centrally.

#### **Best Practices**
- Centralized error handling with custom middleware.
- Validations using FluentValidation for flexibility and reusability.
- Security measures, including rate limiting and headers to prevent XSS.
- Automated tests for controllers and middlewares.

### **License**
This project is licensed under the MIT License.

### **Contact**
For questions or issues, feel free to reach out:
- Name: Vinícius Carvalho
- Email: [vinicius.cs01@gmail.com](mailto:vinicius.cs01@gmail.com)
- GitHub: [Viniciuscs01](https://github.com/Viniciuscs01)
