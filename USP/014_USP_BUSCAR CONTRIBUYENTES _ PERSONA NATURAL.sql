-- 1. Aseguramos que estamos trabajando en la base de datos correcta
USE [BD_RCPDOC];
GO

-- =============================================
-- 1. Procedimiento para búsqueda genérica
-- =============================================
IF OBJECT_ID('dbo.usp_BuscarContribuyente', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BuscarContribuyente;
GO

CREATE PROCEDURE dbo.usp_BuscarContribuyente
    @valor VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(@valor) > 8
    BEGIN
        SELECT TOP 1 [ruc], [nombre_razon_social], [estado_contribuyente], [condicion_domicilio], [ubigeo]
        FROM [dbo].[T_Contribuyente]
        WHERE [ruc] = @valor;
    END
    ELSE
    BEGIN
        SELECT TOP 1 @valor AS [dni], [nombre_razon_social] AS [nombre]
        FROM [dbo].[T_Contribuyente]
        WHERE [ruc] LIKE '10' + @valor + '%';
    END
END
GO

-- =============================================
-- 2. Procedimiento para Persona Natural
-- =============================================
IF OBJECT_ID('dbo.usp_BuscarPersonaNatural', 'P') IS NOT NULL
    DROP PROCEDURE dbo.usp_BuscarPersonaNatural;
GO

CREATE PROCEDURE dbo.usp_BuscarPersonaNatural
    @dni VARCHAR(8)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 [ruc], [nombre_razon_social], [estado_contribuyente], [condicion_domicilio]
    FROM [dbo].[T_Contribuyente]
    WHERE [ruc] LIKE '10' + @dni + '%';
END
GO