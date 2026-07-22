/**
 * @typedef {Object} ProfileCompletion
 * @property {number} completionPercentage
 * @property {number} completionLevel
 * @property {boolean} isComplete
 * @property {string[]} completedSections
 * @property {string[]} missingSections
 */

/**
 * @typedef {Object} Resume
 * @property {number} resumeId
 * @property {number} candidateId
 * @property {string} fileName
 * @property {string} filePath
 * @property {string} fileType
 * @property {number} fileSize
 * @property {string} uploadedDate
 * @property {boolean} isActive
 */

/**
 * @typedef {Object} CandidateSkill
 * @property {number} candidateId
 * @property {number} skillId
 * @property {string} skillName
 */

/**
 * @typedef {Object} Skill
 * @property {number} skillId
 * @property {string} skillName
 */

/**
 * @typedef {Object} CandidateProfile
 * @property {number} userId
 * @property {string} firstName
 * @property {string} lastName
 * @property {?string} phoneNumber
 * @property {?string} location
 * @property {?string} professionalTitle
 * @property {?string} professionalSummary
 * @property {?string} education
 * @property {?string} degree
 * @property {?string} university
 * @property {?number} experienceYears
 * @property {?string} profileImagePath
 * @property {string} createdDate
 * @property {string} updatedDate
 * @property {boolean} isProfileCompleted
 * @property {?Resume} activeResume
 * @property {CandidateSkill[]} skills
 * @property {ProfileCompletion} profileCompletion
 */

/**
 * @typedef {Object} CandidateDashboard
 * @property {boolean} hasProfile
 * @property {boolean} isFirstTimeUser
 * @property {?string} welcomeMessage
 * @property {Object} profile
 * @property {ProfileCompletion} profileCompletion
 * @property {number} totalResumes
 * @property {boolean} hasActiveResume
 * @property {?Resume} activeResume
 * @property {?Resume} latestResume
 * @property {number} totalSkills
 * @property {CandidateSkill[]} skills
 * @property {number} totalApplications
 * @property {number} activeApplications
 * @property {number} appliedApplications
 * @property {number} shortlistedApplications
 * @property {number} interviewScheduledApplications
 * @property {number} acceptedApplications
 * @property {number} rejectedApplications
 * @property {number} upcomingInterviews
 * @property {Object[]} recommendedJobs
 * @property {Object[]} interviews
 */

/**
 * @typedef {import('../api/apiClient').ApiError} ApiError
 */

export {};

