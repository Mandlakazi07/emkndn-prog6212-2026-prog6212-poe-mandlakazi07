/* ================================================================================
   Student:          Mandlakazi Sibiya
   Student Number:   ST10473547
   Module:           PROG6212 - Programming 2B
   Assessment:       Portfolio of Evidence (PoE) - Part 1, Section C
   Institution:      The Independent Institute of Education (IIE)
   Date:             20 September 2026

   Reference (IIE Harvard Anglia style):
   Sibiya, M., 2026. RaceDay database schema [SQL script]. Durban:
   The Independent Institute of Education (IIE).
   ================================================================================ */

/* =====================================================================
   RaceDay Database Schema
   PROG6212 PoE Part 1 - Section C
   Matches the ERD (v1.2) exactly: Users, Venues, Events, Categories,
   Enrollments, Results
   Run this on a clean SQL Server instance via SSMS.
   ===================================================================== */

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'RaceDayDB')
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO

-- Drop tables if re-running the script (children first, to respect FKs)
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrollments', 'U') IS NOT NULL DROP TABLE dbo.Enrollments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Venues', 'U') IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* ---------------------------------------------------------------------
   USERS - both Organizers and Participants (role-based, per POE spec)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Users (
    UserID          INT IDENTITY(1,1) PRIMARY KEY,
    FullName        NVARCHAR(100)   NOT NULL,
    Email           NVARCHAR(150)   NOT NULL UNIQUE,
    PasswordHash    NVARCHAR(255)   NOT NULL,
    Role            NVARCHAR(20)    NOT NULL
                    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organizer','Participant')),
    CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE()
);
GO

/* ---------------------------------------------------------------------
   VENUES - normalized out of Events so a venue can host many events
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Venues (
    VenueID         INT IDENTITY(1,1) PRIMARY KEY,
    Name            NVARCHAR(150)   NOT NULL,
    Address         NVARCHAR(255)   NOT NULL,
    City            NVARCHAR(100)   NOT NULL
);
GO

/* ---------------------------------------------------------------------
   EVENTS - one Organizer (Users), one Venue
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Events (
    EventID         INT IDENTITY(1,1) PRIMARY KEY,
    OrganizerID     INT             NOT NULL,
    VenueID         INT             NOT NULL,
    Name            NVARCHAR(150)   NOT NULL,
    Description     NVARCHAR(MAX)   NULL,
    EventDate       DATE            NOT NULL,
    Distance        DECIMAL(6,2)    NOT NULL,
    EventType       NVARCHAR(20)    NOT NULL
                    CONSTRAINT CK_Events_Type CHECK (EventType IN ('Run','Walk','Cycle')),
    CONSTRAINT FK_Events_Organizer FOREIGN KEY (OrganizerID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Events_Venue     FOREIGN KEY (VenueID)     REFERENCES dbo.Venues(VenueID)
);
GO

/* ---------------------------------------------------------------------
   CATEGORIES - one Event has many Categories (age/distance bands)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Categories (
    CategoryID      INT IDENTITY(1,1) PRIMARY KEY,
    EventID         INT             NOT NULL,
    CategoryName    NVARCHAR(50)    NOT NULL,
    CONSTRAINT FK_Categories_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID)
);
GO

/* ---------------------------------------------------------------------
   ENROLLMENTS - links a Participant (Users) to an Event + Category
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Enrollments (
    EnrollmentID    INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID   INT             NOT NULL,
    EventID         INT             NOT NULL,
    CategoryID      INT             NOT NULL,
    EnrollmentDate  DATETIME        NOT NULL DEFAULT GETDATE(),
    Status          NVARCHAR(20)    NOT NULL DEFAULT 'Pending'
                    CONSTRAINT CK_Enrollments_Status CHECK (Status IN ('Pending','Confirmed','Cancelled')),
    CONSTRAINT FK_Enrollments_Participant FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Enrollments_Event       FOREIGN KEY (EventID)       REFERENCES dbo.Events(EventID),
    CONSTRAINT FK_Enrollments_Category    FOREIGN KEY (CategoryID)    REFERENCES dbo.Categories(CategoryID)
);
GO

/* ---------------------------------------------------------------------
   RESULTS - one Enrollment has at most one Result (1..1 : 1..1)
   --------------------------------------------------------------------- */
CREATE TABLE dbo.Results (
    ResultID        INT IDENTITY(1,1) PRIMARY KEY,
    EnrollmentID    INT             NOT NULL UNIQUE,
    FinishTime      TIME            NULL,
    FinishPosition  INT             NULL,
    CONSTRAINT FK_Results_Enrollment FOREIGN KEY (EnrollmentID) REFERENCES dbo.Enrollments(EnrollmentID)
);
GO

/* =====================================================================
   SEED DATA
   2 organizers, 2 participants, 3 events, categories per event,
   sample enrollments + results
   ===================================================================== */

INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role) VALUES
('Thabo Mokoena',   'thabo.organizer@raceday.co.za',    'hashed_pw_placeholder_1', 'Organizer'),
('Lindiwe Nkosi',   'lindiwe.organizer@raceday.co.za',  'hashed_pw_placeholder_2', 'Organizer'),
('Sipho Dlamini',   'sipho.participant@raceday.co.za',  'hashed_pw_placeholder_3', 'Participant'),
('Ayesha Patel',    'ayesha.participant@raceday.co.za', 'hashed_pw_placeholder_4', 'Participant');
GO

INSERT INTO dbo.Venues (Name, Address, City) VALUES
('Kings Park Stadium',    '2 Jacko Jackson Drive',  'Durban'),
('Kirstenbosch Gardens',  'Rhodes Drive, Newlands', 'Cape Town'),
('Zwartkops Raceway',     'Kudu Avenue, Centurion', 'Pretoria');
GO

INSERT INTO dbo.Events (OrganizerID, VenueID, Name, Description, EventDate, Distance, EventType) VALUES
(1, 1, 'Durban Beachfront Fun Run',  'A scenic beachfront run for all fitness levels.',        '2026-11-14', 10.00, 'Run'),
(1, 2, 'Cape Town Cycle Classic',    'Annual cycling event through the Cape winelands.',       '2026-12-05', 42.00, 'Cycle'),
(2, 3, 'Pretoria Community Walk',   'A charity walk supporting local schools.',                '2026-10-25', 5.00,  'Walk');
GO

INSERT INTO dbo.Categories (EventID, CategoryName) VALUES
(1, 'Under 20'),
(1, 'Senior'),
(2, '42km'),
(2, '21km'),
(3, '5km Family');
GO

INSERT INTO dbo.Enrollments (ParticipantID, EventID, CategoryID, Status) VALUES
(3, 1, 1, 'Confirmed'),
(4, 1, 2, 'Pending'),
(3, 2, 3, 'Confirmed'),
(4, 3, 5, 'Confirmed');
GO

INSERT INTO dbo.Results (EnrollmentID, FinishTime, FinishPosition) VALUES
(1, '00:45:12', 12),
(3, '01:32:47', 47);
GO

/* Quick sanity check after running - should return 6, 4, 3, 5, 4, 2 */
SELECT
    (SELECT COUNT(*) FROM dbo.Users)        AS UserCount,
    (SELECT COUNT(*) FROM dbo.Venues)       AS VenueCount,
    (SELECT COUNT(*) FROM dbo.Events)       AS EventCount,
    (SELECT COUNT(*) FROM dbo.Categories)   AS CategoryCount,
    (SELECT COUNT(*) FROM dbo.Enrollments)  AS EnrollmentCount,
    (SELECT COUNT(*) FROM dbo.Results)      AS ResultCount;


/* NOTE FOR VIDEO RECORDING:
   The rubric requires the video to show this script executing live in SSMS,
   with the RaceDayDB database and its tables visible and expanded in
   Object Explorer afterwards (right-click Databases > Refresh if it
   doesn't appear automatically). Don't just show the query results grid. */


/* ================================================================================
   READABLE VERIFICATION QUERIES
   These join across tables to show human-readable output (names instead of raw
   FK numbers) - useful for the video demo to prove the seeded data is correctly
   linked, not just present.
   ================================================================================ */

-- All events with venue and organizer name
SELECT
    e.EventID,
    e.Name           AS EventName,
    e.EventDate,
    v.Name            AS VenueName,
    v.City,
    e.EventType,
    u.FullName        AS OrganizerName
FROM dbo.Events e
JOIN dbo.Venues v ON e.VenueID = v.VenueID
JOIN dbo.Users u  ON e.OrganizerID = u.UserID;

-- All enrollments with participant, event and category name
SELECT
    en.EnrollmentID,
    p.FullName        AS ParticipantName,
    e.Name             AS EventName,
    c.CategoryName,
    en.Status,
    en.EnrollmentDate
FROM dbo.Enrollments en
JOIN dbo.Users p      ON en.ParticipantID = p.UserID
JOIN dbo.Events e     ON en.EventID = e.EventID
JOIN dbo.Categories c ON en.CategoryID = c.CategoryID;

-- All results with participant/event/category name and a finishing summary
SELECT
    r.ResultID,
    p.FullName        AS ParticipantName,
    e.Name             AS EventName,
    c.CategoryName,
    r.FinishTime,
    r.FinishPosition,
    CAST(r.FinishPosition AS VARCHAR)
        + ' out of '
        + CAST((SELECT COUNT(*) FROM dbo.Enrollments en2 WHERE en2.EventID = e.EventID) AS VARCHAR)
        AS FinishingSummary
FROM dbo.Results r
JOIN dbo.Enrollments en ON r.EnrollmentID = en.EnrollmentID
JOIN dbo.Users p        ON en.ParticipantID = p.UserID
JOIN dbo.Events e       ON en.EventID = e.EventID
JOIN dbo.Categories c   ON en.CategoryID = c.CategoryID;

-- Total enrollments per event
SELECT
    e.Name             AS EventName,
    COUNT(en.EnrollmentID) AS TotalEnrollments
FROM dbo.Events e
LEFT JOIN dbo.Enrollments en ON e.EventID = en.EventID
GROUP BY e.Name;
