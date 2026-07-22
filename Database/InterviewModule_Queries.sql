-- Module 5: Interview & Hiring Manager Module
-- Interview Module Queries

-- 1. Get all interviews
SELECT *
FROM Interview;

-- 2. Get interview by ID
SELECT *
FROM Interview
WHERE InterviewId = @InterviewId;

-- 3. Get upcoming interviews
SELECT *
FROM Interview
WHERE ScheduledDate >= GETDATE()
ORDER BY ScheduledDate ASC;

-- 4. Get today's interviews
SELECT *
FROM Interview
WHERE CAST(ScheduledDate AS DATE) = CAST(GETDATE() AS DATE);

-- 5. Schedule or reschedule interview
UPDATE Interview
SET 
    ScheduledDate = @ScheduledDate,
    Duration = @Duration,
    Location = @Location,
    MeetingLink = @MeetingLink,
    Status = @Status
WHERE InterviewId = @InterviewId;

-- 6. Cancel interview
UPDATE Interview
SET Status = 'Cancelled'
WHERE InterviewId = @InterviewId;

-- 7. Insert interview evaluation
INSERT INTO InterviewEvaluation
(
    InterviewId,
    InterviewerId,
    TechnicalScore,
    CommunicationScore,
    ProblemSolvingScore,
    CultureFitScore,
    OverallScore,
    Recommendation,
    Comments,
    SubmittedAt
)
VALUES
(
    @InterviewId,
    @InterviewerId,
    @TechnicalScore,
    @CommunicationScore,
    @ProblemSolvingScore,
    @CultureFitScore,
    @OverallScore,
    @Recommendation,
    @Comments,
    GETDATE()
);

-- 8. Get evaluation by interview ID
SELECT *
FROM InterviewEvaluation
WHERE InterviewId = @InterviewId;

-- 9. Update evaluation
UPDATE InterviewEvaluation
SET
    InterviewerId = @InterviewerId,
    TechnicalScore = @TechnicalScore,
    CommunicationScore = @CommunicationScore,
    ProblemSolvingScore = @ProblemSolvingScore,
    CultureFitScore = @CultureFitScore,
    OverallScore = @OverallScore,
    Recommendation = @Recommendation,
    Comments = @Comments,
    SubmittedAt = GETDATE()
WHERE InterviewId = @InterviewId;

-- 10. Hiring manager dashboard counts
SELECT
    COUNT(*) AS TotalInterviews,
    SUM(CASE WHEN CAST(ScheduledDate AS DATE) = CAST(GETDATE() AS DATE) THEN 1 ELSE 0 END) AS TodaysInterviews,
    SUM(CASE WHEN ScheduledDate >= GETDATE() THEN 1 ELSE 0 END) AS UpcomingInterviews,
    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedInterviews,
    SUM(CASE WHEN Status = 'Pending Feedback' THEN 1 ELSE 0 END) AS PendingFeedback
FROM Interview;