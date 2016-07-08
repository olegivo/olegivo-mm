CREATE TABLE [dbo].[MediaContainers] (
    [Id]         BIGINT           IDENTITY (1, 1) NOT NULL,
    [Name]       VARCHAR (255)    NOT NULL,
    [IsRoot]     BIT              CONSTRAINT [DF_MediaContainers_IsRoot] DEFAULT ((1)) NOT NULL,
    [DateUpdate] DATETIME         CONSTRAINT [DF_MediaContainers_DateUpdate] DEFAULT (getdate()) NOT NULL,
    [DateInsert] DATETIME         CONSTRAINT [DF_MediaContainers_DateInsert] DEFAULT (getdate()) NOT NULL,
    [RowGuid]    UNIQUEIDENTIFIER CONSTRAINT [DF_MediaContainers_RowGuid] DEFAULT (newid()) NOT NULL,
    CONSTRAINT [PK_MediaContainer] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Находится ли контейнер в корне иерархии. Контейнер может находиться и в корне, и быть вложенным одновременно.', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MediaContainers', @level2type = N'COLUMN', @level2name = N'IsRoot';


GO
-- =============================================
-- Author:		olegivo
-- Create date: 2015-08-08
-- Description:	Обновление поля DateUpdate
-- =============================================
CREATE TRIGGER [dbo].[MediaContainers_DateUpdate] 
   ON  [dbo].[MediaContainers]
   AFTER INSERT,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
    IF 1=0
		OR UPDATE(Name)
		OR UPDATE(IsRoot)
	BEGIN
		DECLARE @DT DATETIME = GETDATE()
		
		UPDATE MC SET DateUpdate = @DT
		FROM INSERTED I
			LEFT OUTER JOIN DELETED D ON D.Id = I.Id
			INNER JOIN dbo.MediaContainers MC ON MC.Id = I.Id
		WHERE 1=1
			AND NOT
			( 
				1=1
				AND D.Id IS NOT NULL
				AND D.Name = I.Name
				AND D.IsRoot = I.IsRoot
			)
	END
END