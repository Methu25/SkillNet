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
        })).data
};
