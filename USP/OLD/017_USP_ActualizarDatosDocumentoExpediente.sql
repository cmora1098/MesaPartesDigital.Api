USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE OR ALTER PROCEDURE [dbo].[USP_ActualizarDatosDocumentoExpediente]
    @iCodAsunto INT,
    @vNombreAsunto VARCHAR(255),
    @iCodTipoDoc INT,
    @vNroDoc VARCHAR(50),
    @dFecDoc DATE,
    @vReferencia VARCHAR(255),
    @vNroPagFolios VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Actualizar el Asunto/Resumen en T_Asunto
        UPDATE [BD_RCPDOC].[dbo].[T_Asunto]
        SET [vNombreAsunto] = @vNombreAsunto
        WHERE [iCodAsunto] = @iCodAsunto;

        -- 2. Actualizar el Documento Principal (btipo = 0) en T_Documento
        UPDATE [BD_RCPDOC].[dbo].[T_Documento]
        SET [iCodTipoDoc] = @iCodTipoDoc,
            [vNroDoc] = @vNroDoc,
            [dFecDoc] = @dFecDoc,
            [vReferencia] = @vReferencia,
            [vNroPagFolios] = @vNroPagFolios
        WHERE [iCodAsunto] = @iCodAsunto AND [btipo] = 0;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
GO