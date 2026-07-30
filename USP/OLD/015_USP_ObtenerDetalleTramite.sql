USE [BD_RCPDOC]
GO
/****** Object:  StoredProcedure [dbo].[USP_ObtenerDetalleTramite]    Script Date: 21/07/2026 11:59:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE [dbo].[USP_ObtenerDetalleTramite]
    @iCodAsunto INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
			A.iCodAsunto,
			A.vAutoGenerado AS CodigoTramite,
			A.vNombreAsunto AS AsuntoTramite,
			E.vNombreEstado AS EstadoTramite,
			A.[vCut] AS CUTTramite,
			A.[iCodDependencia] AS CodigoDependencia,
			DP.TXT_DESCRIPCION AS NombreDependencia,
			A.dtFechaCreacion AS Fecha,
			A.vMailSeguimiento AS CorreoTramite,
			-- DOCUMENTO
			D.iCodDoc,
			D.iCodTipoDoc,
			D.vNroDoc AS NroDocumento,
			D.vNroPagFolios AS FoliosDocumento,
			D.dFecDoc AS FechaDocumento,
			D.vReferencia AS RefDocumento,
			D.vRutaDoc AS RutaDocumento,
			-- TRAMITE
			T.iCodTipoPer AS TipoTramite,
			T.vRUC AS RucTramite 
   FROM [BD_RCPDOC].[dbo].[T_Documento] D
	INNER JOIN [BD_RCPDOC].[dbo].[T_Asunto] A ON D.iCodAsunto = A.iCodAsunto
	INNER JOIN [BD_RCPDOC].[dbo].[T_TipoDocumento] TD ON D.iCodTipoDoc = TD.iCodTipoDoc
	INNER JOIN [BD_RCPDOC].[dbo].[T_Estado] E ON A.iCodEstado = E.iCodEstado
	LEFT JOIN [BD_RCPDOC].[dbo].[T_Tramite] T ON A.iCodAsunto = T.iCodAsunto
	LEFT JOIN [SISGED].[dbo].[GED_TG_DEPENDENCIA] DP ON A.iCodDependencia = DP.ide_dependencia AND DP.IDE_ENTIDAD = 7
    WHERE  D.btipo = 0 AND D.bActivo= 1 AND
	A.iCodAsunto = @iCodAsunto;
END
GO
 