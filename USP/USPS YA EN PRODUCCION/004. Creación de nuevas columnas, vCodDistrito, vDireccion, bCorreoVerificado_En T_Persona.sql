USE [BD_RCPDOC]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Persona') AND name = 'vCodDistrito')
BEGIN
    ALTER TABLE dbo.T_Persona ADD [vCodDistrito] CHAR(6) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Persona') AND name = 'vDireccion')
BEGIN
    ALTER TABLE dbo.T_Persona ADD [vDireccion] VARCHAR(250) NULL;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Persona') AND name = 'bCorreoVerificado')
BEGIN
    ALTER TABLE dbo.T_Persona ADD [bCorreoVerificado] BIT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('dbo.T_Persona') AND name = 'iCodPerRepresentante')
BEGIN
    ALTER TABLE dbo.T_Persona ADD [iCodPerRepresentante] INT NULL;
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID('dbo.FK_T_Persona_Representante'))
BEGIN
    ALTER TABLE dbo.T_Persona  WITH CHECK ADD CONSTRAINT [FK_T_Persona_Representante] FOREIGN KEY([iCodPerRepresentante])
    REFERENCES dbo.T_Persona ([iCodPer]);

    ALTER TABLE dbo.T_Persona CHECK CONSTRAINT [FK_T_Persona_Representante];
END
GO
