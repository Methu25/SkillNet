import { jsonRequest } from './apiClient';

export const applicationApi = {
    apply: async (application) =>
        (await jsonRequest('/api/application', 'POST', application)).data,
    getMine: async () =>
        (await jsonRequest('/api/application/my')).data,
    getMineById: async (applicationId) =>
        (await jsonRequest(`/api/application/my/${applicationId}`)).data,
    withdraw: async (applicationId, reason) =>
        (await jsonRequest(`/api/application/my/${applicationId}/withdraw`, 'PATCH', {
            reason: reason?.trim() || null
        })).data,
    getRecruiterJobs: async () =>
        (await jsonRequest('/api/application/recruiter/jobs')).data,
    getForJob: async (jobId, filters = {}) => {
        const query = new URLSearchParams();
        Object.entries(filters).forEach(([key, value]) => {
            if (value !== undefined && value !== null && value !== '') query.set(key, value);
        });
        const suffix = query.toString() ? `?${query}` : '';
        return (await jsonRequest(`/api/application/job/${jobId}${suffix}`)).data;
    },
    getRecruiterApplication: async (applicationId) =>
        (await jsonRequest(`/api/application/recruiter/${applicationId}`)).data
};
