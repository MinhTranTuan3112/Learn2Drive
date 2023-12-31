use master;
go
if exists (select name from sys.databases where name = 'DrivingLicense')
begin
	drop database DrivingLicense;
end;
go
create database DrivingLicense;
go
use DrivingLicense;

create table Account
(
   AccountID uniqueidentifier default newid() primary key,
   Username nvarchar(100) unique,
   [Password] nvarchar(100),
   [Role] nvarchar(50),
);
go

create table License
(
   LicenseID nvarchar(10) primary key,
   LicenseName nvarchar(100),
   Describe nvarchar(max),
   Condition nvarchar(max),
   Cost nvarchar(max),
   [Time] nvarchar(max),
   ExamContent nvarchar(max),
   Tips nvarchar(max)
);
go

create table Users
(
   UserID uniqueidentifier default newid() primary key,
   AccountID uniqueidentifier unique,
   Avatar nvarchar(max),
   CCCD nvarchar(15),
   Email nvarchar(100),
   FullName nvarchar(100),
   BirthDate date,
   Nationality nvarchar(100),
   PhoneNumber nvarchar(20),
   [Address] nvarchar(100),

   foreign key (AccountID) references dbo.Account(AccountID)
);
go

create table Teacher
(
   TeacherID uniqueidentifier default newid() primary key,
   AccountID uniqueidentifier unique,
   LicenseID nvarchar(10),
   Avatar nvarchar(max),
   FullName nvarchar(100),
   Information nvarchar(max),
   ContactNumber nvarchar(20),
   Email nvarchar(100),

   foreign key (AccountID) references dbo.Account(AccountID),
   foreign key (LicenseID) references dbo.License(LicenseID)
);
go

create table Staff(
	StaffID uniqueidentifier default newid() primary key,
	AccountID uniqueidentifier unique,
	FullName nvarchar(100),
	Email nvarchar(100),
	ContactNumber nvarchar(20),

   foreign key (AccountID) references dbo.Account(AccountID)
);
go

create table [Admin](
	AdminID uniqueidentifier default newid() primary key,
	AccountID uniqueidentifier unique,
	FullName nvarchar(100),
	Email nvarchar(100),
	ContactNumber nvarchar(20),

   foreign key (AccountID) references dbo.Account(AccountID)
);
go

create table Quiz
(
   QuizID int identity(1,1) primary key,
   LicenseID nvarchar(10),
   [Timer] int,
   [Name] nvarchar(100),
   [Description] nvarchar(max),
   TotalDid int,

   foreign key (LicenseID) references dbo.License(LicenseID)
);
go

create table Question
(
   QuestionID int identity (1,1) primary key,
   LicenseID nvarchar(10),
   QuestionText nvarchar(max),
   QuestionImage nvarchar(max),
   isCritical bit,

   foreign key (LicenseID) references License(LicenseID)
);
go

create table Have(
   QuizID int not null,
   QuestionID int not null,

   primary key (QuizID, QuestionID),
   foreign key (QuizID) references dbo.Quiz(QuizID),
   foreign key (QuestionID) references dbo.Question(QuestionID)
);
go

create table Answer
(
   AnswerID int identity(1,1) primary key,
   QuestionID int not null,
   isCorrect bit,
   AnswerText nvarchar(max),
   AnswerImage nvarchar(max),

   foreign key (QuestionID) references Question(QuestionID)
);
go

create table Attempt (
    AttemptID uniqueidentifier default newid() primary key,
	UserID uniqueidentifier,
	QuizID int not null,
	AttemptTime time,
	AttemptDate datetime,
	TotalQuestion int,
	TotalAnswered int,
	Result bit,

	foreign key (UserID) references dbo.Users(UserID),
	foreign key (QuizID) references dbo.Quiz(QuizID)
);
go

create table AttemptDetail (
   AttemptDetailID uniqueidentifier default newid() primary key,
   AttemptID uniqueidentifier,
   SelectedAnswerID int null,
   QuestionID int not null,
   [Status] nvarchar(10),

   foreign key (AttemptID) references dbo.Attempt(AttemptID),
   foreign key (QuestionID) references dbo.Question(QuestionID),
   foreign key (SelectedAnswerID) references dbo.Answer(AnswerID),
);
go

create table Hire
(
	HireID uniqueidentifier default newid() primary key,
	TeacherID uniqueidentifier,
	UserID uniqueidentifier,
	HireDate datetime,
	[Status] nvarchar(50),

	foreign key (TeacherID) references dbo.Teacher(TeacherID),
	foreign key (UserID) references dbo.Users(UserID),
);
go

create table Schedule
(
   ScheduleID uniqueidentifier default newid() primary key,
   HireID uniqueidentifier,
   LicenseID nvarchar(10),
   StartTime time,
   EndTime time,
   [Date] datetime,
   [Address] nvarchar(100),
   [Status] nvarchar(50),

   foreign key (HireID) references dbo.Hire(HireID),
   foreign key (LicenseID) references dbo.License(LicenseID)
);
go

create table ExamProfile
(
	ExamProfileID uniqueidentifier default newid() primary key,
	UserID uniqueidentifier,
	LicenseID nvarchar(10),
	ExamDate datetime,
	ExamResult nvarchar(100),
	HealthCondition nvarchar(max),
	[Status] nvarchar(50),

	foreign key (UserID) references dbo.Users(UserID),
	foreign key (LicenseID) references dbo.License(LicenseID)
);
go

create table Vehicle
(
   VehicleID uniqueidentifier default newid() primary key,
   [Name] nvarchar(100),
   [Image] nvarchar(max),
   Brand nvarchar(100),
   [Type] nvarchar(100),
   Years int,
   ContactNumber nvarchar(20),
   [Address] nvarchar(100),
   RentPrice decimal,
   [Description] nvarchar(max),
   [Status] bit,
);
go

create table Rent
(
   RentID uniqueidentifier default newid() primary key,
   VehicleID uniqueidentifier,
   UserID uniqueidentifier,
   StartDate datetime,
   EndDate datetime,

   TotalRentPrice decimal,
   [Status] nvarchar(100),

   foreign key (VehicleID) references dbo.Vehicle(VehicleID),
   foreign key (UserID) references dbo.Users(UserID)
);
go

create table Feedback(
	FeedbackID uniqueidentifier default newid() primary key,
	userID uniqueidentifier,
	FeedbackDate datetime,
	SenderName nvarchar(100),
	Title nvarchar(max),
	[Description] nvarchar(max),
	[Status] nvarchar(50)

	foreign key (userID) references dbo.Users(UserID),
);
go

create table Response(
	ResponseID uniqueidentifier default newid() primary key,
	StaffID uniqueidentifier,
	FeedbackID uniqueidentifier,
	UserID uniqueidentifier,
	ResponseDate datetime,
	ReplierName nvarchar(100),
	[ReplyContent] nvarchar(max)
	
	foreign key (FeedbackID) references dbo.Feedback(FeedbackID),
	foreign key (userID) references dbo.Users(UserID),
	foreign key (staffID) references dbo.Staff(StaffID),
);
go


--RESET Auto Answer ID:
dbcc checkident ('Answer', reseed, 0);

--RESET Auto Question ID:
dbcc checkident ('Question', reseed, 0);

--RESET Auto Quiz ID:
dbcc checkident ('Quiz', reseed, 0);
