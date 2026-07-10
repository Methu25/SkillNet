<<<<<<< Updated upstream
import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

// Import Pages
import Login from './pages/Login';
import Register from './pages/Register';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';
import AdminDashboard from './pages/AdminDashboard';
import RecruiterDashboard from './pages/RecruiterDashboard';
import HiringDashboard from './pages/HiringDashboard';
import CandidateDashboard from './pages/CandidateDashboard';
import AccessDenied from './pages/AccessDenied';
import NotFound from './pages/NotFound';

import './App.css';

const HomeRedirect = () => {
    const { user, loading } = useAuth();

    if (loading) {
        return <div style={{ textAlign: 'center', marginTop: '100px' }}>Loading...</div>;
    }

    if (!user) {
        return <Navigate to="/login" replace />;
    }

    const roles = user.roles || [];
    if (roles.includes('Admin')) return <Navigate to="/admin-dashboard" replace />;
    if (roles.includes('Recruiter')) return <Navigate to="/recruiter-dashboard" replace />;
    if (roles.includes('HiringManager')) return <Navigate to="/hiring-dashboard" replace />;
    if (roles.includes('Candidate')) return <Navigate to="/candidate-dashboard" replace />;

    return <Navigate to="/access-denied" replace />;
};

function App() {
    return (
        <AuthProvider>
            <Router>
                <Routes>
                    {/* Home Route Redirect */}
                    <Route path="/" element={<HomeRedirect />} />

                    {/* Public Auth Routes */}
                    <Route path="/login" element={<Login />} />
                    <Route path="/register" element={<Register />} />
                    <Route path="/forgot-password" element={<ForgotPassword />} />
                    <Route path="/reset-password" element={<ResetPassword />} />

                    {/* Protected Dashboard Routes */}
                    <Route 
                        path="/admin-dashboard" 
                        element={
                            <ProtectedRoute allowedRoles={['Admin']}>
                                <AdminDashboard />
                            </ProtectedRoute>
                        } 
                    />
                    <Route 
                        path="/recruiter-dashboard" 
                        element={
                            <ProtectedRoute allowedRoles={['Recruiter']}>
                                <RecruiterDashboard />
                            </ProtectedRoute>
                        } 
                    />
                    <Route 
                        path="/hiring-dashboard" 
                        element={
                            <ProtectedRoute allowedRoles={['HiringManager']}>
                                <HiringDashboard />
                            </ProtectedRoute>
                        } 
                    />
                    <Route 
                        path="/candidate-dashboard" 
                        element={
                            <ProtectedRoute allowedRoles={['Candidate']}>
                                <CandidateDashboard />
                            </ProtectedRoute>
                        } 
                    />

                    {/* Error Routes */}
                    <Route path="/access-denied" element={<AccessDenied />} />
                    <Route path="/unauthorized" element={<AccessDenied />} />
                    <Route path="/404" element={<NotFound />} />
                    <Route path="*" element={<Navigate to="/404" replace />} />
                </Routes>
            </Router>
        </AuthProvider>
=======
import ScheduleInterview from './pages/ScheduleInterview';

function App() {
    return (
        <div>
            <ScheduleInterview />
        </div>
>>>>>>> Stashed changes
    );
}

export default App;