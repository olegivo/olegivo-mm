CREATE TABLE [dbo].[Categories] (
    [Id] BIGINT NOT NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Categories_MediaContainers] FOREIGN KEY ([Id]) REFERENCES [dbo].[MediaContainers] ([Id]) ON DELETE CASCADE
);

