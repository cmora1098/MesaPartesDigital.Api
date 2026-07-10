USE [SISGED];
GO

/* Compatible con SQL Server 2012 o superior. */
IF OBJECT_ID(N'[dbo].[USP_UBIGEO_LISTAR]', N'P') IS NULL
BEGIN
    EXEC(N'CREATE PROCEDURE [dbo].[USP_UBIGEO_LISTAR] AS BEGIN SET NOCOUNT ON; END');
END;
GO

ALTER PROCEDURE [dbo].[USP_UBIGEO_LISTAR]
    @TIPO_LISTADO       TINYINT,
    @COD_UBIGEO_PADRE   CHAR(6) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    /*
        @TIPO_LISTADO:
        1 = Departamentos
        2 = Provincias de un departamento
        3 = Distritos de una provincia
    */

    IF @TIPO_LISTADO NOT IN (1, 2, 3)
    BEGIN
        THROW 50001, 'El tipo de listado debe ser 1, 2 o 3.', 1;
    END;

    IF @TIPO_LISTADO IN (2, 3)
       AND (NULLIF(LTRIM(RTRIM(@COD_UBIGEO_PADRE)), '') IS NULL
            OR LEN(@COD_UBIGEO_PADRE) <> 6)
    BEGIN
        THROW 50002, 'Debe indicar un código de ubigeo padre válido de 6 caracteres.', 1;
    END;

    IF @TIPO_LISTADO = 1
    BEGIN
        SELECT
            U.COD_UBIGEO AS vCodDepartamento,
            U.TXT_UBIGEO AS vNomDepartamento,
            CAST(0 AS DECIMAL(18, 8)) AS dcLongitud,
            CAST(0 AS DECIMAL(18, 8)) AS dcLatitud
        FROM dbo.GED_TG_UBIGEO AS U
        WHERE U.FLG_ACTIVO = 1
          AND U.COD_UBIGEO <> '000000'
          AND U.COD_UBIGEO_PADRE IS NULL
          AND RIGHT(U.COD_UBIGEO, 4) = '0000'
        ORDER BY U.TXT_UBIGEO;

        RETURN;
    END;

    IF @TIPO_LISTADO = 2
    BEGIN
        SELECT
            U.COD_UBIGEO_PADRE AS vCodDepartamento,
            U.COD_UBIGEO AS vCodProvincia,
            U.TXT_UBIGEO AS vNomProvincia,
            CAST(0 AS DECIMAL(18, 8)) AS dcLongitud,
            CAST(0 AS DECIMAL(18, 8)) AS dcLatitud
        FROM dbo.GED_TG_UBIGEO AS U
        WHERE U.FLG_ACTIVO = 1
          AND U.COD_UBIGEO_PADRE = @COD_UBIGEO_PADRE
          AND RIGHT(U.COD_UBIGEO, 2) = '00'
        ORDER BY U.TXT_UBIGEO;

        RETURN;
    END;

    SELECT
        LEFT(U.COD_UBIGEO, 2) + '0000' AS vCodDepartamento,
        U.COD_UBIGEO_PADRE AS vCodProvincia,
        U.COD_UBIGEO AS vCodDistrito,
        U.TXT_UBIGEO AS vNomDistrito,
        CAST(0 AS DECIMAL(18, 8)) AS dcLongitud,
        CAST(0 AS DECIMAL(18, 8)) AS dcLatitud
    FROM dbo.GED_TG_UBIGEO AS U
    WHERE U.FLG_ACTIVO = 1
      AND U.COD_UBIGEO_PADRE = @COD_UBIGEO_PADRE
      AND RIGHT(U.COD_UBIGEO, 2) <> '00'
    ORDER BY U.TXT_UBIGEO;
END;
GO

-- Ejemplos:
-- EXEC dbo.USP_UBIGEO_LISTAR @TIPO_LISTADO = 1;
-- EXEC dbo.USP_UBIGEO_LISTAR @TIPO_LISTADO = 2, @COD_UBIGEO_PADRE = '010000';
-- EXEC dbo.USP_UBIGEO_LISTAR @TIPO_LISTADO = 3, @COD_UBIGEO_PADRE = '010100';
