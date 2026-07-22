# SkillNet 🚀

SkillNet is a modern, AI-powered recruitment and job matching platform built with Clean Architecture. It seamlessly connects candidates with recruiters by leveraging advanced skill-matching strategies, including integration with the Gemini API, to ensure the best fit for both parties.

## 🌟 Features

*   **Role-Based Access Control:** Secure dashboards and functionalities tailored for Candidates, Recruiters, Hiring Managers, and Administrators.
*   **AI-Powered Job Matching:** Utilizes `GeminiMatchAnalysisProvider` and `RequiredSkillCoverageStrategy` to intelligently match candidate skills with job requirements.
*   **Comprehensive Candidate Profiles:** Robust tools for candidates to build profiles, upload resumes, and track skill coverage.
*   **End-to-End Recruitment Workflow:** Complete lifecycle management from job posting and application tracking to interview scheduling.
*   **Secure Authentication:** Robust JWT-based authentication with sliding refresh tokens, persistent sessions, and strict password policies.
*   **Modern Tech Stack:** Built on a robust .NET Core backend with a fast, responsive React frontend.

## 🛠️ Technology Stack

**Backend (Clean Architecture):**
*   **Framework:** ASP.NET Core Web API
*   **ORM:** Entity Framework Core (SQL Server / LocalDB)
*   **Authentication:** JWT Bearer Authentication# SkillNet

## About

SkillNet is an AI-powered recruitment and job matching platform. Built on Clean Architecture, it seamlessly connects candidates with recruiters by leveraging advanced skill-matching strategies—including integration with the Gemini API—to evaluate candidate profiles and ensure optimal job alignment.

## Tech Stack

### Backend

* **Framework:** ASP.NET Core Web API (Domain-Driven Design)
* **Database / ORM:** Entity Framework Core (SQL Server / LocalDB)
* **Security:** JWT Bearer Authentication
* **AI Integration:** Gemini API

### Frontend

* **Framework:** React 19
* **Build Tool:** Vite
* **Routing:** React Router DOM
*   **AI Integration:** Gemini API
*   **Architecture:** Domain-Driven Design (DDD) principles with Application, Domain, Infrastructure, and WebApi layers.

**Frontend:**
*   **Framework:** React 19
*   **Build Tool:** Vite
*   **Routing:** React Router DOM

## 📂 Project Structure

*   `SkillNet.Domain`: Core business models and interfaces.
*   `SkillNet.Application`: Business logic, services, and policies (e.g., matching strategies).
*   `SkillNet.Infrastructure`: Data access, repositories, external services (Email, Gemini, Storage).
*   `SkillNet.WebApi`: REST API controllers, middleware, and configuration.
*   `skillnet.client`: React frontend application.
*   `SkillNet.Tests`: Unit and integration test suites.
*   `SkillNetDocs`: Detailed PDF documentation for all system modules.

## 🚀 Getting Started

### Prerequisites

*   [.NET SDK](https://dotnet.microsoft.com/download)
*   [Node.js](https://nodejs.org/) (for the React frontend)
*   SQL Server (or LocalDB)

### Setup Instructions

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/your-repo/SkillNet.git
    cd SkillNet
    ```

2.  **Configure Backend:**
    *   Navigate to the `SkillNet.WebApi` directory.
    *   Update `appsettings.json` or use user secrets to set your database connection string and Gemini API key:
        ```json
        {
          "ConnectionStrings": {
            "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SkillNetDb;Trusted_Connection=True;"
          },
          "Jwt": {
            "Key": "YourSuperSecretHighlySecureKeyWithAtLeast32Characters!",
            "Issuer": "your-issuer",
            "Audience": "your-audience"
          },
          "Gemini": {
            "BaseUrl": "https://generativelanguage.googleapis.com",
            "ApiKey": "YOUR_GEMINI_API_KEY"
          }
        }
        ```
    *   Start the backend (this will automatically run database migrations on startup):
        ```bash
        dotnet run
        ```

3.  **Configure Frontend:**
    *   Navigate to the `skillnet.client` directory.
    *   Install dependencies:
        ```bash
        npm install
        ```
    *   Start the development server:
        ```bash
        npm run dev
        ```

4.  **Access the Application:**
    *   Frontend: Typically `http://localhost:5173`
    *   Swagger API Docs: Typically `https://localhost:<port>/swagger`

## 📄 Documentation

Detailed module documentation can be found in the `SkillNetDocs` directory:
*   Authentication & Security Module
*   Admin Module
*   Application Module
*   Candidate Module
*   Interview Module
*   Job & Recruiter Module

## 🧪 Testing

The project includes a comprehensive test suite. To run tests:
```bash
cd SkillNet.Tests
dotnet test
```
See `testing_results.md` for recent verification results and end-to-end frontend/backend testing walkthroughs.

---
*Built by the group 11 for the Software Architecture Module by NSBM Green University *
