CREATE TABLE [dbo].[MediaContainers] (
    [Id]         BIGINT        IDENTITY (1, 1) NOT NULL,
    [Type]       VARCHAR (255) NOT NULL,
    [Name]       VARCHAR (255) NOT NULL,
    [IsRepaired] BIT           NOT NULL,
    [IsRoot]     BIT           CONSTRAINT [DF_MediaContainers_IsRoot] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_MediaContainer] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Находится ли контейнер в корне иерархии. Контейнер может находиться и в корне, и быть вложенным одновременно.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MediaContainers', @level2type = N'COLUMN', @level2name = N'IsRoot';

