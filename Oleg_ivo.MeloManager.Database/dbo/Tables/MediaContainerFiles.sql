CREATE TABLE [dbo].[MediaContainerFiles] (
    [Id]               BIGINT   IDENTITY (1, 1) NOT NULL,
    [MediaContainerId] BIGINT   NOT NULL,
    [FileId]           BIGINT   NOT NULL,
    [DateInsert]       DATETIME CONSTRAINT [DF_MediaContainerFiles_DateInsert] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_MediaContainerFiles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MediaContainerFiles_Files] FOREIGN KEY ([FileId]) REFERENCES [dbo].[Files] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [FK_MediaContainerFiles_MediaContainers] FOREIGN KEY ([MediaContainerId]) REFERENCES [dbo].[MediaContainers] ([Id]) ON UPDATE CASCADE,
    CONSTRAINT [UK_MediaContainerFilesMediaContainerId_FileId] UNIQUE NONCLUSTERED ([MediaContainerId] ASC, [FileId] ASC)
);



