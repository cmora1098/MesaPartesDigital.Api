USE [BD_RCPDOC]
GO
/****** Object:  StoredProcedure [dbo].[USP_RegistroTramiteInternoPersonaJuridica] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistroTramiteInternoPersonaJuridica]
    -- PARÁMETROS DE SESIÓN DEL USUARIO LOGEADO (Desde Blazor)
    @iCodPerUsuario INT,
    @vEmailUsuario VARCHAR(150),

    -- I. DATOS DE LA EMPRESA  
    @vRucEmpresa VARCHAR(20),
    @vRazonSocial VARCHAR(255),

    -- II. DATOS DEL DOCUMENTO 
    @iCodAsunto INT,          
    @vRutaDoc VARCHAR(MAX),    
    @iCodTipoDoc INT,         
    @vNroDoc VARCHAR(50),
    @dFecDoc DATE,
    @vReferencia VARCHAR(MAX),
    @vNroPagFolios VARCHAR(50),
    @btipo BIT,
    @vLink VARCHAR(MAX) = NULL 
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @iCodPerEmpresa INT;
    DECLARE @GeneratediCodDoc INT;
    DECLARE @vAutoGenerado VARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ==========================================
        -- 1. CONTROL DE LA EMPRESA (Persona Jurídica)
        -- ==========================================
        DECLARE @iCodTipoDocRUC INT = 2; 

        -- Verificamos si la empresa con ese RUC ya existe
        SELECT @iCodPerEmpresa = iCodPer 
        FROM dbo.T_Persona 
        WHERE iCodTipoDocPer = @iCodTipoDocRUC AND vDocPer = @vRucEmpresa;

        IF (@iCodPerEmpresa IS NULL)
        BEGIN
            INSERT INTO dbo.T_Persona (
                iCodTipoPer, 
                iCodTipoDocPer, 
                vDocPer, 
                vNombres,          
                vEmpresa,          
                vEmail,            
                bActivo, 
                bCorreoVerificado,
                iCodPerRepresentante -- Se vincula al usuario logeado actual
            )
            VALUES (
                2, -- 2 = Persona Jurídica
                @iCodTipoDocRUC, 
                @vRucEmpresa, 
                NULL, 
                UPPER(@vRazonSocial), 
                '', 
                1, 
                0,
                @iCodPerUsuario
            );
            SET @iCodPerEmpresa = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Si la empresa ya existe, actualizamos su Razón Social y vinculamos al usuario logeado como gestor actual
            UPDATE dbo.T_Persona
            SET vEmpresa = UPPER(@vRazonSocial),
                iCodPerRepresentante = @iCodPerUsuario
            WHERE iCodPer = @iCodPerEmpresa;
        END

        -- ==========================================
        -- 2. CONTROL DE T_ASUNTO
        -- ==========================================
        IF (@btipo = 1)
        BEGIN
            SET @vAutoGenerado = 'AGRORURAL_' + SUBSTRING(CONVERT(VARCHAR(50), NEWID()), 1, 8);
            
            INSERT INTO dbo.T_Asunto (
                iCodEstado,
                vNombreAsunto,
                iCodPer,            -- El expediente se asienta a nombre de la EMPRESA
                vMailSeguimiento,   -- Correo del usuario logeado para alertas y acuses
                vAutoGenerado,
                bActivo,
                dtFechaCreacion
            )
            VALUES (
                1, 
                UPPER(@vReferencia), 
                @iCodPerEmpresa,    
                UPPER(@vEmailUsuario),  
                @vAutoGenerado,
                1,
                GETDATE()
            );
            
            SET @iCodAsunto = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            SET @vAutoGenerado = NULL; 
        END

        -- ==========================================
        -- 3. INSERCIÓN EN T_DOCUMENTO
        -- ==========================================
        INSERT INTO dbo.T_Documento (
            iCodPer,                -- El documento pertenece formalmente a la Empresa
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
        VALUES (
            @iCodPerEmpresa,       
            @iCodAsunto,       
            @vRutaDoc, 
            @iCodTipoDoc,
            UPPER(@vNroDoc),
            @dFecDoc,
            GETDATE(),         
            UPPER(@vReferencia),
            @vNroPagFolios,
            @vLink,            
            1,
            GETDATE(),
            @btipo
        );

        SET @GeneratediCodDoc = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        -- ==========================================
        -- RETORNO EN FORMATO BLAZOR 
        -- ==========================================
        SELECT 
            @GeneratediCodDoc AS iCodDoc, 
            @iCodAsunto AS iCodAsunto, 
            'OK' AS Status,
            UPPER(@vEmailUsuario) AS MailSeguimiento,  
            @vAutoGenerado AS vAutoGenerado;     

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO