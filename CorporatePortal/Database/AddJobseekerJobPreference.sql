/* Adds persistent Saved and Archived state for the My Jobs feature.
   Run against the CorporatePortal database after confirming the Jobseeker and Jobs keys. */
IF OBJECT_ID(N'dbo.JobseekerJobPreference', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.JobseekerJobPreference
    (
        PreferenceId INT IDENTITY(1,1) NOT NULL,
        JobseekerId INT NOT NULL,
        JobId INT NOT NULL,
        IsSaved BIT NOT NULL CONSTRAINT DF_JobseekerJobPreference_IsSaved DEFAULT (0),
        IsArchived BIT NOT NULL CONSTRAINT DF_JobseekerJobPreference_IsArchived DEFAULT (0),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_JobseekerJobPreference_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_JobseekerJobPreference_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT PK_JobseekerJobPreference PRIMARY KEY CLUSTERED (PreferenceId),
        CONSTRAINT UQ_JobseekerJobPreference_Jobseeker_Job UNIQUE (JobseekerId, JobId),
        CONSTRAINT FK_JobseekerJobPreference_Jobseeker FOREIGN KEY (JobseekerId)
            REFERENCES dbo.Jobseeker (JobseekerId) ON DELETE CASCADE,
        CONSTRAINT FK_JobseekerJobPreference_Jobs FOREIGN KEY (JobId)
            REFERENCES dbo.Jobs (JobId) ON DELETE CASCADE
    );
END;
GO
