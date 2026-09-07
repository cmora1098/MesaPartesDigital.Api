USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE dbo.T_Tramite (
    iCod_tramite INT IDENTITY(1,1) NOT NULL,
    iCodTipoPer INT NOT NULL,
    iCodAsunto INT NOT NULL,
    vRUC NCHAR(11) NULL,
    CONSTRAINT PK_T_Tramite PRIMARY KEY CLUSTERED (iCod_tramite)
);


-- Agregando los campos en formato BIT para los términos y el tratamiento de datos
ALTER TABLE dbo.T_Tramite
ADD 
    bAceptaTerminos BIT NOT NULL CONSTRAINT DF_T_Tramite_AceptaTerminos DEFAULT 0,
    bAceptaDatosPersonales BIT NOT NULL CONSTRAINT DF_T_Tramite_AceptaDatosPersonales DEFAULT 0;
GO