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

### Frontend
* **Framework:** React 19
* **Build Tool:** Vite
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
```

---

## 🚀 Getting Started

### Prerequisites
* [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
* [Node.js (v18+)](https://nodejs.org/) & npm
* SQL Server or SQL Server LocalDB

### 1. Backend Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/Methu25/SkillNet.git
   cd SkillNet
   ```
2. Navigate to `SkillNet.WebApi` and update `appsettings.json` with your credentials:
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
1. Open a new terminal and navigate to `skillnet.client`:
   ```bash
   cd skillnet.client
   npm install
   ```
2. Launch the Vite development server:
   ```bash
   npm run dev
   ```
3. Access the application at `http://localhost:5173`.

---

## 🧪 Testing & Verification

To execute the unit and integration test suite:
```bash
cd SkillNet.Tests
dotnet test
```
See `testing_results.md` for end-to-end verification reports and test suite breakdowns.

---

## 📄 License & Credits
Built by Group 26 for the **Software Architecture Module** at **NSBM Green University**.
