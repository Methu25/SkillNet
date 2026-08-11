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

### Frontend
* **Framework:** React 19
* **Build Tool:** Vite
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
```

---

## Setup & Local Installation

### Prerequisites
* .NET 9.0 SDK
* Node.js (v18+) & npm
* SQL Server or LocalDB

### 1. Backend Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/Methu25/SkillNet.git
   cd SkillNet
   ```
2. Navigate to `SkillNet.WebApi` and configure `appsettings.json`:
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
1. Open a terminal and navigate to `skillnet.client`:
   ```bash
   cd skillnet.client
   npm install
   ```
2. Start the development server:
   ```bash
   npm run dev
   ```
3. Open `http://localhost:5173` in your browser.

---

## Testing

Run unit and integration tests with:
```bash
cd SkillNet.Tests
dotnet test
```

---

## License & Credits
Developed by Group 26 for the Software Architecture module at NSBM Green University.
