USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistrarAnexoTramite]
    @iCodAsunto INT,
    @vRutaDoc VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    -- Insertamos el anexo clonando los datos del documento principal (btipo = 0) del mismo asunto
    INSERT INTO [dbo].[T_Documento] (
        iCodPer,
        iCodAsunto,
        vRutaDoc,
        iCodTipoDoc,
        vNroDoc,
        dFecDoc,
        dFecRecepcion,
        vReferencia,
        vNroPagFolios,
        vLink,
        bActivo,
        dtFechaCargaArchivo,
        btipo
    )
    SELECT TOP 1
        iCodPer,
        iCodAsunto,
        @vRutaDoc,           -- Único valor que cambia: la ruta del nuevo anexo
        iCodTipoDoc,
        vNroDoc,
        dFecDoc,
        dFecRecepcion,
        vReferencia,
        vNroPagFolios,
        vLink,
        1,                   -- bActivo en 1
        GETDATE(),           -- Fecha actual de carga
        1                    -- btipo en 1 indicando que es un Anexo
    FROM [dbo].[T_Documento]
    WHERE iCodAsunto = @iCodAsunto 
      AND btipo = 0 
      AND bActivo = 1
    ORDER BY iCodDoc DESC;
END
GO