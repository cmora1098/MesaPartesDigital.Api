USE [BD_RCPDOC]
GO
/****** Objeto: StoredProcedure [dbo].[USP_Tramite_ListarHistorialPorUsuario] Fecha de script: 31/07/2026 08:33:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_Tramite_ListarHistorialPorUsuario]
    @iCodPer INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        A.iCodAsunto, -- Añadido para el ID
        A.vAutoGenerado AS Codigo,
        A.vNombreAsunto AS Asunto,
        E.vNombreEstado AS Estado,
        CONVERT(VARCHAR(10), A.dtFechaCreacion, 103) AS Fecha
    FROM [BD_RCPDOC].[dbo].[T_Asunto] A
    INNER JOIN [BD_RCPDOC].[dbo].[T_Estado] E ON A.iCodEstado = E.iCodEstado
    WHERE A.iCodPer = @iCodPer
    ORDER BY A.dtFechaCreacion DESC;
END