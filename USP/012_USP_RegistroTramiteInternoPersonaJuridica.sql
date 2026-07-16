USE [BD_RCPDOC]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistroTramiteInternoPersonaJuridica]
    -- PARÁMETROS DE SESIÓN DEL USUARIO LOGEADO
    @iCodPerUsuario INT,
    @vEmailUsuario VARCHAR(150),

    -- I. DATOS DE LA EMPRESA
    @vRucEmpresa VARCHAR(20),
    @vRazonSocial VARCHAR(255),

    -- II. DATOS DEL DOCUMENTO
    @iCodAsunto INT OUTPUT, -- Definido como OUTPUT para actualizar el ID en el bucle
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
    SET XACT_ABORT ON;
    
    DECLARE @iCodPerEmpresa INT;
    DECLARE @GeneratediCodDoc INT;
    DECLARE @vAutoGenerado VARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. CONTROL DE LA EMPRESA (Persona Jurídica)
        DECLARE @iCodTipoDocRUC INT = 2; 

        SELECT @iCodPerEmpresa = iCodPer 
        FROM dbo.T_Persona 
        WHERE iCodTipoDocPer = @iCodTipoDocRUC AND vDocPer = @vRucEmpresa;

        IF (@iCodPerEmpresa IS NULL)
        BEGIN
            INSERT INTO dbo.T_Persona (iCodTipoPer, iCodTipoDocPer, vDocPer, vEmpresa, vEmail, bActivo, bCorreoVerificado, iCodPerRepresentante, vPassword)
            VALUES (2, @iCodTipoDocRUC, @vRucEmpresa, UPPER(@vRazonSocial), '', 1, 0, @iCodPerUsuario, 'NO_APLICA');
            SET @iCodPerEmpresa = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.T_Persona
            SET vEmpresa = UPPER(@vRazonSocial), iCodPerRepresentante = @iCodPerUsuario
            WHERE iCodPer = @iCodPerEmpresa;
        END

        -- 2. CONTROL DE T_ASUNTO Y T_TRAMITE
        -- Solo se crea el Asunto si @iCodAsunto es 0 (indica documento principal)
        IF (@iCodAsunto = 0 AND @btipo = 1)
        BEGIN
            SET @vAutoGenerado = 'AGRORURAL_' + SUBSTRING(CONVERT(VARCHAR(50), NEWID()), 1, 8);
            
            INSERT INTO dbo.T_Asunto (iCodEstado, vNombreAsunto, iCodPer, vMailSeguimiento, vAutoGenerado, bActivo, dtFechaCreacion)
            VALUES (1, UPPER(@vNombreAsunto), @iCodPerEmpresa, UPPER(@vEmailUsuario), @vAutoGenerado, 1, GETDATE());
            
            SET @iCodAsunto = SCOPE_IDENTITY();

            INSERT INTO dbo.T_Tramite (iCodTipoPer, iCodAsunto, vRUC)
            VALUES (2, @iCodAsunto, @vRucEmpresa);
        END

        -- 3. INSERCIÓN EN T_DOCUMENTO
        INSERT INTO dbo.T_Documento (
            iCodPer, iCodAsunto, vRutaDoc, iCodTipoDoc, vNroDoc, 
            dFecDoc, dFecRecepcion, vReferencia, vNroPagFolios, 
            vLink, bActivo, dtFechaCargaArchivo, btipo
        )
        VALUES (
            @iCodPerEmpresa, @iCodAsunto, @vRutaDoc, @iCodTipoDoc, UPPER(@vNroDoc), 
            @dFecDoc, GETDATE(), UPPER(@vReferencia), LEFT(@vNroPagFolios, 50), 
            @vLink, 1, GETDATE(), @btipo
        );

        SET @GeneratediCodDoc = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        -- Retorno de resultados
        SELECT @GeneratediCodDoc AS iCodDoc, @iCodAsunto AS iCodAsunto, 'OK' AS Status, UPPER(@vEmailUsuario) AS MailSeguimiento, @vAutoGenerado AS vAutoGenerado;    

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrMsg NVARCHAR(4000) = 'Error en USP_RegistroTramiteInternoPersonaJuridica: ' + ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END;
GO