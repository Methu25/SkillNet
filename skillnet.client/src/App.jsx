import { useState } from 'react';
import AdminLayout from './components/AdminLayout';
import Dashboard from './pages/Dashboard';
import UserManagement from './pages/UserManagement';
import OrganizationManagement from './pages/OrganizationManagement';
import SystemSettings from './pages/SystemSettings';
import AuditLogs from './pages/AuditLogs';
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
                return <OrganizationManagement />;
            case 'configs':
                return <SystemSettings />;
            case 'logs':
                return <AuditLogs />;
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