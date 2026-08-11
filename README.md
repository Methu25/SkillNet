<<<<<<< HEAD
# SkillNet 🚀
> **AI-Powered Intelligent Recruitment & Job Matching Platform**  
> *Built with ASP.NET Core Clean Architecture, React 19, and Google Gemini AI.*

---

## 🌟 Overview

**SkillNet** is a modern, enterprise-grade AI-powered recruitment and candidate-job matching platform. Designed using **Clean Architecture (N-Tier)** principles, SkillNet seamlessly connects job seekers with hiring teams by leveraging real-time skill matching algorithms and **Google Gemini AI API** analysis to calculate skill coverage, score resumes, and streamline hiring workflows.

---

## 👥 Contributors & Core Engineering Roles

SkillNet was developed as a group project for the **Software Architecture Module at NSBM Green University** (Group 26). Below is the breakdown of key engineering contributions as recorded in the system architecture and Git history:

### 🛡️ E.J. Yohan Methusael ([@Methu25](https://github.com/Methu25))
> **Lead Security Architect, AI Integration Specialist & Application Flow Lead**

* **Authentication & Security Infrastructure:**
  * Designed and engineered the end-to-end **JWT Bearer Authentication** and **Sliding Refresh Token** subsystem (`RefreshToken` domain entity, repository interfaces, and concrete ADO.NET SQL repositories).
  * Implemented the transactional `AuthDbSession` and `IUnitOfWork` patterns to coordinate atomic registration and login transactions inside `UserService`.
  * Configured granular **Role-Based Access Control (RBAC)** and authorization policies across API controllers for Candidates, Recruiters, Hiring Managers, and Administrators.
* **Google Gemini AI Integration:**
  * Spearheaded the integration of **Google Gemini API** (`GeminiMatchAnalysisProvider`) and custom skill evaluation policies (`RequiredSkillCoverageStrategy`) to calculate real-time candidate suitability match percentages.
* **Admin Dashboard & Security Fixes:**
  * Resolved critical SQL schema query issues in `Admin UserManagement`, fixed role authorization handling across controllers, and established organization approval fallback mechanics.
* **Frontend UI & Application Routing Flow:**
  * Redesigned the **Login** and **Register** pages with custom responsive styling, input validation, and dark/light color contrast accessibility fixes.
  * Created the main **Landing Page**, established React Router DOM route guards, and wired up end-to-end state transitions between the React 19 client and ASP.NET Core backend API.

---

