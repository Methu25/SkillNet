import React, { useState } from 'react';
import AdminLayout from './components/AdminLayout';
import Dashboard from './pages/Dashboard';
import UserManagement from './pages/UserManagement';
import OrganizationManagement from './pages/OrganizationManagement';
import './App.css';

function App() {
    const [currentTab, setCurrentTab] = useState('dashboard');

    // This function switches the screen based on what you click in the sidebar
    const renderContent = () => {
        switch (currentTab) {
            case 'dashboard':
                return <Dashboard />;
            case 'users':
                return <UserManagement />;
            case 'organizations':
                return <div><h2>Organizations & Departments</h2><p>Screen coming soon...</p></div>;
            case 'configs':
                return <div><h2>System Settings</h2><p>Screen coming soon...</p></div>;
            case 'logs':
                return <div><h2>Audit Logs</h2><p>Screen coming soon...</p></div>;
            default:
                return <Dashboard />;
            case 'organizations':
                return <OrganizationManagement />;
        }
    };

    return (
        <AdminLayout currentTab={currentTab} setCurrentTab={setCurrentTab}>
            {renderContent()}
        </AdminLayout>
    );
}

export default App;