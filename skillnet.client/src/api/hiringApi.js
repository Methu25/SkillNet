import { jsonRequest } from './apiClient';

export const hiringApi = {
    getAssignedInterviews: async () => (await jsonRequest('/api/hiring/interviews')).data,
    getAssignedInterview: async (interviewId) => (await jsonRequest(`/api/hiring/interviews/${interviewId}`)).data,
    getEvaluation: async (interviewId) => (await jsonRequest(`/api/interviews/${interviewId}/evaluation`)).data,
    submitEvaluation: async (interviewId, evaluation) => (await jsonRequest(`/api/interviews/${interviewId}/evaluation`, 'POST', evaluation)).data,
    recordDecision: async (interviewId, decision) => (await jsonRequest(`/api/interviews/${interviewId}/decision`, 'PATCH', { decision })).data
};
