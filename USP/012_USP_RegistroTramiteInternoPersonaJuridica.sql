USE [BD_RCPDOC]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistroTramiteInternoPersonaJuridica]
    -- PARÁMETROS DE SESIÓN
	 @iCodPer INT,               
     @vEmail VARCHAR(150),

	-- I. DATOS DE LA EMPRESA
    @vRucEmpresa VARCHAR(20),
	
    -- II. DATOS DEL DOCUMENTO
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

        -- 1. Lógica de Asunto y Trámite (Solo si es documento principal)
        IF (@btipo = 1)
        BEGIN
            SET @vAutoGenerado = 'AGRORURAL_' + SUBSTRING(CONVERT(VARCHAR(50), NEWID()), 1, 8);
            
            -- Insertar cabecera usando el @vNombreAsunto que viene del formulario
            INSERT INTO dbo.T_Asunto (
                iCodEstado, vNombreAsunto, iCodPer, vMailSeguimiento, vAutoGenerado, bActivo, dtFechaCreacion
            )
            VALUES (
                1, UPPER(@vNombreAsunto), @iCodPer, LOWER(@vEmail), @vAutoGenerado, 1, GETDATE()
            );
            
            SET @iCodAsunto = SCOPE_IDENTITY();

            -- Registrar en T_Tramite
            INSERT INTO dbo.T_Tramite (iCodTipoPer, iCodAsunto, vRUC)
            VALUES (2, @iCodAsunto, @vRucEmpresa);
        END
		 -- 2. Inserción del Documento (Incluye todos los campos de tu lista)
        INSERT INTO dbo.T_Documento (
            iCodPer, 
            iCodAsunto, 
            vRutaDoc, 
            iCodTipoDoc, 
            vNroDoc, 
            dFecDoc, 
            dFecRecepcion, 
            vReferencia,     -- La referencia específica del documento
            vNroPagFolios, 
            vLink, 
            bActivo, 
            dtFechaCargaArchivo, 
            btipo
        )
        VALUES (
            @iCodPer, 
            @iCodAsunto, 
            @vRutaDoc, 
            @iCodTipoDoc, 
            UPPER(@vNroDoc), 
            @dFecDoc, 
            GETDATE(), 
            UPPER(@vReferencia), -- Mapeado correctamente
            @vNroPagFolios, 
            @vLink, 
            1, 
            GETDATE(), 
            @btipo
        );

        SET @GeneratediCodDoc = SCOPE_IDENTITY();

        COMMIT TRANSACTION;
		 -- Retorno al Service
        SELECT 
            @GeneratediCodDoc AS iCodDoc, 
            @iCodAsunto AS iCodAsunto, 
            'OK' AS Status,
            LOWER(@vEmail) AS MailSeguimiento, 
            @vAutoGenerado AS vAutoGenerado;       
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = 'Error al registrar trámite: ' + ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO