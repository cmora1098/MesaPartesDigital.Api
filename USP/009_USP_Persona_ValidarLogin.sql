CREATE OR ALTER PROCEDURE [dbo].[USP_Persona_ValidarLogin]
    @vDocPer VARCHAR(20),
    @vPassword VARCHAR(64) 
AS
BEGIN
    SET NOCOUNT ON; 
    
    IF EXISTS (SELECT 1 FROM [BD_RCPDOC].[dbo].[T_Persona] WHERE [vDocPer] = @vDocPer AND [vPassword] = @vPassword)
    BEGIN
        SELECT 
            1 AS Exitoso, 
            'Acceso concedido' AS Mensaje,
            [iCodPer],
            ([vNombres] + ' ' + [vApellidoPaterno] + ' ' + [vApellidoMaterno]) AS vNombreCompleto,
            [vEmail] 
        FROM [BD_RCPDOC].[dbo].[T_Persona]
        WHERE [vDocPer] = @vDocPer AND [vPassword] = @vPassword;
    END
    ELSE
    BEGIN
        -- Estado de error estructurado con las mismas columnas que el bloque exitoso
        SELECT 
            0 AS Exitoso, 
            'DNI o contraseña incorrectos' AS Mensaje,
            0 AS [iCodPer],
            '' AS vNombreCompleto,
            '' AS [vEmail];
    END
END
GO