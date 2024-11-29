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
