# 🛠️ LocalLink — Tech Stack & Tooling Guide

Tài liệu chi tiết về toàn bộ **ngôn ngữ lập trình, framework, cơ sở dữ liệu, thư viện và bộ công cụ DevOps** được áp dụng trong dự án **LocalLink (Cloud-native Regional Banking & Local Services Platform)**.

---

## 1. Tổng quan Công nghệ (High-level Matrix)

| Lĩnh vực | Công nghệ chính | Phiên bản | Mục đích sử dụng |
| :--- | :--- | :--- | :--- |
| **Backend Core** | C# / .NET | `10.0` (LTS) | Phát triển Web API xử lý nghiệp vụ ngân hàng tốc độ cao |
| **Framework API** | ASP.NET Core Web API | `10.0` | Xây dựng RESTful API Gateway, Controller, Middleware |
| **Data Access / ORM** | Entity Framework Core | `10.0.11` | Mapping đối tượng với CSDL, Migration, LINQ Query |
| **Database Engine** | Microsoft SQL Server | `2022 (Latest)` | Hệ quản trị CSDL quan hệ chính (ACID compliance) |
| **Frontend Core** | TypeScript / JavaScript | `5.7+` / `ES2024` | Đảm bảo tính chặt chẽ về type-safety trong UI |
| **Frontend Framework**| Nuxt / Vue.js | `Nuxt 4 / Vue 3.5` | SSR/SPA Framework cho Web Banking Client |
| **UI Component System**| Quasar Framework | `2.18+` | Thư viện UI chuẩn Enterprise & Material Design |
| **Containerization** | Docker Engine | `29.1+` | Đóng gói môi trường ứng dụng và database |
| **Orchestration** | Docker Compose | `v5.0+` | Quản lý multi-containers (Web, API, DB, Network, Volume)|
| **API Documentation** | OpenAPI / Swagger | `Swashbuckle 10.2` | Tự động sinh tài liệu API tương tác cho developer |
| **System Diagnostics**| ASP.NET Health Checks | `10.0` | Giám sát trạng thái hoạt động API và kết nối SQL Server |

---

## 2. Chi tiết Backend Stack (ASP.NET Core & C#)

### 2.1. Ngôn ngữ & Runtime
- **C# 14 (.NET 10)**:
  - Tận dụng các tính năng mới: Primary Constructors, Pattern Matching, Record Types, Top-level statements.
  - Tối ưu hóa hiệu năng bộ nhớ (`Span<T>`, `Memory<T>`), Async/Await Non-blocking I/O phục vụ giao dịch tài chính.

### 2.2. Frameworks & Packages
- **`Microsoft.AspNetCore.App`**:
  - Tích hợp sẵn Kestrel Web Server, Dependency Injection (DI) Container, Logging abstractions.
- **`Microsoft.EntityFrameworkCore.SqlServer` (`v10.0.11`)**:
  - Database Provider chính thức của Microsoft cho SQL Server.
  - Hỗ trợ Connection Resiliency (`EnableRetryOnFailure`), Query Splitting, Batch Execution.
- **`Microsoft.EntityFrameworkCore.Design` (`v10.0.11`)**:
  - Bộ công cụ sinh Migration (`dotnet-ef`) và quản lý Database Schema theo phương pháp Code-First.
- **`Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore` (`v10.0.11`)**:
  - Health check kiểm tra trạng thái sống của CSDL và tự động báo về endpoint `/health`.
- **`Swashbuckle.AspNetCore` (`v10.2.3`) & `Microsoft.AspNetCore.OpenApi`**:
  - Giao diện Swagger UI trực quan tại `/swagger`, hỗ trợ export schema `swagger.json`.

---

## 3. Chi tiết Frontend Stack (Nuxt 3 & Quasar)

### 3.1. Framework & Core Libraries
- **Nuxt 3 / Nuxt 4 Engine**:
  - Tích hợp Nitro Engine, tự động import composables, định tuyến linh hoạt file-based routing.
  - Hỗ trợ Runtime Config an toàn (`NUXT_PUBLIC_API_BASE_URL`).
- **Vue.js 3 (Composition API & `<script setup>`)**:
  - Reactivity System tân tiến dựa trên ES6 Proxies.
  - Quản lý logic phân tách rõ ràng theo composables.
- **TypeScript**:
  - Strict mode, định nghĩa Interface cho tất cả DTOs, API Requests/Responses nhằm hạn chế lỗi Runtime.

### 3.2. UI & Component System
- **Quasar Framework 2.x**:
  - Cung cấp hơn 70+ components chuẩn Responsive & Material Design: `QCard`, `QBadge`, `QBtn`, `QIcon`, `QSpinner`, `QTable`, `QInput`.
- **Typography & Icons**:
  - **Fonts**: *Outfit* (Tiêu đề hiện đại), *Plus Jakarta Sans* (Nội dung tài chính rõ nét).
  - **Icons**: `@quasar/extras/material-icons`.

---

## 4. Cơ sở Dữ liệu & Lưu trữ (Database & Persistence)

### 4.1. Microsoft SQL Server 2022
- **Image**: `mcr.microsoft.com/mssql/server:2022-latest`
- **Tính chất**:
  - Chạy 100% trong Docker Container (không cần cài đặt cồng kềnh trực tiếp lên máy host).
  - Đảm bảo tính toàn vẹn dữ liệu chuẩn **ACID** (Atomicity, Consistency, Isolation, Durability) — tiêu chuẩn tối quan trọng cho hệ thống tài chính ngân hàng.
- **Persistence Strategy**:
  - Dữ liệu lưu trữ độc lập qua Docker Named Volume: `locallink_sql_data` mount tới `/var/opt/mssql`.
  - Dữ liệu không bị mất khi stop, restart hay recreate containers.

---

## 5. DevOps & Containerization

### 5.1. Multi-Stage Docker Builds
- **Backend Dockerfile**:
  - *Stage 1 (Build)*: Sử dụng `mcr.microsoft.com/dotnet/sdk:10.0` để `restore`, `build` và `publish`.
  - *Stage 2 (Runtime)*: Sử dụng image siêu nhẹ `mcr.microsoft.com/dotnet/aspnet:10.0` để chạy ứng dụng (giảm kích thước container và tăng tính bảo mật).
- **Frontend Dockerfile**:
  - *Stage 1 (Builder)*: `node:24-alpine` cài đặt dependencies và compile Nuxt (`npm run build`).
  - *Stage 2 (Runner)*: `node:24-alpine` chỉ chứa thư mục `.output` và Nitro runtime.

### 5.2. Docker Compose
- Quản lý mạng nội bộ cô lập `locallink-network`.
- Thiết lập dependency có điều kiện: `locallink-api` chỉ khởi động khi `locallink-sqlserver` đã đạt trạng thái `healthy`.

---

## 6. Công cụ Phát triển Khuyến nghị (Development Tooling)

- **IDE / Code Editor**:
  - Visual Studio Code / Visual Studio 2022 / JetBrains Rider.
- **VS Code Extensions hữu ích**:
  - *C# Dev Kit* / *OmniSharp*
  - *Vue - Official (Volar)*
  - *Docker* & *Database Client*
- **Kiểm thử API**:
  - Built-in Swagger UI: `http://localhost:8080/swagger`
  - REST Client / Postman / cURL.
- **Quản lý CSDL (GUI Client)**:
  - DBeaver / Azure Data Studio / SQL Server Management Studio (SSMS) kết nối qua `localhost:1433`.
