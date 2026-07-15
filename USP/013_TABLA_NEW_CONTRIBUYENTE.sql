-- 1. Crear la tabla con un nombre más relacional (en singular)
CREATE TABLE T_Contribuyente (
    ruc CHAR(11) NOT NULL,
    nombre_razon_social VARCHAR(250) NOT NULL,
    estado_contribuyente VARCHAR(50) NULL,
    condicion_domicilio VARCHAR(50) NULL,
    ubigeo CHAR(6) NULL,
    tipo_via VARCHAR(50) NULL,
    nombre_via VARCHAR(150) NULL,
    codigo_zona VARCHAR(50) NULL,
    tipo_zona VARCHAR(50) NULL,
    numero VARCHAR(20) NULL,
    interior VARCHAR(20) NULL,
    lote VARCHAR(20) NULL,
    departamento VARCHAR(20) NULL,
    manzana VARCHAR(20) NULL,
    kilometro VARCHAR(20) NULL
);
GO

-- 2. Clave primaria e Índice Clúster en el RUC
ALTER TABLE T_Contribuyente
ADD CONSTRAINT PK_t_contribuyente PRIMARY KEY CLUSTERED (ruc);
GO

-- 3. Índice Nonclúster para búsquedas por Nombre o Razón Social
CREATE NONCLUSTERED INDEX IX_t_contribuyente_razon_social
ON t_Contribuyente (nombre_razon_social);
GO