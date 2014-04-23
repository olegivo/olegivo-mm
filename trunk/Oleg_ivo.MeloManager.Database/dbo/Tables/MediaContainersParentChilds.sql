CREATE TABLE [dbo].[MediaContainersParentChilds] (
    [Id]       BIGINT IDENTITY (1, 1) NOT NULL,
    [ParentId] BIGINT NOT NULL,
    [ChildId]  BIGINT NOT NULL,
    CONSTRAINT [PK_MediaContainersInMediaContainers] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MediaContainersParentChild_MediaContainer_Child] FOREIGN KEY ([ChildId]) REFERENCES [dbo].[MediaContainers] ([Id]),
    CONSTRAINT [FK_MediaContainersParentChild_MediaContainer_Parent] FOREIGN KEY ([ParentId]) REFERENCES [dbo].[MediaContainers] ([Id]),
    CONSTRAINT [UK_MediaContainersParentChilds] UNIQUE NONCLUSTERED ([ParentId] ASC, [ChildId] ASC)
);

