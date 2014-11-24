CREATE TABLE [dbo].[Files] (
    [Id]                       BIGINT         IDENTITY (1, 1) NOT NULL,
    [Drive]                    NVARCHAR (50)  NOT NULL,
    [Path]                     NVARCHAR (255) NOT NULL,
    [Filename]                 NVARCHAR (255) NOT NULL,
    [Extention]                NVARCHAR (50)  NOT NULL,
    [FullFileName]             AS             ([dbo].[GetFullFulename]([Path],[FileNameWithoutExtension],[Extention])) PERSISTED,
    [FileNameWithoutExtension] NVARCHAR (255) NOT NULL,
    CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED ([Id] ASC)
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [UK_Files_FullFileName]
    ON [dbo].[Files]([FullFileName] ASC);

