USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE [dbo].[USP_Persona_Listar]
    @iCodPer INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.[iCodPer],
        p.[iCodTipoPer],
        p.[iCodTipoDocPer],
        p.[vDocPer],
        p.[vNombres],
        p.[vApellidoPaterno],
        p.[vApellidoMaterno],        
        (p.[vNombres] + ' ' + p.[vApellidoPaterno] + ' ' + p.[vApellidoMaterno]) AS vNombreCompleto,
        p.[vEmail],
        p.[vTelefono],
        p.[vEmpresa],
        p.[bActivo],
        p.[vCodDistrito],
        p.[vDireccion],
        p.[bCorreoVerificado],
        p.[iCodPerRepresentante],
        -- Agregamos los códigos seccionados directamente desde la BD
        SUBSTRING(p.[vCodDistrito], 1, 2) AS vCodDepartamento,
        SUBSTRING(p.[vCodDistrito], 1, 4) AS vCodProvincia
    FROM [dbo].[T_Persona] p
    WHERE p.[iCodPer] = @iCodPer;
END
GO