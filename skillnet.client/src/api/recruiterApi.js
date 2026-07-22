import { apiRequest, jsonRequest } from './apiClient';

export const recruiterApi = {
    getProfile: async () => (await jsonRequest('/api/recruiter/profile')).data,
    updateProfile: async (profile) => (await jsonRequest('/api/recruiter/profile', 'POST', profile)).data,
    getOrganization: async () => (await jsonRequest('/api/recruiter/organization')).data,
    updateOrganization: async (organization) =>
        (await jsonRequest('/api/recruiter/organization', 'POST', organization)).data,
    uploadOrganizationLogo: async (file) => {
        const formData = new FormData();
        formData.append('file', file);
        return (await apiRequest('/api/recruiter/organization/logo', {
            method: 'POST',
            body: formData
        })).data;
    },
    deleteOrganizationLogo: async () =>
        (await jsonRequest('/api/recruiter/organization/logo', 'DELETE')).data,
    getDashboard: async () => (await jsonRequest('/api/recruiter/jobs')).data,
    getSkills: async () => (await jsonRequest('/api/job/skills')).data,
    getCategories: async () => (await jsonRequest('/api/job/categories')).data,
    getJob: async (jobId) => (await jsonRequest(`/api/job/${jobId}`)).data,
    createJob: async (job) => (await jsonRequest('/api/job', 'POST', job)).data,
    updateJob: async (jobId, job) => (await jsonRequest(`/api/job/${jobId}`, 'PUT', job)).data,
    deleteJob: async (jobId) => (await jsonRequest(`/api/job/${jobId}`, 'DELETE')).data,
    publishJob: async (jobId) => (await jsonRequest(`/api/job/${jobId}/publish`, 'PATCH')).data,
    closeJob: async (jobId) => (await jsonRequest(`/api/job/${jobId}/close`, 'PATCH')).data,
    duplicateJob: async (jobId) => (await jsonRequest(`/api/job/${jobId}/duplicate`, 'POST')).data
};
