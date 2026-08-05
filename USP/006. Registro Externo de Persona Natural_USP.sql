USE [BD_RCPDOC]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistroPersonaNatural]
    @iCodTipoDocPer INT,
    @vDocPer VARCHAR(20),
    @vNombres VARCHAR(100),
    @vApellidoPaterno VARCHAR(100),
    @vApellidoMaterno VARCHAR(100),
    @vEmail VARCHAR(150),
    @vTelefono VARCHAR(20),
    @vDireccion VARCHAR(255),
    @vCodDistrito VARCHAR(6),
    @iCodAsunto INT,          
    @vRutaDoc VARCHAR(MAX),    
    @iCodTipoDoc INT,          
    @vNroDoc VARCHAR(50),
    @dFecDoc DATE,
    @vNombreAsunto VARCHAR(MAX),
    @vReferencia VARCHAR(MAX),
    @vNroPagFolios VARCHAR(50),
    @btipo BIT, -- 0: Principal, 1: Anexo
    @bAceptaTerminos BIT,
    @bAceptaDatosPersonales BIT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @iCodPer INT;
    DECLARE @GeneratediCodDoc INT;
    DECLARE @vAutoGenerado VARCHAR(MAX);

    -- 0. Validación inicial de campos básicos
    IF (@vDocPer IS NULL OR @vNombres IS NULL OR @vEmail IS NULL)
    BEGIN
        RAISERROR('Error: Campos obligatorios vacíos (Doc, Nombres o Email).', 16, 1);
        RETURN;
    END

    -- Validación para anexos: Si es anexo (btipo=1), es obligatorio tener un iCodAsunto
    IF (@btipo = 1 AND @iCodAsunto IS NULL)
    BEGIN
        RAISERROR('Error: Para adjuntar un anexo (btipo=1), es obligatorio proporcionar un iCodAsunto válido.', 16, 1);
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        -- 1. Control del Remitente
        SELECT @iCodPer = iCodPer 
        FROM dbo.T_Persona WITH (UPDLOCK, ROWLOCK) 
        WHERE iCodTipoDocPer = @iCodTipoDocPer AND vDocPer = @vDocPer;

        IF (@iCodPer IS NULL)
        BEGIN
            INSERT INTO dbo.T_Persona (iCodTipoDocPer, vDocPer, vNombres, vApellidoPaterno, vApellidoMaterno, vEmail, vTelefono, bActivo, vDireccion, vCodDistrito, bCorreoVerificado)
            VALUES (@iCodTipoDocPer, @vDocPer, 
                    LEFT(UPPER(@vNombres), 50), 
                    LEFT(UPPER(@vApellidoPaterno), 50), 
                    LEFT(UPPER(@vApellidoMaterno), 50), 
                    LEFT(LOWER(@vEmail), 50), 
                    LEFT(@vTelefono, 50), 
                    1, 
                    LEFT(UPPER(@vDireccion), 250), 
                    LEFT(@vCodDistrito, 6), 
                    1);
            SET @iCodPer = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            UPDATE dbo.T_Persona
            SET vNombres = LEFT(UPPER(@vNombres), 50),
                vApellidoPaterno = LEFT(UPPER(@vApellidoPaterno), 50),
                vApellidoMaterno = LEFT(UPPER(@vApellidoMaterno), 50),
                vEmail = LEFT(LOWER(@vEmail), 50), 
                vTelefono = LEFT(@vTelefono, 50), 
                vDireccion = LEFT(UPPER(@vDireccion), 250),
                vCodDistrito = LEFT(@vCodDistrito, 6)
            WHERE iCodPer = @iCodPer;
        END

        -- 2. Control de T_Asunto (Solo si es documento principal, btipo=0)
        IF (@btipo = 0)
        BEGIN
            SET @vAutoGenerado = 'AGRORURAL_' + SUBSTRING(CONVERT(VARCHAR(50), NEWID()), 1, 8); 

            INSERT INTO dbo.T_Asunto (iCodEstado, vNombreAsunto, iCodPer, vMailSeguimiento, vAutoGenerado, bActivo, dtFechaCreacion)
            VALUES (1, LEFT(UPPER(@vNombreAsunto), 255), @iCodPer, LEFT(LOWER(@vEmail), 50), @vAutoGenerado, 1, GETDATE());
    
            SET @iCodAsunto = SCOPE_IDENTITY();

            -- ACTUALIZACIÓN: Se agregan los campos bAceptaTerminos y bAceptaDatosPersonales
            INSERT INTO dbo.T_Tramite (iCodTipoPer, iCodAsunto, vRUC, bAceptaTerminos, bAceptaDatosPersonales)
            VALUES (2, @iCodAsunto, NULL, @bAceptaTerminos, @bAceptaDatosPersonales);
        END

        -- 3. Inserción del Documento
        INSERT INTO dbo.T_Documento (iCodPer, iCodAsunto, vRutaDoc, iCodTipoDoc, vNroDoc, dFecDoc, dFecRecepcion, vReferencia, vNroPagFolios, bActivo, dtFechaCargaArchivo, btipo)
        VALUES (@iCodPer, @iCodAsunto, @vRutaDoc, @iCodTipoDoc, UPPER(@vNroDoc), @dFecDoc, GETDATE(), LEFT(UPPER(@vReferencia), 50), @vNroPagFolios, 1, GETDATE(), @btipo);

        SET @GeneratediCodDoc = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        -- Respuesta final
        SELECT @GeneratediCodDoc AS iCodDoc, @iCodAsunto AS iCodAsunto, @iCodPer AS iCodPer, 'OK' AS Status, LOWER(@vEmail) AS MailSeguimiento, @vAutoGenerado AS vAutoGenerado;   
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        
        DECLARE @ErrMsg NVARCHAR(4000) = 'Error en SP USP_RegistroPersonaNatural: ' + ERROR_MESSAGE();
        RAISERROR(@ErrMsg, 16, 1);
    END CATCH
END;
GO