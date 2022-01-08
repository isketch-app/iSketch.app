USE [master]
GO
/****** Object:  Database [iSketch.app]    Script Date: 1/8/2022 2:20:54 AM ******/
CREATE DATABASE [iSketch.app]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'iSketch.app', FILENAME = N'/var/opt/mssql/data/iSketch.app.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'iSketch.app_log', FILENAME = N'/var/opt/mssql/data/iSketch.app_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT
GO
ALTER DATABASE [iSketch.app] SET COMPATIBILITY_LEVEL = 150
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [iSketch.app].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [iSketch.app] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [iSketch.app] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [iSketch.app] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [iSketch.app] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [iSketch.app] SET ARITHABORT OFF 
GO
ALTER DATABASE [iSketch.app] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [iSketch.app] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [iSketch.app] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [iSketch.app] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [iSketch.app] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [iSketch.app] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [iSketch.app] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [iSketch.app] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [iSketch.app] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [iSketch.app] SET  DISABLE_BROKER 
GO
ALTER DATABASE [iSketch.app] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [iSketch.app] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [iSketch.app] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [iSketch.app] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [iSketch.app] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [iSketch.app] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [iSketch.app] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [iSketch.app] SET RECOVERY FULL 
GO
ALTER DATABASE [iSketch.app] SET  MULTI_USER 
GO
ALTER DATABASE [iSketch.app] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [iSketch.app] SET DB_CHAINING OFF 
GO
ALTER DATABASE [iSketch.app] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [iSketch.app] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [iSketch.app] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [iSketch.app] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [iSketch.app] SET QUERY_STORE = OFF
GO
USE [iSketch.app]
GO
/****** Object:  Table [dbo].[Words.Game.Difficulties]    Script Date: 1/8/2022 2:20:54 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Words.Game.Difficulties](
	[DifficultyID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[From] [int] NOT NULL,
	[To] [int] NOT NULL,
	[Name] [varchar](10) NOT NULL,
 CONSTRAINT [PK_Words.Difficulties] PRIMARY KEY CLUSTERED 
(
	[DifficultyID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_Words.Difficulties] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Words.Game]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Words.Game](
	[WordID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Word] [varchar](50) NOT NULL,
	[Score] [int] NOT NULL,
 CONSTRAINT [PK_Words] PRIMARY KEY CLUSTERED 
(
	[WordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_Words] UNIQUE NONCLUSTERED 
(
	[Word] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[Words.Game.Difficulties.Splice]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Words.Game.Difficulties.Splice]
AS
SELECT        WordID, Word, Score,
                             (SELECT        TOP (1) Name
                               FROM            dbo.[Words.Game.Difficulties]
                               WHERE        ([From] <= dbo.[Words.Game].Score) AND ([To] >= dbo.[Words.Game].Score)) AS Difficulty
FROM            dbo.[Words.Game]
GO
/****** Object:  Table [dbo].[Security.Users]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security.Users](
	[UserID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UserName] [varchar](36) NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[Password] [binary](128) NULL,
	[PasswordSalt] [binary](128) NULL,
	[ResetPasswordTime] [datetime] NULL,
	[Email] [varchar](50) NULL,
	[ProfilePicture] [varbinary](max) NULL,
	[Biography] [varchar](500) NULL,
	[EmailVerified] [bit] NOT NULL,
	[LastLogonTime] [datetime] NULL,
	[Score.Wins] [int] NOT NULL,
	[Score.Plays] [int] NOT NULL,
	[Score.Guesses] [int] NOT NULL,
	[Score.CorrectGuesses] [int] NOT NULL,
	[Score.Points] [int] NOT NULL,
	[Settings.AnonMode] [bit] NOT NULL,
	[Settings.DarkMode] [bit] NOT NULL,
	[OpenID.IdpID] [uniqueidentifier] NULL,
	[OpenID.Subject] [varchar](200) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Security.Groups.Membership]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security.Groups.Membership](
	[GroupID] [uniqueidentifier] NOT NULL,
	[UserID] [uniqueidentifier] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Security.Groups]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security.Groups](
	[GroupID] [uniqueidentifier] NOT NULL,
	[DisplayName] [varchar](20) NOT NULL,
	[PermissionsA] [binary](8) NOT NULL,
	[PermissionsB] [binary](8) NOT NULL,
 CONSTRAINT [PK_Security.Groups] PRIMARY KEY CLUSTERED 
(
	[GroupID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[Security.Users.Permissions.Splice]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Security.Users.Permissions.Splice]
AS
SELECT        dbo.[Security.Users].UserID, dbo.[Security.Groups].PermissionsA, dbo.[Security.Groups].PermissionsB
FROM            dbo.[Security.Groups] INNER JOIN
                         dbo.[Security.Groups.Membership] ON dbo.[Security.Groups].GroupID = dbo.[Security.Groups.Membership].GroupID RIGHT OUTER JOIN
                         dbo.[Security.Users] ON dbo.[Security.Groups.Membership].UserID = dbo.[Security.Users].UserID
GO
/****** Object:  Table [dbo].[Security.OpenID]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security.OpenID](
	[IdpID] [uniqueidentifier] NOT NULL,
	[DisplayName] [varchar](50) NOT NULL,
	[DisplayOrder] [int] NOT NULL,
	[DisplayIcon] [varbinary](max) NULL,
	[Enabled] [bit] NOT NULL,
	[ClientID] [varchar](100) NOT NULL,
	[ClientSecret] [varchar](100) NULL,
	[ExtraScopes] [varchar](100) NULL,
	[Endpoint.Authorization] [varchar](100) NOT NULL,
	[Endpoint.Token] [varchar](100) NOT NULL,
	[Endpoint.Logout] [varchar](100) NULL,
	[Claims.UserName] [varchar](50) NULL,
	[Claims.Email] [varchar](50) NULL,
	[Claims.UserPhoto] [varchar](50) NULL,
 CONSTRAINT [PK_Security] PRIMARY KEY CLUSTERED 
(
	[IdpID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[Security.Users.Splice]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Security.Users.Splice]
AS
SELECT        UserID, UserName, CreatedTime, ResetPasswordTime, Email, Biography, EmailVerified, LastLogonTime, [Score.Wins], [Score.Plays], [Score.Guesses], [Score.CorrectGuesses], [Score.Points],
                             (SELECT        DisplayName
                               FROM            dbo.[Security.OpenID]
                               WHERE        (dbo.[Security.Users].[OpenID.IdpID] = IdpID)) AS IdpName
FROM            dbo.[Security.Users]
GO
/****** Object:  Table [dbo].[Security.Sessions]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security.Sessions](
	[SessionID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[SessionTime] [datetime] NOT NULL,
	[SessionIP] [char](39) NULL,
	[SessionIPVersion] [smallint] NULL,
	[UserID] [uniqueidentifier] NULL,
 CONSTRAINT [PK_Sessions] PRIMARY KEY CLUSTERED 
(
	[SessionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[Security.Sessions.Splice]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Security.Sessions.Splice]
AS
SELECT        dbo.[Security.Sessions].SessionID, dbo.[Security.Sessions].SessionTime, dbo.[Security.Sessions].SessionIP, dbo.[Security.Sessions].SessionIPVersion, dbo.[Security.Sessions].UserID, dbo.[Security.Users].UserName
FROM            dbo.[Security.Sessions] INNER JOIN
                         dbo.[Security.Users] ON dbo.[Security.Sessions].UserID = dbo.[Security.Users].UserID
GO
/****** Object:  Table [dbo].[Security.Bans]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security.Bans](
	[BanID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UserID] [uniqueidentifier] NULL,
	[CIDR.IPv4] [char](18) NULL,
	[CIDR.IPv6] [char](43) NULL,
	[Reason] [varchar](50) NULL,
	[Note] [varchar](100) NULL,
	[Description] [varchar](50) NULL,
	[ExpiresTime] [datetime] NULL,
	[CreatedTime] [datetime] NOT NULL,
	[IsBanEnabled] [bit] NOT NULL,
 CONSTRAINT [PK_Bans] PRIMARY KEY CLUSTERED 
(
	[BanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[System.Properties]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[System.Properties](
	[Property] [varchar](50) NOT NULL,
	[Value] [varchar](max) NULL,
 CONSTRAINT [PK_Properties] PRIMARY KEY CLUSTERED 
(
	[Property] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Words.Banned]    Script Date: 1/8/2022 2:20:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Words.Banned](
	[WordID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[Word] [varchar](50) NOT NULL,
	[Contains] [bit] NOT NULL,
 CONSTRAINT [PK_Words.Banned] PRIMARY KEY CLUSTERED 
(
	[WordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [IX_Words.Banned] UNIQUE NONCLUSTERED 
(
	[Word] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Security.Groups]    Script Date: 1/8/2022 2:20:55 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Security.Groups] ON [dbo].[Security.Groups]
(
	[DisplayName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users]    Script Date: 1/8/2022 2:20:55 AM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users] ON [dbo].[Security.Users]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_BanID]  DEFAULT (newid()) FOR [BanID]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_UserID]  DEFAULT (NULL) FOR [UserID]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_IPv4]  DEFAULT (NULL) FOR [CIDR.IPv4]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_IPv6]  DEFAULT (NULL) FOR [CIDR.IPv6]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_Reason]  DEFAULT (NULL) FOR [Reason]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_Note]  DEFAULT (NULL) FOR [Note]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_Description]  DEFAULT (NULL) FOR [Description]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_ExpiresTime]  DEFAULT (NULL) FOR [ExpiresTime]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Security.Bans] ADD  CONSTRAINT [DF_Bans_Enabled]  DEFAULT ((1)) FOR [IsBanEnabled]
GO
ALTER TABLE [dbo].[Security.Groups] ADD  CONSTRAINT [DF_Security.Groups_SGroupID]  DEFAULT (newid()) FOR [GroupID]
GO
ALTER TABLE [dbo].[Security.Groups] ADD  CONSTRAINT [DF_Security.Groups_Permissions]  DEFAULT (0x00) FOR [PermissionsA]
GO
ALTER TABLE [dbo].[Security.Groups] ADD  CONSTRAINT [DF_Security.Groups_PermissionsB]  DEFAULT (0x00) FOR [PermissionsB]
GO
ALTER TABLE [dbo].[Security.OpenID] ADD  CONSTRAINT [DF_Security.IDPs.OpenID_IdpID]  DEFAULT (newid()) FOR [IdpID]
GO
ALTER TABLE [dbo].[Security.OpenID] ADD  CONSTRAINT [DF_Table_1_Priority]  DEFAULT ((0)) FOR [DisplayOrder]
GO
ALTER TABLE [dbo].[Security.OpenID] ADD  CONSTRAINT [DF_Security_Enabled]  DEFAULT ((1)) FOR [Enabled]
GO
ALTER TABLE [dbo].[Security.Sessions] ADD  CONSTRAINT [DF_Sessions_SessionID]  DEFAULT (newid()) FOR [SessionID]
GO
ALTER TABLE [dbo].[Security.Sessions] ADD  CONSTRAINT [DF_Sessions_InitializedTime]  DEFAULT (getdate()) FOR [SessionTime]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_UserID]  DEFAULT (newid()) FOR [UserID]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_ResetPasswordTime]  DEFAULT (NULL) FOR [ResetPasswordTime]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Email]  DEFAULT (NULL) FOR [Email]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_ProfilePicture]  DEFAULT (NULL) FOR [ProfilePicture]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Biography]  DEFAULT (NULL) FOR [Biography]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_EmailVerified]  DEFAULT ((0)) FOR [EmailVerified]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Score.Wins]  DEFAULT ((0)) FOR [Score.Wins]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Score.Plays]  DEFAULT ((0)) FOR [Score.Plays]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Score.Guesses]  DEFAULT ((0)) FOR [Score.Guesses]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Score.CorrectGuesses]  DEFAULT ((0)) FOR [Score.CorrectGuesses]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Score.Points]  DEFAULT ((0)) FOR [Score.Points]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Settings.AnonMode]  DEFAULT ((0)) FOR [Settings.AnonMode]
GO
ALTER TABLE [dbo].[Security.Users] ADD  CONSTRAINT [DF_Users_Settings.DarkMode]  DEFAULT ((0)) FOR [Settings.DarkMode]
GO
ALTER TABLE [dbo].[Words.Banned] ADD  CONSTRAINT [DF_Words.Banned_WordID]  DEFAULT (newid()) FOR [WordID]
GO
ALTER TABLE [dbo].[Words.Banned] ADD  CONSTRAINT [DF_Words.Banned_Contains]  DEFAULT ((1)) FOR [Contains]
GO
ALTER TABLE [dbo].[Words.Game] ADD  CONSTRAINT [DF_Words_WordID]  DEFAULT (newid()) FOR [WordID]
GO
ALTER TABLE [dbo].[Words.Game] ADD  CONSTRAINT [DF_Words_Score]  DEFAULT ((0)) FOR [Score]
GO
ALTER TABLE [dbo].[Words.Game.Difficulties] ADD  CONSTRAINT [DF_Words.Difficulties_DifficultyID]  DEFAULT (newid()) FOR [DifficultyID]
GO
ALTER TABLE [dbo].[Words.Game.Difficulties] ADD  CONSTRAINT [DF_Words.Difficulties_From]  DEFAULT ((0)) FOR [From]
GO
ALTER TABLE [dbo].[Words.Game.Difficulties] ADD  CONSTRAINT [DF_Words.Difficulties_To]  DEFAULT ((0)) FOR [To]
GO
ALTER TABLE [dbo].[Security.Bans]  WITH CHECK ADD  CONSTRAINT [FK_Bans_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Security.Users] ([UserID])
GO
ALTER TABLE [dbo].[Security.Bans] CHECK CONSTRAINT [FK_Bans_Users]
GO
ALTER TABLE [dbo].[Security.Groups.Membership]  WITH CHECK ADD  CONSTRAINT [FK_Security.Groups.Membership_Security.Groups] FOREIGN KEY([GroupID])
REFERENCES [dbo].[Security.Groups] ([GroupID])
GO
ALTER TABLE [dbo].[Security.Groups.Membership] CHECK CONSTRAINT [FK_Security.Groups.Membership_Security.Groups]
GO
ALTER TABLE [dbo].[Security.Groups.Membership]  WITH CHECK ADD  CONSTRAINT [FK_Security.Groups.Membership_Security.Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Security.Users] ([UserID])
GO
ALTER TABLE [dbo].[Security.Groups.Membership] CHECK CONSTRAINT [FK_Security.Groups.Membership_Security.Users]
GO
ALTER TABLE [dbo].[Security.Sessions]  WITH CHECK ADD  CONSTRAINT [FK_Sessions_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Security.Users] ([UserID])
GO
ALTER TABLE [dbo].[Security.Sessions] CHECK CONSTRAINT [FK_Sessions_Users]
GO
ALTER TABLE [dbo].[Security.Users]  WITH CHECK ADD  CONSTRAINT [FK_Security.Users_Security.OpenID] FOREIGN KEY([OpenID.IdpID])
REFERENCES [dbo].[Security.OpenID] ([IdpID])
GO
ALTER TABLE [dbo].[Security.Users] CHECK CONSTRAINT [FK_Security.Users_Security.OpenID]
GO
ALTER TABLE [dbo].[Security.Users]  WITH CHECK ADD  CONSTRAINT [FK_Security.Users_Security.Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Security.Users] ([UserID])
GO
ALTER TABLE [dbo].[Security.Users] CHECK CONSTRAINT [FK_Security.Users_Security.Users]
GO
ALTER TABLE [dbo].[Words.Game]  WITH CHECK ADD  CONSTRAINT [FK_Words_Words] FOREIGN KEY([WordID])
REFERENCES [dbo].[Words.Game] ([WordID])
GO
ALTER TABLE [dbo].[Words.Game] CHECK CONSTRAINT [FK_Words_Words]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[37] 4[29] 2[11] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Security.Sessions"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 215
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Security.Users"
            Begin Extent = 
               Top = 6
               Left = 253
               Bottom = 136
               Right = 455
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Security.Sessions.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Security.Sessions.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Security.Groups"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 136
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Security.Groups.Membership"
            Begin Extent = 
               Top = 6
               Left = 246
               Bottom = 102
               Right = 416
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Security.Users"
            Begin Extent = 
               Top = 102
               Left = 246
               Bottom = 232
               Right = 448
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 3525
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Security.Users.Permissions.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Security.Users.Permissions.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[22] 4[17] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Security.Users"
            Begin Extent = 
               Top = 4
               Left = 41
               Bottom = 167
               Right = 739
            End
            DisplayFlags = 280
            TopColumn = 14
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 22
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 8160
         Alias = 3240
         Table = 1170
         Output = 2220
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Security.Users.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Security.Users.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Words.Game"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 119
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Words.Game.Difficulties.Splice'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Words.Game.Difficulties.Splice'
GO
USE [master]
GO
ALTER DATABASE [iSketch.app] SET  READ_WRITE 
GO
