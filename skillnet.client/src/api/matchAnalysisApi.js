import { jsonRequest } from './apiClient';

export const matchAnalysisApi = {
    forRecruiter: async (jobId, candidateId) => (await jsonRequest(`/api/match-analysis/jobs/${jobId}/candidates/${candidateId}`, 'POST')).data,
    forCandidate: async jobId => (await jsonRequest(`/api/match-analysis/jobs/${jobId}/me`, 'POST')).data
};
