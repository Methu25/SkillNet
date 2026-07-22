import { jsonRequest } from './apiClient';

export const interviewApi = {
    getEligibleInterviewers: async () => (await jsonRequest('/api/interviews/eligible-interviewers')).data,
    create: async (request) => (await jsonRequest('/api/interviews', 'POST', request)).data
};
