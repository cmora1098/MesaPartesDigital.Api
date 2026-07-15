USE [BD_RCPDOC]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ==========================================================================================
-- Autor:       Mesa de Partes Digital
-- Fecha:       14/07/2026
-- Descripción: Obtiene el nombre/razón social de un contribuyente mediante su número de RUC
--              consultando la tabla [dbo].[T_Contribuyente].
-- ==========================================================================================
ALTER PROCEDURE [dbo].[USP_PersonaJuridica_ObtenerPorRUC]
    @vRuc NVARCHAR(11)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        [ruc],
        [nombre_razon_social]
    FROM 
        [dbo].[T_Contribuyente] WITH (NOLOCK)
    WHERE 
        [ruc] = @vRuc;
END;
GO