USE [db-bolsa-trabajo-local];
SET NOCOUNT ON;

------------------------------------------------------------
-- Verifica si el índice ya existe antes de crearlo
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'UX_Postulacion_OfertaCandidato'
      AND object_id = OBJECT_ID('Postulacion')
)
BEGIN
    CREATE UNIQUE INDEX UX_Postulacion_OfertaCandidato
    ON Postulacion (IdOferta, IdPerfilCandidato);

    PRINT 'Índice UX_Postulacion_OfertaCandidato creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El índice UX_Postulacion_OfertaCandidato ya existe.';
END

------------------------------------------------------------

PRINT 'Script ejecutado correctamente.';


