USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[USP_ActualizarDocumentoPrincipal]
    @iCodAsunto INT,
    @vNuevaRutaDoc VARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;

    BEGIN TRY
        -- 1. Desactivar el documento principal actual (btipo = 0 y bActivo = 1)
        UPDATE [dbo].[T_Documento]
        SET bActivo = 0
        WHERE iCodAsunto = @iCodAsunto 
          AND btipo = 0 
          AND bActivo = 1;

        -- 2. Insertar el nuevo documento principal clonando los datos del anterior pero con la nueva ruta y bActivo = 1
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
            @vNuevaRutaDoc,
            iCodTipoDoc,
            vNroDoc,
            dFecDoc,
            dFecRecepcion,
            vReferencia,
            vNroPagFolios,
            vLink,
            1, -- bActivo en 1 para el nuevo documento vigente
            GETDATE(),
            0  -- btipo en 0 indicando que es principal
        FROM [dbo].[T_Documento]
        WHERE iCodAsunto = @iCodAsunto 
          AND btipo = 0
        ORDER BY iCodDoc DESC; -- Toma el último registrado como base si hubiera historial previo

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO