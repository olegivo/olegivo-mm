CREATE TABLE [dbo].[Files] (
    [Id]                       BIGINT        IDENTITY (1, 1) NOT NULL,
    [Drive]                    VARCHAR (50)  NOT NULL,
    [Path]                     VARCHAR (255) NOT NULL,
    [Filename]                 VARCHAR (255) NOT NULL,
    [Extention]                VARCHAR (10)  NOT NULL,
    [FullFileName]             AS            ((([Path]+'\')+[FileNameWithoutExtension])+[Extention]),
    [FileNameWithoutExtension] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED ([Id] ASC)
);

