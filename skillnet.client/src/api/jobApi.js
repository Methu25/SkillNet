import { jsonRequest } from './apiClient';

const toQueryString = (filters = {}) => {
    const query = new URLSearchParams();
    Object.entries(filters).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== '') query.set(key, value);
    });
    const value = query.toString();
    return value ? `?${value}` : '';
};

export const jobApi = {
    search: async (filters) =>
        (await jsonRequest(`/api/job${toQueryString(filters)}`)).data,
    getById: async (jobId) =>
        (await jsonRequest(`/api/job/${jobId}`)).data
};
