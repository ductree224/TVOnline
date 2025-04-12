-- Tạo bảng EmployerRegistrationRequests nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EmployerRegistrationRequests]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EmployerRegistrationRequests](
        [RequestId] [nvarchar](450) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [Email] [nvarchar](max) NOT NULL,
        [CompanyName] [nvarchar](max) NOT NULL,
        [Description] [nvarchar](max) NOT NULL,
        [Field] [nvarchar](max) NOT NULL,
        [LogoURL] [nvarchar](max) NULL,
        [Website] [nvarchar](max) NULL,
        [CityId] [int] NOT NULL,
        [Status] [nvarchar](max) NOT NULL,
        [AdminNotes] [nvarchar](max) NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        [ProcessedAt] [datetime2](7) NULL,
        CONSTRAINT [PK_EmployerRegistrationRequests] PRIMARY KEY CLUSTERED 
        (
            [RequestId] ASC
        ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[EmployerRegistrationRequests] WITH CHECK ADD CONSTRAINT [FK_EmployerRegistrationRequests_AspNetUsers_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[AspNetUsers] ([Id])
    ON DELETE CASCADE

    ALTER TABLE [dbo].[EmployerRegistrationRequests] WITH CHECK ADD CONSTRAINT [FK_EmployerRegistrationRequests_Cities_CityId] FOREIGN KEY([CityId])
    REFERENCES [dbo].[Cities] ([CityId])
    ON DELETE CASCADE
END

-- Tạo bảng Notifications nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Notifications]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Notifications](
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [nvarchar](450) NOT NULL,
        [Title] [nvarchar](max) NOT NULL,
        [Message] [nvarchar](max) NOT NULL,
        [Link] [nvarchar](max) NULL,
        [IsRead] [bit] NOT NULL,
        [CreatedAt] [datetime2](7) NOT NULL,
        CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED 
        (
            [Id] ASC
        ) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

    ALTER TABLE [dbo].[Notifications] WITH CHECK ADD CONSTRAINT [FK_Notifications_AspNetUsers_UserId] FOREIGN KEY([UserId])
    REFERENCES [dbo].[AspNetUsers] ([Id])
    ON DELETE CASCADE
END

-- Thêm role Employer nếu chưa tồn tại
IF NOT EXISTS (SELECT * FROM AspNetRoles WHERE Name = 'Employer')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Employer', 'EMPLOYER', NEWID())
END
