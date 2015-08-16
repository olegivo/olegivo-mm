CREATE TABLE [dbo].[Playlists] (
    [Id] BIGINT NOT NULL,
    CONSTRAINT [PK_Playlists] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Playlists_MediaContainers] FOREIGN KEY ([Id]) REFERENCES [dbo].[MediaContainers] ([Id]) ON DELETE CASCADE
);

