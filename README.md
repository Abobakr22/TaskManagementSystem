# Task Management System API

A robust and scalable RESTful API built with **.NET 9** applying **Clean Architecture** principles and **SOLID** design patterns. This system is designed to manage tasks efficiently with features like background processing, distributed caching, and secure authentication.

## 🚀 Features

* **Clean Architecture:** Separation of concerns (Domain, Application, Infrastructure, API).
* **JWT Authentication & Authorization:** Secure endpoints with Role-based access.
* **Background Task Processing:** Asynchronous status updates using `.NET Channels` (In-Memory Queue) and `IHostedService`.
* **Distributed Caching:** Integrated **Redis** to dramatically optimize read operations and reduce database load.
* **Entity Framework Core 9:** Code-first approach with SQL Server.

## 🛠️ Technologies Used

* .NET 9.0 (ASP.NET Core Web API)
* Entity Framework Core 9.0
* MS SQL Server
* Redis (Distributed Caching)
* BCrypt (Password Hashing)
* Swagger / OpenAPI

## ⚙️ Prerequisites

Before running the project, ensure you have the following installed:
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* MS SQL Server (LocalDB or full instance)
* Docker Desktop (for running the Redis container)

## 🏃‍♂️ How to Run the Project

### 1. Start Redis Server
Run the following Docker command to start a local Redis container:
```bash
docker run --name task-redis -p 6379:6379 -d redis