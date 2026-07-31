USE [BD_RCPDOC]
GO
/****** Objeto: StoredProcedure [dbo].[USP_EliminarAnexoTramite] Fecha de script: 31/07/2026 09:10:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_EliminarAnexoTramite]
    @iCodDoc INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [BD_RCPDOC].[dbo].[T_Documento]
    SET bActivo = 0
    WHERE iCodDoc = @iCodDoc;
END
