USE TermoMatic
GO

DECLARE @cmdSQL NVARCHAR(MAX)
DECLARE @tAuxi TemperaturaSinProcesar

SET @cmdSQL = (
	SELECT 
		STRING_AGG(CAST('SELECT ' AS VARCHAR(MAX)) + QUOTENAME(TRIM(NOMB_CAMPO), '''') + ', ' + QUOTENAME(TRIM(NOMB_TALA)) + ', [CAMPO2] FROM OFFAL_DB.dbo.temp2018', ' UNION ALL ')
	FROM OFFAL_DB.dbo.DESCRIPCIONES_CAMPOS
	WHERE NOMB_CAMPO NOT IN ('SIN USO', 'KEY', 'FECHA')
)

INSERT INTO @tAuxi
EXECUTE sp_executesql @cmdSQL

UPDATE @tAuxi
SET FechaRegistro = REPLACE(REPLACE(FechaRegistro, 'a.m.', 'AM'), 'p.m.', 'PM') 
WHERE ISDATE(FechaRegistro) = 0

EXECUTE registro.InsertarTemperaturaDesdeTabla @tAuxi

GO