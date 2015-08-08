CREATE TABLE [dbo].[Files] (
    [Id]                       BIGINT         IDENTITY (1, 1) NOT NULL,
    [Drive]                    NVARCHAR (50)  NOT NULL,
    [Path]                     NVARCHAR (255) NOT NULL,
    [Filename]                 NVARCHAR (255) NOT NULL,
    [Extention]                NVARCHAR (50)  NOT NULL,
    [FullFileName]             AS             ([dbo].[GetFullFulename]([Path],[FileNameWithoutExtension],[Extention])) PERSISTED,
    [FileNameWithoutExtension] NVARCHAR (255) NOT NULL,
    [DateUpdate]               DATETIME       CONSTRAINT [DF_Files_DateUpdate] DEFAULT (getdate()) NOT NULL,
    [DateInsert]               DATETIME       CONSTRAINT [DF_Files_DateInsert] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Files] PRIMARY KEY CLUSTERED ([Id] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [UK_Files_FullFileName]
    ON [dbo].[Files]([FullFileName] ASC);


GO
-- =============================================
-- Author:		olegivo
-- Create date: 2015-08-08
-- Description:	Обновление поля DateUpdate
-- =============================================
CREATE TRIGGER dbo.Files_DateUpdate 
   ON  dbo.Files 
   AFTER INSERT,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
    IF 1=0
		OR UPDATE([Drive])
		OR UPDATE([Path])
		OR UPDATE([Filename])
		OR UPDATE([Extention])
		OR UPDATE([FileNameWithoutExtension])
	BEGIN
		DECLARE @DT DATETIME = GETDATE()
		
		UPDATE F SET DateUpdate = @DT
		FROM INSERTED I
			INNER JOIN dbo.Files F ON F.Id = I.Id
		WHERE 1=1
			AND NOT
			( 
				1=1
				AND F.[Drive] = I.[Drive]
				AND F.[Path] = I.[Path]
				AND F.[Filename] = I.[Filename]
				AND F.[Extention] = I.[Extention]
				AND F.[Extention] = I.[Extention]
			)
	END
END