USE BD_RCPDOC;
GO

ALTER TABLE dbo.T_Asunto
ADD dtFechaSubsanacion DATETIME NULL;
GO

/****** Objeto: StoredProcedure [dbo].[USP_CambiarEstadoTramite] Fecha de script: 31/07/2026 09:12:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_CambiarEstadoTramite]
    @iCodAsunto INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Actualizamos el estado solo si actualmente se encuentra en 5
    UPDATE [dbo].[T_Asunto]
    SET iCodEstado = 1,
        dtFechaSubsanacion = GETDATE() -- Opcional: registra la fecha en que cambió a pendiente/activo si tu lógica lo requiere
    WHERE iCodAsunto = @iCodAsunto 
      AND iCodEstado = 5;

    -- Opcional: Puedes verificar si se afectó alguna fila para saber si la actualización fue exitosa
    IF @@ROWCOUNT > 0
    BEGIN
        SELECT 1 AS Resultado; -- Éxito
    END
    ELSE
    BEGIN
        SELECT 0 AS Resultado; -- No se actualizó (quizás el asunto no existe o no estaba en estado 5)
    END
END
