USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_ActualizarDatosTramite]
    @iCodAsunto INT,
    @TipoTramite INT,
    @CorreoTramite VARCHAR(255),
    @RucTramite VARCHAR(11) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Validamos que el asunto exista
    IF NOT EXISTS (SELECT 1 FROM [BD_RCPDOC].[dbo].[T_Asunto] WHERE iCodAsunto = @iCodAsunto)
    BEGIN
        RAISERROR('El expediente o asunto no existe.', 16, 1);
        RETURN;
    END

    -- CASO 1: Persona Natural (Solo actualiza el correo en T_Asunto)
    IF @TipoTramite = 2
    BEGIN
        UPDATE [BD_RCPDOC].[dbo].[T_Asunto]
        SET vMailSeguimiento = @CorreoTramite
        WHERE iCodAsunto = @iCodAsunto;
    END

    -- CASO 2: Persona Jurídica (Actualiza RUC en T_Tramite y Correo en T_Asunto)
    ELSE IF @TipoTramite = 1
    BEGIN
        -- Actualizamos el correo en T_Asunto
        UPDATE [BD_RCPDOC].[dbo].[T_Asunto]
        SET vMailSeguimiento = @CorreoTramite
        WHERE iCodAsunto = @iCodAsunto;

        -- Actualizamos el RUC en T_Tramite vinculándolo a través del iCodAsunto
        UPDATE T
        SET T.vRUC = @RucTramite
        FROM [BD_RCPDOC].[dbo].[T_Tramite] T
        INNER JOIN [BD_RCPDOC].[dbo].[T_Asunto] A ON T.iCodAsunto = A.iCodAsunto
        WHERE A.iCodAsunto = @iCodAsunto;
    END
    
    ELSE
    BEGIN
        RAISERROR('El tipo de trámite no es válido.', 16, 1);
        RETURN;
    END
END
GO