
CREATE DATABASE IF NOT EXISTS LearnToCode;
USE LearnToCode;

-- 1. Users
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    Email VARCHAR(120) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL,
    Provider ENUM('local','google','github') DEFAULT 'local',
    ProviderId VARCHAR(255),
    ProfilePicture VARCHAR(255),
    Bio TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    LastLogin TIMESTAMP NULL,
    LastIp VARCHAR(45),
    IsVerified BOOLEAN DEFAULT FALSE,
    VerificationToken VARCHAR(255),
    IsBanned BOOLEAN DEFAULT FALSE,
    TwoFactorEnabled BOOLEAN DEFAULT FALSE,
    TwoFactorSecret VARCHAR(255)
);

-- 2. Roles
CREATE TABLE Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(30) NOT NULL UNIQUE,
    Description TEXT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 3. UserRoles
CREATE TABLE UserRoles (
    UserId INT,
    RoleId INT,
    AssignedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, RoleId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (RoleId) REFERENCES Roles(Id)
);

-- 4. PasswordResetTokens
CREATE TABLE PasswordResetTokens (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Token VARCHAR(255) NOT NULL UNIQUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    ExpiresAt DATETIME NOT NULL,
    Used BOOLEAN DEFAULT FALSE,
    UsedAt DATETIME NULL,
    IpAddress VARCHAR(45),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 5. UserProgress
CREATE TABLE UserProgress (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    Language VARCHAR(30),
    Level INT DEFAULT 1,
    XP INT DEFAULT 0,
    LessonsCompleted INT DEFAULT 0,
    MistakesCount INT DEFAULT 0,
    StreakDays INT DEFAULT 0,
    TotalTimeSpent INT DEFAULT 0,
    LastLessonId INT,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 6. Lessons
CREATE TABLE Lessons (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Title VARCHAR(120),
    Language VARCHAR(30),
    Content LONGTEXT,
    Difficulty ENUM('Beginner','Intermediate','Advanced'),
    XPReward INT DEFAULT 10,
    EstimatedMinutes INT DEFAULT 10,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 7. CodeSubmissions
CREATE TABLE CodeSubmissions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    Code LONGTEXT,
    Language VARCHAR(30),
    Output TEXT,
    IsCorrect BOOLEAN DEFAULT FALSE,
    ErrorMessage TEXT,
    ExecutionTime INT,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 8. AiChatHistory
CREATE TABLE AiChatHistory (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    UserMessage TEXT,
    AiResponse TEXT,
    Language VARCHAR(30),
    ModelUsed VARCHAR(50),
    TokensUsed INT DEFAULT 0,
    Rating INT DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 9. Settings
CREATE TABLE Settings (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    Theme ENUM('dark','light') DEFAULT 'dark',
    FontSize INT DEFAULT 14,
    AutoSave BOOLEAN DEFAULT TRUE,
    EmailNotifications BOOLEAN DEFAULT TRUE,
    Language VARCHAR(10) DEFAULT 'en',
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 10. EmailPreferences
CREATE TABLE EmailPreferences (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    DailyEmails BOOLEAN DEFAULT TRUE,
    WeeklyEmails BOOLEAN DEFAULT TRUE,
    MarketingEmails BOOLEAN DEFAULT FALSE,
    SecurityEmails BOOLEAN DEFAULT TRUE,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 11. Notifications
CREATE TABLE Notifications (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    Title VARCHAR(120),
    Message TEXT,
    Type ENUM('info','warning','success','error'),
    IsRead BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 12. Achievements
CREATE TABLE Achievements (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(120),
    Description TEXT,
    Icon VARCHAR(255),
    XPReward INT DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 13. UserAchievements
CREATE TABLE UserAchievements (
    UserId INT,
    AchievementId INT,
    EarnedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (UserId, AchievementId),
    FOREIGN KEY (UserId) REFERENCES Users(Id),
    FOREIGN KEY (AchievementId) REFERENCES Achievements(Id)
);

-- 14. CodeProjects
CREATE TABLE CodeProjects (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    Name VARCHAR(120),
    Description TEXT,
    Code LONGTEXT,
    Language VARCHAR(30),
    IsPublic BOOLEAN DEFAULT FALSE,
    Stars INT DEFAULT 0,
    Forks INT DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- 15. CodeSessions
CREATE TABLE CodeSessions (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT,
    SessionKey VARCHAR(255),
    ActiveFile TEXT,
    CursorPosition INT DEFAULT 0,
    LastSaved TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    IsActive BOOLEAN DEFAULT TRUE,
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
