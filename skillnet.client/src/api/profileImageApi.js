import { apiRequest, jsonRequest } from './apiClient';

export const profileImageApi = {
    upload: async (file) => {
        const formData = new FormData();
        formData.append('file', file);
        return (await apiRequest('/api/candidate/profile/image', {
            method: 'POST',
            body: formData
        })).data;
    },
    remove: async () =>
        (await jsonRequest('/api/candidate/profile/image', 'DELETE')).data
};

