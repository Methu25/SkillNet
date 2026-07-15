import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';

import HiringDashboard from './pages/HiringDashboard';
import InterviewDetails from './pages/InterviewDetails';

import Login from './pages/Login';
import Register from './pages/Register';
import ForgotPassword from './pages/ForgotPassword';
import ResetPassword from './pages/ResetPassword';
import NotFound from './pages/NotFound';

function App() {
    return (
        <BrowserRouter>
            <Routes>
                {/* Default page */}
                <Route path="/" element={<Navigate to="/hiring-dashboard" replace />} />

                {/* Interview / Hiring Manager Module */}
                <Route path="/hiring-dashboard" element={<HiringDashboard />} />
                <Route path="/interviews/:id" element={<InterviewDetails />} />

                {/* Auth pages keep for team, but not default */}
                <Route path="/login" element={<Login />} />
                <Route path="/register" element={<Register />} />
                <Route path="/forgot-password" element={<ForgotPassword />} />
                <Route path="/reset-password" element={<ResetPassword />} />

                {/* Fallback */}
                <Route path="*" element={<NotFound />} />
            </Routes>
        </BrowserRouter>
    );
}

export default App;