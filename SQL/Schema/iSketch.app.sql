USE [iSketch.app]
GO
/****** Object:  Table [dbo].[Words.Difficulties]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Words.Difficulties](
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
/****** Object:  Table [dbo].[Words]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Words](
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
/****** Object:  View [dbo].[Words.Difficulty]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[Words.Difficulty]
AS
SELECT        WordID, Word, Score,
                             (SELECT        TOP (1) Name
                               FROM            dbo.[Words.Difficulties]
                               WHERE        ([From] <= dbo.Words.Score) AND ([To] >= dbo.Words.Score)) AS Difficulty
FROM            dbo.Words
GO
/****** Object:  Table [dbo].[Bans]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Bans](
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
/****** Object:  Table [dbo].[Properties]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Properties](
	[Property] [varchar](50) NOT NULL,
	[Value] [varchar](500) NULL,
 CONSTRAINT [PK_Properties] PRIMARY KEY CLUSTERED 
(
	[Property] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sessions]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sessions](
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
/****** Object:  Table [dbo].[Users]    Script Date: 8/3/2021 1:23:03 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [uniqueidentifier] ROWGUIDCOL  NOT NULL,
	[UserName] [char](20) NOT NULL,
	[CreatedTime] [datetime] NULL,
	[Password] [binary](384) NULL,
	[ResetPasswordTime] [datetime] NULL,
	[Email] [varchar](50) NULL,
	[Biography] [varchar](500) NULL,
	[Score.Wins] [int] NOT NULL,
	[Score.Plays] [int] NOT NULL,
	[Score.Guesses] [int] NOT NULL,
	[Score.CorrectGuesses] [int] NOT NULL,
	[Settings.AnonMode] [bit] NOT NULL,
	[Settings.DarkMode] [bit] NOT NULL,
	[ProfilePicture] [varbinary](max) NULL,
	[Score.Points] [int] NOT NULL,
	[ThirdPartyAuthID] [uniqueidentifier] NULL,
	[LastLogonTime] [datetime] NULL,
	[EmailVerified] [bit] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Words.Banned]    Script Date: 8/3/2021 1:23:03 AM ******/
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
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_BanID]  DEFAULT (newid()) FOR [BanID]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_UserID]  DEFAULT (NULL) FOR [UserID]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_IPv4]  DEFAULT (NULL) FOR [CIDR.IPv4]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_IPv6]  DEFAULT (NULL) FOR [CIDR.IPv6]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_Reason]  DEFAULT (NULL) FOR [Reason]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_Note]  DEFAULT (NULL) FOR [Note]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_Description]  DEFAULT (NULL) FOR [Description]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_ExpiresTime]  DEFAULT (NULL) FOR [ExpiresTime]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Bans] ADD  CONSTRAINT [DF_Bans_Enabled]  DEFAULT ((1)) FOR [IsBanEnabled]
GO
ALTER TABLE [dbo].[Sessions] ADD  CONSTRAINT [DF_Sessions_SessionID]  DEFAULT (newid()) FOR [SessionID]
GO
ALTER TABLE [dbo].[Sessions] ADD  CONSTRAINT [DF_Sessions_InitializedTime]  DEFAULT (getdate()) FOR [SessionTime]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_UserID]  DEFAULT (newid()) FOR [UserID]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_CreatedTime]  DEFAULT (getdate()) FOR [CreatedTime]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_ResetPasswordTime]  DEFAULT (NULL) FOR [ResetPasswordTime]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Email]  DEFAULT (NULL) FOR [Email]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Biography]  DEFAULT (NULL) FOR [Biography]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Score.Wins]  DEFAULT ((0)) FOR [Score.Wins]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Score.Plays]  DEFAULT ((0)) FOR [Score.Plays]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Score.Guesses]  DEFAULT ((0)) FOR [Score.Guesses]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Score.CorrectGuesses]  DEFAULT ((0)) FOR [Score.CorrectGuesses]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Settings.AnonMode]  DEFAULT ((0)) FOR [Settings.AnonMode]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Settings.DarkMode]  DEFAULT ((0)) FOR [Settings.DarkMode]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_ProfilePicture]  DEFAULT (NULL) FOR [ProfilePicture]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Score.Points]  DEFAULT ((0)) FOR [Score.Points]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_EmailVerified]  DEFAULT ((0)) FOR [EmailVerified]
GO
ALTER TABLE [dbo].[Words] ADD  CONSTRAINT [DF_Words_WordID]  DEFAULT (newid()) FOR [WordID]
GO
ALTER TABLE [dbo].[Words] ADD  CONSTRAINT [DF_Words_Score]  DEFAULT ((0)) FOR [Score]
GO
ALTER TABLE [dbo].[Words.Banned] ADD  CONSTRAINT [DF_Words.Banned_WordID]  DEFAULT (newid()) FOR [WordID]
GO
ALTER TABLE [dbo].[Words.Banned] ADD  CONSTRAINT [DF_Words.Banned_Contains]  DEFAULT ((1)) FOR [Contains]
GO
ALTER TABLE [dbo].[Words.Difficulties] ADD  CONSTRAINT [DF_Words.Difficulties_DifficultyID]  DEFAULT (newid()) FOR [DifficultyID]
GO
ALTER TABLE [dbo].[Words.Difficulties] ADD  CONSTRAINT [DF_Words.Difficulties_From]  DEFAULT ((0)) FOR [From]
GO
ALTER TABLE [dbo].[Words.Difficulties] ADD  CONSTRAINT [DF_Words.Difficulties_To]  DEFAULT ((0)) FOR [To]
GO
ALTER TABLE [dbo].[Bans]  WITH CHECK ADD  CONSTRAINT [FK_Bans_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Bans] CHECK CONSTRAINT [FK_Bans_Users]
GO
ALTER TABLE [dbo].[Sessions]  WITH CHECK ADD  CONSTRAINT [FK_Sessions_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
GO
ALTER TABLE [dbo].[Sessions] CHECK CONSTRAINT [FK_Sessions_Users]
GO
ALTER TABLE [dbo].[Words]  WITH CHECK ADD  CONSTRAINT [FK_Words_Words] FOREIGN KEY([WordID])
REFERENCES [dbo].[Words] ([WordID])
GO
ALTER TABLE [dbo].[Words] CHECK CONSTRAINT [FK_Words_Words]
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
         Begin Table = "Words"
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
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Words.Difficulty'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'Words.Difficulty'
GO
