import { useState } from 'react';
import AdminLayout from '../components/AdminLayout';
import Dashboard from './Dashboard';
import UserManagement from './UserManagement';
import OrganizationManagement from './OrganizationManagement';
import SystemSettings from './SystemSettings';
import AuditLogs from './AuditLogs';

const adminPages = {
    dashboard: Dashboard,
    users: UserManagement,
    organizations: OrganizationManagement,
    configs: SystemSettings,
    logs: AuditLogs
};

const AdminDashboard = () => {
    const [currentTab, setCurrentTab] = useState('dashboard');
    const CurrentPage = adminPages[currentTab] || Dashboard;

    return (
        <AdminLayout currentTab={currentTab} setCurrentTab={setCurrentTab}>
            <CurrentPage />
        </AdminLayout>
    );
};

export default AdminDashboard;
