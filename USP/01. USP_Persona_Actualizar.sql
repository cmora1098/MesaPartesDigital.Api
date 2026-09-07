CREATE OR ALTER PROCEDURE [dbo].[USP_Persona_Actualizar]
    @iCodPer INT,
    @vNombres VARCHAR(200),
    @vApellidoPaterno VARCHAR(100),
    @vApellidoMaterno VARCHAR(100),
    @vEmail VARCHAR(200),
    @vTelefono VARCHAR(50),
    @vCodDistrito VARCHAR(10),
    @vDireccion VARCHAR(500),
    @bCorreoVerificado BIT -- <-- Añadir este parámetro
AS
BEGIN
    SET NOCOUNT OFF;

    UPDATE [dbo].[T_Persona]
    SET
        [vNombres] = @vNombres,
        [vApellidoPaterno] = @vApellidoPaterno,
        [vApellidoMaterno] = @vApellidoMaterno,
        [vEmail] = @vEmail,
        [vTelefono] = @vTelefono,
        [vCodDistrito] = @vCodDistrito,
        [vDireccion] = @vDireccion,
        [bCorreoVerificado] = @bCorreoVerificado
    WHERE [iCodPer] = @iCodPer;
END
GO