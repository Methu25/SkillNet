import { apiRequest, jsonRequest } from './apiClient';

const fileForm = (file) => {
    const formData = new FormData();
    formData.append('file', file);
    return formData;
};

const getDownloadFileName = (response, fallback) => {
    const disposition = response.headers.get('content-disposition') || '';
    const encodedMatch = disposition.match(/filename\*=UTF-8''([^;]+)/i);
    if (encodedMatch) return decodeURIComponent(encodedMatch[1]);

    const plainMatch = disposition.match(/filename="?([^";]+)"?/i);
    return plainMatch?.[1] || fallback;
};

export const resumeApi = {
    getAll: async () => (await jsonRequest('/api/candidate/resumes')).data,
    getActive: async () => (await jsonRequest('/api/candidate/resumes/active')).data,
    upload: async (file) => (await apiRequest('/api/candidate/resumes', {
        method: 'POST',
        body: fileForm(file)
    })).data,
    replace: async (resumeId, file) => (await apiRequest(
        `/api/candidate/resumes/${resumeId}/replace`,
        { method: 'PUT', body: fileForm(file) }
    )).data,
    setActive: async (resumeId) =>
        (await jsonRequest(`/api/candidate/resumes/${resumeId}/set-active`, 'PUT')).data,
    remove: async (resumeId) =>
        (await jsonRequest(`/api/candidate/resumes/${resumeId}`, 'DELETE')).data,
    download: async (resumeId, fallbackFileName = 'resume.pdf') => {
        const { data, response } = await apiRequest(
            `/api/candidate/resumes/${resumeId}/download`,
            { responseType: 'blob' }
        );
        return { blob: data, fileName: getDownloadFileName(response, fallbackFileName) };
    }
};

