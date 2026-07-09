import React, { useState } from 'react';
import AdminLayout from './components/AdminLayout';
import Dashboard from './pages/Dashboard';
import './App.css';

function App() {
    const [currentTab, setCurrentTab] = useState('dashboard');

    // This function switches the screen based on what you click in the sidebar
    const renderContent = () => {
        switch (currentTab) {
            case 'dashboard':
                return <Dashboard />;
            case 'users':
                return <div><h2>User Management</h2><p>Screen coming next...</p></div>;
            case 'organizations':
                return <div><h2>Organizations & Departments</h2><p>Screen coming soon...</p></div>;
            case 'configs':
                return <div><h2>System Settings</h2><p>Screen coming soon...</p></div>;
            case 'logs':
                return <div><h2>Audit Logs</h2><p>Screen coming soon...</p></div>;
            default:
                return <Dashboard />;
        }
    };

    return (
        <AdminLayout currentTab={currentTab} setCurrentTab={setCurrentTab}>
            {renderContent()}
        </AdminLayout>
    );
}

export default App;