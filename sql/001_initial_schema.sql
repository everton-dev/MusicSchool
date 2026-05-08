SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.Users
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    Role NVARCHAR(32) NOT NULL,
    PreferredCulture NVARCHAR(16) NOT NULL CONSTRAINT DF_Users_PreferredCulture DEFAULT N'en-US',
    CreatedOnUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Users_CreatedOnUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT CK_Users_Role CHECK (Role IN (N'Admin', N'Teacher', N'Student', N'Guardian'))
);
GO

CREATE UNIQUE INDEX UX_Users_Tenant_Email ON dbo.Users (TenantId, Email);
GO

CREATE TABLE dbo.Students
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    BirthDate DATE NULL,
    CONSTRAINT PK_Students PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Students_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

CREATE UNIQUE INDEX UX_Students_Tenant_User ON dbo.Students (TenantId, UserId);
GO

CREATE TABLE dbo.Teachers
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    CONSTRAINT PK_Teachers PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Teachers_Users_UserId FOREIGN KEY (UserId) REFERENCES dbo.Users (Id)
);
GO

CREATE UNIQUE INDEX UX_Teachers_Tenant_User ON dbo.Teachers (TenantId, UserId);
GO

CREATE TABLE dbo.Instruments
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_Instruments PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE UNIQUE INDEX UX_Instruments_Tenant_Name ON dbo.Instruments (TenantId, Name);
GO

CREATE TABLE dbo.TeacherInstruments
(
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_TeacherInstruments PRIMARY KEY CLUSTERED (TeacherId, InstrumentId),
    CONSTRAINT FK_TeacherInstruments_Teachers_TeacherId FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers (Id),
    CONSTRAINT FK_TeacherInstruments_Instruments_InstrumentId FOREIGN KEY (InstrumentId) REFERENCES dbo.Instruments (Id)
);
GO

CREATE INDEX IX_TeacherInstruments_InstrumentId ON dbo.TeacherInstruments (InstrumentId);
GO

CREATE TABLE dbo.FamilyGroups
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    DisplayName NVARCHAR(200) NOT NULL,
    CreatedOnUtc DATETIME2(7) NOT NULL CONSTRAINT DF_FamilyGroups_CreatedOnUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT PK_FamilyGroups PRIMARY KEY CLUSTERED (Id)
);
GO

CREATE INDEX IX_FamilyGroups_TenantId ON dbo.FamilyGroups (TenantId);
GO

CREATE TABLE dbo.FamilyRelationships
(
    Id UNIQUEIDENTIFIER NOT NULL,
    FamilyGroupId UNIQUEIDENTIFIER NOT NULL,
    GuardianUserId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    Kind NVARCHAR(32) NOT NULL,
    IsPrimaryPayer BIT NOT NULL CONSTRAINT DF_FamilyRelationships_IsPrimaryPayer DEFAULT 0,
    CONSTRAINT PK_FamilyRelationships PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_FamilyRelationships_FamilyGroups_FamilyGroupId FOREIGN KEY (FamilyGroupId) REFERENCES dbo.FamilyGroups (Id),
    CONSTRAINT FK_FamilyRelationships_Users_GuardianUserId FOREIGN KEY (GuardianUserId) REFERENCES dbo.Users (Id),
    CONSTRAINT FK_FamilyRelationships_Students_StudentId FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id),
    CONSTRAINT CK_FamilyRelationships_Kind CHECK (Kind IN (N'Parent', N'Guardian', N'Other'))
);
GO

CREATE UNIQUE INDEX UX_FamilyRelationships_Family_Guardian_Student
    ON dbo.FamilyRelationships (FamilyGroupId, GuardianUserId, StudentId);
GO

CREATE UNIQUE INDEX UX_FamilyRelationships_PrimaryPayer_PerStudent
    ON dbo.FamilyRelationships (FamilyGroupId, StudentId)
    WHERE IsPrimaryPayer = 1;
GO

CREATE TABLE dbo.Lessons
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    TeacherId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    CreatedOnUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Lessons_CreatedOnUtc DEFAULT SYSUTCDATETIME(),
    CancelledOnUtc DATETIME2(7) NULL,
    CancellationReason NVARCHAR(500) NULL,
    LastScheduleChangeOnUtc DATETIME2(7) NULL,
    CONSTRAINT PK_Lessons PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Lessons_Teachers_TeacherId FOREIGN KEY (TeacherId) REFERENCES dbo.Teachers (Id),
    CONSTRAINT FK_Lessons_Students_StudentId FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id),
    CONSTRAINT FK_Lessons_Instruments_InstrumentId FOREIGN KEY (InstrumentId) REFERENCES dbo.Instruments (Id),
    CONSTRAINT CK_Lessons_Status CHECK (Status IN (N'Scheduled', N'Paused', N'Cancelled'))
);
GO

CREATE INDEX IX_Lessons_Tenant_Teacher_Status ON dbo.Lessons (TenantId, TeacherId, Status);
CREATE INDEX IX_Lessons_Tenant_Student_Status ON dbo.Lessons (TenantId, StudentId, Status);
GO

