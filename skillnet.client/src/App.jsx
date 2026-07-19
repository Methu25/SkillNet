import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

import Login from './pages/Login';
import Register from './pages/Register';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';

// Role-specific dashboards (protected)
import AdminDashboard from './pages/AdminDashboard';
import RecruiterDashboard from './pages/RecruiterDashboard';
import HiringDashboard from './pages/HiringDashboard';
import CandidateDashboard from './pages/CandidateDashboard';
import CandidateProfileCreate from './pages/CandidateProfileCreate';
import CandidateProfile from './pages/CandidateProfile';
import CandidateResumes from './pages/CandidateResumes';
import CandidateSkills from './pages/CandidateSkills';

// Admin sub-pages (protected, Admin only)
import UserManagement from './pages/UserManagement';
import OrganizationManagement from './pages/OrganizationManagement';
import AuditLogs from './pages/AuditLogs';
import SystemSettings from './pages/SystemSettings';

// Hiring Manager sub-pages (protected, HiringManager only)
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
                    {/* ── Default: redirect root to login ── */}
                    <Route path="/" element={<Navigate to="/login" replace />} />

                    {/* ── Public routes ── */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/forgot-password" element={<ForgotPassword />} />
                    <Route path="/reset-password" element={<ResetPassword />} />

                    {/* ── Admin routes ── */}
                    <Route
                        path="/admin-dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <AdminDashboard />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/user-management"
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <UserManagement />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/organization-management"
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <OrganizationManagement />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/audit-logs"
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <AuditLogs />
                            </ProtectedRoute>
                        }
                    />
                    <Route
                        path="/system-settings"
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <SystemSettings />
                            </ProtectedRoute>
                        }
                    />

                    {/* ── Recruiter routes ── */}
                    <Route
                        path="/recruiter-dashboard"
                        element={
                            <ProtectedRoute allowedRoles={['Recruiter']}>
                                <RecruiterDashboard />
                            </ProtectedRoute>
                        }
                    />

                    {/* ── Hiring Manager routes ── */}
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

                    {/* ── Candidate routes ── */}
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

                    {/* ── Utility routes ── */}
                    <Route path="/access-denied" element={<AccessDenied />} />
                    <Route path="/404" element={<NotFound />} />
                    <Route path="*" element={<Navigate to="/404" replace />} />
                </Routes>
            </Router>
        </AuthProvider>
    );
}

export default App;
