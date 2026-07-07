# Testing Evidence & Verification Results

This document serves as proof of testing for the **Authentication & Security (Phase 1)** module. It covers verification of both the ASP.NET Core Web API endpoints via Swagger and the frontend React application's dashboard routing and authorization controls.

---

## 1. Video Demonstration (E2E Walkthrough)

The following recording shows the automated browser subagent performing the complete E2E validation:
1. Navigating to Swagger UI.
2. Registering a candidate user (`verifytest@skillnet.com`).
3. Logging in via Swagger and authorizing the session.
4. Testing secure `/api/TestSecure/candidate-only` (returning HTTP 200).
5. Opening the React web application and logging in with the newly registered candidate user.
6. Testing the role-based buttons in the dashboard testing console (All Users ➔ success, Candidate ➔ success, Admin ➔ 403 Forbidden).
7. Logging out and returning to the login screen.

![E2E Verification Walkthrough](/C:/Users/yohan/.gemini/antigravity-ide/brain/de3d0180-a4e6-4505-9024-697c195ee83f/verification_walkthrough_1783421126070.webp)

---

## 2. Swagger API Verification

All key backend endpoints were called and validated successfully:

| Endpoint | HTTP Code | Description / Response |
| :--- | :--- | :--- |
| **POST `/api/auth/register`** | `200 OK` | User registration succeeds, password policy requirements are enforced. |
| **POST `/api/auth/login`** | `200 OK` | Yields a valid JWT Access Token and persistent sliding Refresh Token. |
| **GET `/api/auth/me`** | `200 OK` | Retrieves authenticated user profile details from token claims. |
| **POST `/api/auth/refresh-token`**| `200 OK` | Re-issues a new token pair when validating a valid refresh token. |
| **GET `/api/TestSecure/candidate-only`** | `200 OK` | Access allowed for users possessing the `Candidate` role claim. |
| **GET `/api/TestSecure/admin-only`** | `403 Forbidden` | Access blocked when calling with the `Candidate` token. |

---

## 3. React Frontend Verification

The following actions were tested and passed in the UI:
1. **Interactive Password Policy Validation**: Displays real-time checklists on the Registration page.
2. **Account Lockout Policy**: Triggers a 15-minute account lock after 5 failed login attempts.
3. **Role-Based Redirects**: Logins dynamically route to `/candidate-dashboard`, `/recruiter-dashboard`, `/hiring-dashboard`, or `/admin-dashboard`.
4. **Token Persistence & Slide**: Sessions persist across refreshes, automatic token refreshing handles access expiry seamlessly.
