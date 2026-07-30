USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
-- 1. Creamos el procedimiento desde cero con todos los alias de columna corregidos
CREATE OR ALTER PROCEDURE [dbo].[USP_Persona_GestionarCredenciales]
    @vDocPer VARCHAR(20), 
    @vCodigoOTP VARCHAR(10) = NULL,
    @iTipoAccion INT 
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @NewPassword VARCHAR(12);
    DECLARE @iCodPer INT;
    DECLARE @bCorreoVerificado BIT;
    DECLARE @bActivo BIT;

    -- Buscar la persona por DNI en lugar de email
    SELECT @iCodPer = [iCodPer], 
           @bCorreoVerificado = [bCorreoVerificado],
           @bActivo = [bActivo]
    FROM [BD_RCPDOC].[dbo].[T_Persona]
    WHERE [vDocPer] = @vDocPer; -- Cambio principal

    -- Si no existe el usuario, retornar error
    IF @iCodPer IS NULL
    BEGIN
        SELECT 0 AS Exitoso, 'El número de documento (DNI) no se encuentra registrado.' AS Mensaje, NULL AS PasswordGenerado;
        RETURN;
    END

    SET @NewPassword = SUBSTRING(REPLACE(CAST(NEWID() AS VARCHAR(50)), '-', ''), 1, 8);

    -- Acción 1: Validar OTP
    IF @iTipoAccion = 1
    BEGIN
        IF @bCorreoVerificado = 1
        BEGIN
            SELECT 0 AS Exitoso, 'Esta cuenta ya se encuentra activada.', NULL AS PasswordGenerado;
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM [BD_RCPDOC].[dbo].[T_Persona] WHERE [iCodPer] = @iCodPer AND [vCodigoRecup] = @vCodigoOTP)
        BEGIN
            UPDATE [BD_RCPDOC].[dbo].[T_Persona]
            SET [bCorreoVerificado] = 1,
                [bActivo] = 1,
                [vPassword] = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @NewPassword), 2)),
                [vCodigoRecup] = NULL        
            WHERE [iCodPer] = @iCodPer;

            SELECT 1 AS Exitoso, 'Cuenta verificada con éxito. Se ha enviado su contraseña al correo registrado.' AS Mensaje, @NewPassword AS PasswordGenerado;
        END
        ELSE
        BEGIN
            SELECT 0 AS Exitoso, 'El código de verificación es incorrecto.' AS Mensaje, NULL AS PasswordGenerado;
        END
    END

    -- Acción 2: Generar Clave / Recuperar
    ELSE IF @iTipoAccion = 2
    BEGIN
        UPDATE [BD_RCPDOC].[dbo].[T_Persona]
        SET [vPassword] = LOWER(CONVERT(VARCHAR(64), HASHBYTES('SHA2_256', @NewPassword), 2)),
            [bCorreoVerificado] = 1,
            [bActivo] = 1            
        WHERE [iCodPer] = @iCodPer;

        SELECT 1 AS Exitoso, 'Se ha generado la clave y activado la cuenta con éxito.' AS Mensaje, @NewPassword AS PasswordGenerado;
    END
END