### 💻 Team Engineering Collaborators
* **Yashara Gamage** ([@YasharaGamage](https://github.com/YasharaGamage)) – *Project Lead, Job & Recruiter Module, Ownership Flow & Database Setup*
* **Hansaja Mudalige** ([@HansajaMudalige](https://github.com/HansajaMudalige)) – *Admin Module UI, Dashboard Polishing & System Settings*
* **Dinuri Chamindi** ([@Dinuri2004](https://github.com/Dinuri2004)) – *Hiring Manager Module & Interview Scheduling System*
* **P.T.A. Jayasena** ([@ptajayasena](https://github.com/ptajayasena)) – *Recruiter Application Management & Candidate Tracking*

---

## 🛠️ Tech Stack & Architecture

### Backend
* **Framework:** ASP.NET Core 9.0 / Web API
* **Architecture:** Clean Architecture / N-Tier (Domain, Application, Infrastructure, WebApi)
* **Database & Persistence:** Entity Framework Core & Custom ADO.NET SQL Sessions (SQL Server / LocalDB)
* **Security & Auth:** JWT Bearer Tokens, Sliding Refresh Tokens, BCrypt Hashing, Custom `IUnitOfWork` Transaction Context
* **AI Integration:** Google Gemini AI REST API (`generativelanguage.googleapis.com`)
=======
# SkillNet
> An AI-powered recruitment and job matching platform built with ASP.NET Core Clean Architecture, React 19, and Google Gemini AI.

---

## Overview

SkillNet is a recruitment and candidate-job matching platform designed to connect job seekers with hiring teams. Built following N-Tier Clean Architecture principles, the application evaluates candidate profiles using skill-matching algorithms and Google Gemini AI to analyze candidate skill coverage and calculate match suitability for posted jobs.

---

## Team & Key Contributions

SkillNet was developed as a group project for the Software Architecture module at NSBM Green University (Group 26).

### E.J. Yohan Methusael (GitHub: @Methu25)
**Security, AI Integration & Application Flow**

* **Authentication & Security Architecture:**
  * Implemented JWT bearer authentication and sliding refresh tokens (`RefreshToken` domain entity, repository interfaces, and concrete ADO.NET SQL repositories).
  * Built transactional `AuthDbSession` and `IUnitOfWork` patterns to coordinate atomic registration and authentication operations in `UserService`.
  * Set up Role-Based Access Control (RBAC) and authorization policies across API controllers for Candidates, Recruiters, Hiring Managers, and Administrators.
* **Google Gemini AI Integration:**
  * Integrated the Google Gemini API (`GeminiMatchAnalysisProvider`) and custom skill evaluation logic (`RequiredSkillCoverageStrategy`) to calculate real-time candidate suitability match percentages.
* **Admin Module & System Fixes:**
  * Fixed SQL schema query issues in Admin User Management, corrected controller role authorization handling, and added organization approval fallback logic.
* **Frontend UI & Application Flow:**
  * Redesigned the Login and Register pages for responsive layout and input validation.
  * Built the main Landing Page, set up React Router DOM route guards, and connected the React 19 client with ASP.NET Core backend services.

---

### Team Collaborators
* **Yashara Gamage** (@YasharaGamage) - Project setup, Job & Recruiter modules, database structure.
* **Hansaja Mudalige** (@HansajaMudalige) - Admin module UI and system settings.
* **Dinuri Chamindi** (@Dinuri2004) - Hiring manager module and interview scheduling.
* **P.T.A. Jayasena** (@ptajayasena) - Recruiter application management.

---

## Tech Stack

### Backend
* **Framework:** ASP.NET Core 9.0 Web API
* **Architecture:** N-Tier Clean Architecture (Domain, Application, Infrastructure, WebApi)
* **Database & ORM:** Entity Framework Core & Custom ADO.NET SQL Sessions (SQL Server / LocalDB)
* **Security:** JWT Bearer Tokens, Sliding Refresh Tokens, BCrypt Hashing, Custom `IUnitOfWork` Transaction Context
* **AI Engine:** Google Gemini REST API
>>>>>>> E.J.Yohan-Methusael-36999

### Frontend
* **Framework:** React 19
* **Build Tool:** Vite
<<<<<<< HEAD
* **Routing:** React Router DOM (Protected Routes & Auth Context)
* **Styling:** CSS3 with modern design principles (Glassmorphism, Dark/Light theme tokens)

---

## 📂 Architecture & Project Structure

```text
SkillNet/
├── SkillNet.Domain/          # Core Domain Entities, Interfaces, Enums & Value Objects
├── SkillNet.Application/     # Use Cases, DTOs, Matching Strategies & Business Logic
├── SkillNet.Infrastructure/  # EF Core DbContext, Repositories, Gemini AI Client & Services
├── SkillNet.WebApi/          # Controllers, Middleware, Auth Handlers & API Configurations
├── skillnet.client/          # React 19 Frontend (Vite, Components, Hooks, Views)
└── SkillNet.Tests/           # Automated Unit & Integration Test Suites
=======
* **Routing:** React Router DOM
* **Styling:** CSS3 (Modern responsive layouts, light/dark accessibility fixes)

---

## Project Structure

```text
SkillNet/
├── SkillNet.Domain/          # Core Domain Entities, Interfaces & Value Objects
├── SkillNet.Application/     # Use Cases, DTOs, Matching Strategies & Policies
├── SkillNet.Infrastructure/  # EF Core DbContext, Repositories, Gemini AI Client
├── SkillNet.WebApi/          # API Controllers, Middleware & Auth Handlers
├── skillnet.client/          # React 19 Frontend Application
└── SkillNet.Tests/           # Unit & Integration Test Suites
>>>>>>> E.J.Yohan-Methusael-36999
```

---

<<<<<<< HEAD
## 🚀 Getting Started

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
* [Node.js (v18+)](https://nodejs.org/) & npm
* SQL Server or SQL Server LocalDB
=======
## Setup & Local Installation

### Prerequisites
* .NET 9.0 SDK
* Node.js (v18+) & npm
* SQL Server or LocalDB
>>>>>>> E.J.Yohan-Methusael-36999

### 1. Backend Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/Methu25/SkillNet.git
   cd SkillNet
   ```
<<<<<<< HEAD
2. Navigate to `SkillNet.WebApi` and update `appsettings.json` with your credentials:
=======
2. Navigate to `SkillNet.WebApi` and configure `appsettings.json`:
>>>>>>> E.J.Yohan-Methusael-36999
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SkillNetDb;Trusted_Connection=True;"
     },
     "Jwt": {
       "Key": "YourSuperSecretHighlySecureKeyWithAtLeast32Characters!",
       "Issuer": "SkillNetIssuer",
       "Audience": "SkillNetAudience"
     },
     "Gemini": {
       "BaseUrl": "https://generativelanguage.googleapis.com",
       "ApiKey": "YOUR_GEMINI_API_KEY"
     }
   }
   ```
3. Run the Web API backend:
   ```bash
   cd SkillNet.WebApi
   dotnet run
   ```

### 2. Frontend Setup
<<<<<<< HEAD
1. Open a new terminal and navigate to `skillnet.client`:
=======
1. Open a terminal and navigate to `skillnet.client`:
>>>>>>> E.J.Yohan-Methusael-36999
   ```bash
   cd skillnet.client
   npm install
   ```
<<<<<<< HEAD
2. Launch the Vite development server:
   ```bash
   npm run dev
   ```
3. Access the application at `http://localhost:5173`.

---

## 🧪 Testing & Verification

To execute the unit and integration test suite:
=======
2. Start the development server:
   ```bash
   npm run dev
   ```
3. Open `http://localhost:5173` in your browser.

---

## Testing

Run unit and integration tests with:
>>>>>>> E.J.Yohan-Methusael-36999
```bash
cd SkillNet.Tests
dotnet test
```
<<<<<<< HEAD
See `testing_results.md` for end-to-end verification reports and test suite breakdowns.

---

## 📄 License & Credits
Built by Group 26 for the **Software Architecture Module** at **NSBM Green University**.
=======

---

## License & Credits
Developed by Group 26 for the Software Architecture module at NSBM Green University.
>>>>>>> E.J.Yohan-Methusael-36999
