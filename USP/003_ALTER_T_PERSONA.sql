USE [BD_RCPDOC];
GO

-- 1. Cambiar columnas a NULL solo si es necesario
ALTER TABLE dbo.T_Persona ALTER COLUMN vNombres varchar(50) NULL;
ALTER TABLE dbo.T_Persona ALTER COLUMN vApellidoPaterno varchar(50) NULL;
ALTER TABLE dbo.T_Persona ALTER COLUMN vApellidoMaterno varchar(50) NULL;
ALTER TABLE dbo.T_Persona ALTER COLUMN vEmpresa varchar(50) NULL;
ALTER TABLE dbo.T_Persona ALTER COLUMN vTelefono varchar(50) NULL;
GO

-- 2. Agregar nuevas columnas validando que no existan previamente
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