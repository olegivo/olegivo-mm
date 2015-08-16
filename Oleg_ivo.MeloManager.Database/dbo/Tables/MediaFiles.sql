CREATE TABLE [dbo].[MediaFiles] (
    [Id]     BIGINT NOT NULL,
    [Length] INT    NULL,
    CONSTRAINT [PK_MediaFiles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MediaFiles_MediaContainers] FOREIGN KEY ([Id]) REFERENCES [dbo].[MediaContainers] ([Id]) ON DELETE CASCADE
);

