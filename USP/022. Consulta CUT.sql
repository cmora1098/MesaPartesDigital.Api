USE BD_RCPDOC;
GO

-- Si ya existe previamente, puedes usar ALTER PROCEDURE
CREATE OR ALTER PROCEDURE dbo.USP_CONSULTA_CUT
    @intNroTramite INT,
    @intPeriodo    INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Ejecuta el SP alojado en la otra base de datos pasándole los mismos parámetros
    EXEC SISGED.dbo.GED_SP_SEL_CONSULTAR_TRAMITE 
        @intNroTramite = @intNroTramite, 
        @intPeriodo = @intPeriodo;
END
GO


USE BD_RCPDOC;
GO

-- Ejecutando el procedimiento puente con los valores solicitados
EXEC dbo.USP_CONSULTA_CUT 
    @intNroTramite = 2049, 
    @intPeriodo = 2021;
GO