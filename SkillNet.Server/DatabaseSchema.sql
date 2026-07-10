-- Database Schema for SkillNet Interview Module

CREATE TABLE Interview (
	InterviewId INT IDENTITY(1,1) PRIMARY KEY,
	ApplicationId INT NOT NULL,
	InterviewType NVARCHAR(100),
	InterviewRound INT NOT NULL,
	ScheduledDate DATETIME NOT NULL,
	Duration INT NOT NULL,
	Location NVARCHAR(255),
	MeetingLink NVARCHAR(MAX),
	Status NVARCHAR(50),
	CreatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE Interviewer (
	InterviewerId INT IDENTITY(1,1) PRIMARY KEY,
	UserId INT NOT NULL,
	DepartmentId INT,
	Position NVARCHAR(100)
);

CREATE TABLE InterviewAssignment (
	InterviewId INT NOT NULL,
	InterviewerId INT NOT NULL,
	Role NVARCHAR(50),
	PRIMARY KEY (InterviewId, InterviewerId),
	FOREIGN KEY (InterviewId) REFERENCES Interview(InterviewId),
	FOREIGN KEY (InterviewerId) REFERENCES Interviewer(InterviewerId)
);

CREATE TABLE InterviewEvaluation (
	EvaluationId INT IDENTITY(1,1) PRIMARY KEY,
	InterviewId INT NOT NULL,
	InterviewerId INT NOT NULL,
	TechnicalScore INT NOT NULL,
	CommunicationScore INT NOT NULL,
	ProblemSolvingScore INT NOT NULL,
	CultureFitScore INT NOT NULL,
	OverallScore INT NOT NULL,
	Recommendation NVARCHAR(50),
	Comments NVARCHAR(MAX),
	SubmittedAt DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (InterviewId) REFERENCES Interview(InterviewId),
	FOREIGN KEY (InterviewerId) REFERENCES Interviewer(InterviewerId)
);

CREATE TABLE InterviewFeedbackHistory (
	HistoryId INT IDENTITY(1,1) PRIMARY KEY,
	EvaluationId INT NOT NULL,
	UpdatedBy INT NOT NULL,
	OldValue NVARCHAR(MAX),
	NewValue NVARCHAR(MAX),
	UpdatedAt DATETIME DEFAULT GETDATE(),
	FOREIGN KEY (EvaluationId) REFERENCES InterviewEvaluation(EvaluationId)
);
