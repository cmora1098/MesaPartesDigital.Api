CREATE OR ALTER PROCEDURE [dbo].[USP_Persona_ValidarLogin]
    @vDocPer VARCHAR(20),  -- Cambiado de @vEmail
    @vPassword VARCHAR(64) 
AS
BEGIN
    SET NOCOUNT ON; 
    
    -- Verificamos si existe el registro con esas credenciales
    IF EXISTS (SELECT 1 FROM [BD_RCPDOC].[dbo].[T_Persona] WHERE [vDocPer] = @vDocPer AND [vPassword] = @vPassword)
    BEGIN
        SELECT 
            1 AS Exitoso, 
            'Acceso concedido' AS Mensaje,
            [iCodPer],
            ([vNombres] + ' ' + [vApellidoPaterno] + ' ' + [vApellidoMaterno]) AS vNombreCompleto,
            [vEmail] -- Mantenemos el correo por si lo necesitas en el frontend
        FROM [BD_RCPDOC].[dbo].[T_Persona]
        WHERE [vDocPer] = @vDocPer AND [vPassword] = @vPassword;
    END
    ELSE
    BEGIN
        -- Retornamos un estado de error estructurado para que el API no falle al leer
        SELECT 
            0 AS Exitoso, 
            'DNI o contraseña incorrectos' AS Mensaje,
            NULL AS [iCodPer],
            NULL AS vNombreCompleto,
            NULL AS [vEmail];
    END
END
GO