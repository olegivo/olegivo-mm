CREATE TABLE [dbo].[TMediaContainerType] (
    [MediaContainerType] VARCHAR (255)  NOT NULL,
    [Description]        NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_TMediaContainerType] PRIMARY KEY CLUSTERED ([MediaContainerType] ASC)
);

