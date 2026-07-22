import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

import LandingPage from './pages/LandingPage';
import Login from './pages/Login';
import Register from './pages/Register';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';

// Role-specific dashboards
import AdminLayout from './components/AdminLayout';
import Dashboard from './pages/Dashboard';

import HiringDashboard from './pages/HiringDashboard';
import CandidateDashboard from './pages/CandidateDashboard';
import CandidateProfileCreate from './pages/CandidateProfileCreate';
import CandidateProfile from './pages/CandidateProfile';
import CandidateResumes from './pages/CandidateResumes';
import CandidateSkills from './pages/CandidateSkills';
import CandidateApplications from './pages/CandidateApplications';
import CandidateJobs from './pages/CandidateJobs';
import RecruiterApplicants from './pages/RecruiterApplicants';

// Recruiter workspace
import RecruiterRoute from './components/recruiter/RecruiterRoute';
import RecruiterLayout from './components/recruiter/RecruiterLayout';
import RecruiterDashboard from './pages/recruiter/RecruiterDashboard';
import RecruiterJobs from './pages/recruiter/RecruiterJobs';
import CreateJob from './pages/recruiter/CreateJob';
import JobDetails from './pages/recruiter/JobDetails';
import EditJob from './pages/recruiter/EditJob';
import RecruiterCompany from './pages/recruiter/RecruiterCompany';
import RecruiterSetup from './pages/recruiter/RecruiterSetup';
import RecruiterSettings from './pages/recruiter/RecruiterSettings';

// Admin sub-pages (protected, Admin only)
import UserManagement from './pages/UserManagement';
import OrganizationManagement from './pages/OrganizationManagement';
import AuditLogs from './pages/AuditLogs';
import SystemSettings from './pages/SystemSettings';

// Hiring Manager sub-pages
import InterviewDetails from './pages/InterviewDetails';
import ScheduleInterview from './pages/ScheduleInterview';

// Utility pages
import AccessDenied from './pages/AccessDenied';
import NotFound from './pages/NotFound';

function App() {
    return (
        <AuthProvider>
            <Router>
                <Routes>
                    {/* Default root route */}
                    <Route path="/" element={<LandingPage />} />

                    {/* Public routes */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/forgot-password" element={<ForgotPassword />} />
                    <Route path="/reset-password" element={<ResetPassword />} />

                    {/* Admin routes */}
                    <Route path="/admin-dashboard" element={<Navigate to="/admin/dashboard" replace />} />
                    <Route
                        path="/admin"
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <AdminLayout />
                            </ProtectedRoute>
                        }
                    >
                        <Route index element={<Navigate to="dashboard" replace />} />
                        <Route path="dashboard" element={<Dashboard />} />
                        <Route path="users" element={<UserManagement />} />
                        <Route path="organizations" element={<OrganizationManagement />} />
                        <Route path="logs" element={<AuditLogs />} />
                        <Route path="settings" element={<SystemSettings />} />
                    </Route>

                    {/* ── Recruiter routes ── */}
                    <Route path="/recruiter-dashboard" element={<Navigate to="/recruiter/dashboard" replace />} />
                    <Route
                        path="/recruiter"
                        element={
                            <RecruiterRoute>
                                <RecruiterLayout />
                            </RecruiterRoute>
                        }
                    >
                        <Route index element={<Navigate to="dashboard" replace />} />
                        <Route path="setup" element={<RecruiterSetup />} />
                        <Route path="company" element={<RecruiterCompany />} />
                        <Route path="dashboard" element={<RecruiterDashboard />} />
                        <Route path="jobs" element={<RecruiterJobs />} />
                        <Route path="jobs/create" element={<CreateJob />} />
                        <Route path="jobs/:id" element={<JobDetails />} />
                        <Route path="jobs/:id/edit" element={<EditJob />} />
                        <Route path="settings" element={<RecruiterSettings />} />
                    </Route>

                    {/* Hiring Manager routes */}
                    <Route
                        path="/hiring-dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['HiringManager']}>
                                <HiringDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/interviews/:id"
                        element={
                            <ProtectedRoute allowedRoles={['HiringManager']}>
                                <InterviewDetails />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/schedule-interview"
                        element={
                            <ProtectedRoute allowedRoles={['HiringManager']}>
                                <ScheduleInterview />
                            </ProtectedRoute>
                        }
                    />

                    {/* Candidate routes */}
                    <Route
                        path="/candidate-dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/profile"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateProfile />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/profile/create"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateProfileCreate />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/resumes"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateResumes />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/skills"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateSkills />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/recruiter/jobs/:jobId/applicants"
                        element={
                            <ProtectedRoute allowedRoles={['Recruiter']}>
                                <RecruiterApplicants />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/recruiter/jobs/:jobId/applicants/:applicationId/schedule"
                        element={
                            <ProtectedRoute allowedRoles={['Recruiter']}>
                                <ScheduleInterview />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/recruiter/jobs/:jobId/applicants/:applicationId"
                        element={
                            <ProtectedRoute allowedRoles={['Recruiter']}>
                                <RecruiterApplicants />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/applications"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateApplications />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/applications/:applicationId"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateApplications />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/jobs"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateJobs />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/candidate/jobs/:jobId"
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateJobs />
                            </ProtectedRoute>
                        }
                    />

                    {/* Utility routes */}
                    <Route path="/access-denied" element={<AccessDenied />} />
                    <Route path="/404" element={<NotFound />} />
                    <Route path="*" element={<Navigate to="/404" replace />} />
                </Routes>
            </Router>
        </AuthProvider>
    );
}

export default App;
