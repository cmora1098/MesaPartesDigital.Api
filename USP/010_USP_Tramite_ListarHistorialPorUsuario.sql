CREATE PROCEDURE [dbo].[USP_Tramite_ListarHistorialPorUsuario]
    @iCodPer INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        A.vAutoGenerado AS Codigo,
        A.vNombreAsunto AS Asunto,
        E.vNombreEstado AS Estado,
        CONVERT(VARCHAR(10), A.dtFechaCreacion, 103) AS Fecha -- Formato dd/mm/yyyy
    FROM [BD_RCPDOC].[dbo].[T_Asunto] A
    INNER JOIN [BD_RCPDOC].[dbo].[T_Estado] E ON A.iCodEstado = E.iCodEstado
    WHERE A.iCodPer = @iCodPer
    ORDER BY A.dtFechaCreacion DESC;
END
GO