USE [BD_RCPDOC]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistroTramiteInternoPersonaNatural]
    @iCodPer INT,               
    @vEmail VARCHAR(150),       
    @iCodAsunto INT OUTPUT,     
    @vRutaDoc VARCHAR(MAX),
    @iCodTipoDoc INT,
    @vNroDoc VARCHAR(50),
    @dFecDoc DATE,
    @vNombreAsunto VARCHAR(MAX),
    @vReferencia VARCHAR(MAX),  
    @vNroPagFolios VARCHAR(50),
    @btipo BIT,                 
    @vLink VARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @GeneratediCodDoc INT;
    DECLARE @vAutoGenerado VARCHAR(MAX) = NULL;

    BEGIN TRY
        BEGIN TRANSACTION;

        IF (@btipo = 0)
        BEGIN
            SET @vAutoGenerado = 'AGRORURAL_' + SUBSTRING(CONVERT(VARCHAR(50), NEWID()), 1, 8);
            
            INSERT INTO dbo.T_Asunto (iCodEstado, vNombreAsunto, iCodPer, vMailSeguimiento, vAutoGenerado, bActivo, dtFechaCreacion)
            VALUES (1, UPPER(@vNombreAsunto), @iCodPer, LOWER(@vEmail), @vAutoGenerado, 1, GETDATE());
            
            SET @iCodAsunto = SCOPE_IDENTITY();

            INSERT INTO dbo.T_Tramite (iCodTipoPer, iCodAsunto, vRUC)
            VALUES (2, @iCodAsunto, NULL);
        END

        INSERT INTO dbo.T_Documento (iCodPer, iCodAsunto, vRutaDoc, iCodTipoDoc, vNroDoc, dFecDoc, dFecRecepcion, vReferencia, vNroPagFolios, vLink, bActivo, dtFechaCargaArchivo, btipo)
        VALUES (@iCodPer, @iCodAsunto, @vRutaDoc, @iCodTipoDoc, UPPER(@vNroDoc), @dFecDoc, GETDATE(), UPPER(@vReferencia), @vNroPagFolios, @vLink, 1, GETDATE(), @btipo);

        SET @GeneratediCodDoc = SCOPE_IDENTITY();

        -- RECUPERAR AUTOGENERADO SI ES ANEXO
        IF @vAutoGenerado IS NULL
        BEGIN
            SELECT @vAutoGenerado = vAutoGenerado FROM dbo.T_Asunto WHERE iCodAsunto = @iCodAsunto;
        END

        COMMIT TRANSACTION;

        SELECT @GeneratediCodDoc AS iCodDoc, @iCodAsunto AS iCodAsunto, 'OK' AS Status, LOWER(@vEmail) AS MailSeguimiento, ISNULL(@vAutoGenerado, '') AS vAutoGenerado;       
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = 'Error al registrar trámite: ' + ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;