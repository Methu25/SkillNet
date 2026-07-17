import { jsonRequest } from './apiClient';

export const skillApi = {
    getCandidateSkills: async () =>
        (await jsonRequest('/api/candidate/skills')).data,
    getAvailableSkills: async () =>
        (await jsonRequest('/api/candidate/skills/available')).data,
    addSkill: async (skillId) =>
        (await jsonRequest('/api/candidate/skills', 'POST', { skillId })).data,
    removeSkill: async (skillId) =>
        (await jsonRequest(`/api/candidate/skills/${skillId}`, 'DELETE')).data
};

