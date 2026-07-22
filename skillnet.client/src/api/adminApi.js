import { jsonRequest } from './apiClient';

export const adminApi = {
    getDashboard: async () => (await jsonRequest('/api/dashboard/statistics')).data,
    getUsers: async () => (await jsonRequest('/api/user')).data,
    getRoles: async () => (await jsonRequest('/api/userrole')).data,
    getOrganizations: async () => (await jsonRequest('/api/organization')).data,
    getDepartments: async () => (await jsonRequest('/api/department')).data,
    getPendingOrganizations: async () => {
        try {
            return (await jsonRequest('/api/organization-approval/pending')).data;
        } catch {
            return [];
        }
    },
    approveOrganization: async id => (await jsonRequest(`/api/organization-approval/${id}/approve`, 'PATCH')).data,
    rejectOrganization: async (id, reason) => (await jsonRequest(`/api/organization-approval/${id}/reject`, 'PATCH', { reason })).data,
    getSettings: async () => (await jsonRequest('/api/systemconfiguration')).data,
    updateSettings: async settings => (await jsonRequest('/api/systemconfiguration', 'PUT', settings)).data,
    getAuditLogs: async query => (await jsonRequest(`/api/auditlog?${query}`)).data
};
