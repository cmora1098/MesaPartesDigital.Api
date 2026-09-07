USE [BD_RCPDOC]
GO
/****** Objeto: StoredProcedure [dbo].[USP_ListarAnexosTramite] Fecha de script: 31/07/2026 09:07:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_ListarAnexosTramite]
    @iCodAsunto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        iCodDoc,
        iCodAsunto,
        vNroDoc AS Nombre,
        vRutaDoc AS Ruta,
        dFecDoc AS Fecha,
        vReferencia AS Descripcion
    FROM [BD_RCPDOC].[dbo].[T_Documento]
    WHERE iCodAsunto = @iCodAsunto AND bActivo = 1 AND btipo = 1;
END
