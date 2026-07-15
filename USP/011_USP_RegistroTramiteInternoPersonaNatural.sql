USE [BD_RCPDOC]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_RegistroTramiteInternoPersonaNatural]
    -- 1. Identificación del Remitente (Ya obtenido de la sesión del usuario)
    @iCodPer INT,               -- ID único de la persona logueada
    @vEmail VARCHAR(150),       -- Email de la sesión para las alertas/seguimiento

    -- 2. Datos específicos de este Archivo (Principal o Anexo)
    @iCodAsunto INT,          -- Recibe 0 si es Principal (Padre), o el ID generado si es Anexo (Hijo)
    @vRutaDoc VARCHAR(MAX),   -- Ruta física del archivo guardado en el servidor
    @iCodTipoDoc INT,         -- 1 = Informe, 2 = Memo, 3 = Carta, etc.
    @vNroDoc VARCHAR(50),     -- Número correlativo del documento escrito
    @dFecDoc DATE,            -- Fecha de emisión del documento
    @vReferencia VARCHAR(MAX),-- Sumilla / Asunto del trámite
    @vNroPagFolios VARCHAR(50),-- Cantidad de folios
    @btipo BIT,               -- 1 = Principal (Padre), 0 = Anexo (Hijo)
    @vLink VARCHAR(MAX) = NULL-- URL externa opcional
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @GeneratediCodDoc INT;
    DECLARE @vAutoGenerado VARCHAR(MAX);

    BEGIN TRY
        BEGIN TRANSACTION;

        -- ====================================================================
        -- 1. CONTROL DE T_ASUNTO Y GENERACIÓN DEL CÓDIGO DE TRÁMITE (PADRE)
        -- ====================================================================
        IF (@btipo = 1)
        BEGIN
            -- Generamos el correlativo único para el trámite principal
            SET @vAutoGenerado = 'AGRORURAL_' + SUBSTRING(CONVERT(VARCHAR(50), NEWID()), 1, 8);
            
            -- Insertar la cabecera del expediente vinculándolo directamente al @iCodPer provisto
            INSERT INTO dbo.T_Asunto (
                iCodEstado,
                vNombreAsunto,
                iCodPer,
                vMailSeguimiento,
                vAutoGenerado,
                bActivo,
                dtFechaCreacion
            )
            VALUES (
                1,                    -- Estado inicial: Registrado
                UPPER(@vReferencia),  -- Usamos la sumilla/asunto de la UI
                @iCodPer,             -- Usuario logueado
                UPPER(@vEmail),       -- Email de seguimiento
                @vAutoGenerado,
                1,
                GETDATE()
            );
            
            -- Recuperamos el ID recién generado para usarlo en el documento principal y anexos
            SET @iCodAsunto = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- Si btipo = 0 (Anexo), no crea cabecera. Mantiene el @iCodAsunto enviado desde Blazor
            SET @vAutoGenerado = NULL; 
        END

        -- ====================================================================
        -- 2. INSERCIÓN DEL REGISTRO DE ARCHIVO EN dbo.T_Documento
        -- ====================================================================
        INSERT INTO dbo.T_Documento (
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
        VALUES (
            @iCodPer,
            @iCodAsunto,       -- Enlazado al ID del expediente (Generado arriba o recibido por parámetro)
            @vRutaDoc, 
            @iCodTipoDoc,
            UPPER(@vNroDoc),
            @dFecDoc,
            GETDATE(),         -- Fecha y hora del servidor al momento de la recepción
            UPPER(@vReferencia),
            @vNroPagFolios,
            @vLink,            
            1,
            GETDATE(),
            @btipo
        );

        SET @GeneratediCodDoc = SCOPE_IDENTITY();

        COMMIT TRANSACTION;

        -- ====================================================================
        -- RETORNO ESPERADO POR EL DOCUMENTOSERVICE EN C#
        -- ====================================================================
        SELECT 
            @GeneratediCodDoc AS iCodDoc, 
            @iCodAsunto AS iCodAsunto, 
            'OK' AS Status,
            UPPER(@vEmail) AS MailSeguimiento,  
            @vAutoGenerado AS vAutoGenerado;     

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END;
GO