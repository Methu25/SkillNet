import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';

import HiringDashboard from './pages/HiringDashboard';
import InterviewDetails from './pages/InterviewDetails';
import NotFound from './pages/NotFound';
import './App.css';

function App() {
    return (
        <Router>
            <Routes>
                <Route path="/" element={<Navigate to="/hiring-dashboard" replace />} />
                <Route path="/hiring-dashboard" element={<HiringDashboard />} />
                <Route path="/interviews/:id" element={<InterviewDetails />} />
                <Route path="/404" element={<NotFound />} />
                <Route path="*" element={<Navigate to="/404" replace />} />
            </Routes>
        </Router>
    );
}

export default App;