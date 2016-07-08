CREATE TABLE [dbo].[MediaFiles] (
    [Id]         BIGINT         NOT NULL,
    [Length]     INT            NULL,
    [Artist]     NVARCHAR (255) NULL,
    [Album]      NVARCHAR (255) NULL,
    [Title]      NVARCHAR (255) NULL,
    [Year]       INT            NULL,
    [Track]      INT            NULL,
    [TrackCount] INT            NULL,
    CONSTRAINT [PK_MediaFiles] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_MediaFiles_MediaContainers] FOREIGN KEY ([Id]) REFERENCES [dbo].[MediaContainers] ([Id]) ON DELETE CASCADE
);

GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'Длительность (сек.)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'MediaFiles', @level2type = N'COLUMN', @level2name = N'Length';


GO

-- =============================================
-- Author:		olegivo
-- Create date: 2016-07-08
-- Description:	Обновление поля DateUpdate
-- =============================================
CREATE TRIGGER [dbo].[MediaFile_DateUpdate] 
   ON  [dbo].[MediaFiles]
   AFTER INSERT,UPDATE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here
    IF 1=0
		OR UPDATE(Artist)
		OR UPDATE(Album)
		OR UPDATE([Length])
		OR UPDATE(Title)
		OR UPDATE(Track)
		OR UPDATE(TrackCount)
		OR UPDATE([Year])
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
				AND ISNULL(D.Artist, '') = ISNULL(I.Artist, '')
				AND ISNULL(D.Album, '') = ISNULL(I.Album, '')
				AND ISNULL(D.[Length], 0) = ISNULL(I.[Length], 0)
				AND ISNULL(D.Title, '') = ISNULL(I.Title, '')
				AND ISNULL(D.Track, 0) = ISNULL(I.Track, 0)
				AND ISNULL(D.TrackCount, 0) = ISNULL(I.TrackCount, 0)
				AND ISNULL(D.[Year], 0) = ISNULL(I.[Year], 0)
			)
	END
END