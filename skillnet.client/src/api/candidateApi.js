import { jsonRequest } from './apiClient';

export const candidateApi = {
    getProfile: async () => (await jsonRequest('/api/candidate/profile')).data,
    profileExists: async () => (await jsonRequest('/api/candidate/profile/exists')).data,
    createProfile: async (profile) =>
        (await jsonRequest('/api/candidate/profile', 'POST', profile)).data,
    updateProfile: async (profile) =>
        (await jsonRequest('/api/candidate/profile', 'PUT', profile)).data,
    deleteProfile: async () =>
        (await jsonRequest('/api/candidate/profile', 'DELETE')).data,
    getProfileCompletion: async () =>
        (await jsonRequest('/api/candidate/profile/completion')).data,
    getDashboard: async () =>
        (await jsonRequest('/api/candidate/dashboard')).data
};

