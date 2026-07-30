USE [BD_RCPDOC];
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
GO