CREATE TABLE dbo.Schedules
(
    LessonId UNIQUEIDENTIFIER NOT NULL,
    DayOfWeek TINYINT NOT NULL,
    StartTime TIME(0) NOT NULL,
    DurationMinutes SMALLINT NOT NULL,
    TimeZoneId NVARCHAR(100) NOT NULL,
    CONSTRAINT PK_Schedules PRIMARY KEY CLUSTERED (LessonId),
    CONSTRAINT FK_Schedules_Lessons_LessonId FOREIGN KEY (LessonId) REFERENCES dbo.Lessons (Id) ON DELETE CASCADE,
    CONSTRAINT CK_Schedules_DayOfWeek CHECK (DayOfWeek BETWEEN 0 AND 6),
    CONSTRAINT CK_Schedules_DurationMinutes CHECK (DurationMinutes BETWEEN 15 AND 180)
);
GO

CREATE TABLE dbo.CurriculumNodes
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    InstrumentId UNIQUEIDENTIFIER NOT NULL,
    ParentNodeId UNIQUEIDENTIFIER NULL,
    Title NVARCHAR(200) NOT NULL,
    Type NVARCHAR(32) NOT NULL,
    SortOrder INT NOT NULL,
    BlobName NVARCHAR(512) NULL,
    ResourceFileType NVARCHAR(32) NULL,
    ResourceFileName NVARCHAR(255) NULL,
    ResourceContentType NVARCHAR(100) NULL,
    ResourceUploadedByTeacherId UNIQUEIDENTIFIER NULL,
    ResourceUploadedOnUtc DATETIME2(7) NULL,
    CONSTRAINT PK_CurriculumNodes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_CurriculumNodes_Instruments_InstrumentId FOREIGN KEY (InstrumentId) REFERENCES dbo.Instruments (Id),
    CONSTRAINT FK_CurriculumNodes_CurriculumNodes_ParentNodeId FOREIGN KEY (ParentNodeId) REFERENCES dbo.CurriculumNodes (Id),
    CONSTRAINT FK_CurriculumNodes_Teachers_ResourceUploadedByTeacherId FOREIGN KEY (ResourceUploadedByTeacherId) REFERENCES dbo.Teachers (Id),
    CONSTRAINT CK_CurriculumNodes_Type CHECK (Type IN (N'Module', N'Lesson', N'Exercise', N'Resource')),
    CONSTRAINT CK_CurriculumNodes_ResourceFileType CHECK (ResourceFileType IS NULL OR ResourceFileType IN (N'Pdf', N'Mp3', N'Other'))
);
GO

CREATE INDEX IX_CurriculumNodes_Tenant_Instrument_Parent_Sort
    ON dbo.CurriculumNodes (TenantId, InstrumentId, ParentNodeId, SortOrder);
GO

CREATE TABLE dbo.StudentCurriculumProgress
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    CurriculumNodeId UNIQUEIDENTIFIER NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    UpdatedOnUtc DATETIME2(7) NOT NULL CONSTRAINT DF_StudentCurriculumProgress_UpdatedOnUtc DEFAULT SYSUTCDATETIME(),
    CompletedOnUtc DATETIME2(7) NULL,
    CONSTRAINT PK_StudentCurriculumProgress PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_StudentCurriculumProgress_Students_StudentId FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id),
    CONSTRAINT FK_StudentCurriculumProgress_CurriculumNodes_CurriculumNodeId FOREIGN KEY (CurriculumNodeId) REFERENCES dbo.CurriculumNodes (Id),
    CONSTRAINT CK_StudentCurriculumProgress_Status CHECK (Status IN (N'NotStarted', N'InProgress', N'Completed'))
);
GO

CREATE UNIQUE INDEX UX_StudentCurriculumProgress_Tenant_Student_Node
    ON dbo.StudentCurriculumProgress (TenantId, StudentId, CurriculumNodeId);
GO

CREATE TABLE dbo.Payments
(
    Id UNIQUEIDENTIFIER NOT NULL,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    StudentId UNIQUEIDENTIFIER NOT NULL,
    GuardianUserId UNIQUEIDENTIFIER NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Currency NVARCHAR(3) NOT NULL CONSTRAINT DF_Payments_Currency DEFAULT N'EUR',
    Method NVARCHAR(32) NOT NULL,
    Status NVARCHAR(32) NOT NULL,
    DueDate DATE NOT NULL,
    Description NVARCHAR(300) NOT NULL,
    PaymentReference NVARCHAR(100) NULL,
    CreatedOnUtc DATETIME2(7) NOT NULL CONSTRAINT DF_Payments_CreatedOnUtc DEFAULT SYSUTCDATETIME(),
    ConfirmedOnUtc DATETIME2(7) NULL,
    RejectedOnUtc DATETIME2(7) NULL,
    RejectionReason NVARCHAR(500) NULL,
    CancelledOnUtc DATETIME2(7) NULL,
    CONSTRAINT PK_Payments PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Payments_Students_StudentId FOREIGN KEY (StudentId) REFERENCES dbo.Students (Id),
    CONSTRAINT FK_Payments_Users_GuardianUserId FOREIGN KEY (GuardianUserId) REFERENCES dbo.Users (Id),
    CONSTRAINT CK_Payments_Amount CHECK (Amount > 0),
    CONSTRAINT CK_Payments_Currency CHECK (Currency = N'EUR'),
    CONSTRAINT CK_Payments_Method CHECK (Method IN (N'MbWay', N'BankTransfer')),
    CONSTRAINT CK_Payments_Status CHECK (Status IN (N'Pending', N'Confirmed', N'Rejected', N'Cancelled'))
);
GO

CREATE INDEX IX_Payments_Tenant_Guardian_Status
    ON dbo.Payments (TenantId, GuardianUserId, Status);
GO

CREATE INDEX IX_Payments_Tenant_Student_DueDate
    ON dbo.Payments (TenantId, StudentId, DueDate);
GO
