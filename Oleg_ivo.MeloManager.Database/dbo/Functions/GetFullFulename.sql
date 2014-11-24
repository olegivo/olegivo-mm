-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION dbo.GetFullFulename
(
	-- Add the parameters for the function here
	@Path NVARCHAR(255), 
	@FileNameWithoutExtension NVARCHAR(255), 
	@Extention NVARCHAR(10)
)
RETURNS NVARCHAR(521)
WITH SCHEMABINDING
AS
BEGIN
	RETURN @Path+'\'+@FileNameWithoutExtension+@Extention
END