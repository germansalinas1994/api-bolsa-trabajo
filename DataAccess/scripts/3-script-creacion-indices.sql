USE [db-bolsa-trabajo-local];
SET NOCOUNT ON;

------------------ ÍNDICES ÚNICOS ------------------

-- Evita postulaciones duplicadas por oferta y candidato
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'UX_Postulacion_OfertaCandidato'
)
BEGIN
  CREATE UNIQUE INDEX UX_Postulacion_OfertaCandidato
  ON Postulacion (idOferta, idPerfilCandidato);
  PRINT '✓ UX_Postulacion_OfertaCandidato creado';
END

-- Evita asociaciones duplicadas entre oferta y carrera
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'UX_OfertaCarrera_OfertaCarrera'
)
BEGIN
  CREATE UNIQUE INDEX UX_OfertaCarrera_OfertaCarrera
  ON OfertaCarrera (idOferta, idCarrera);
  PRINT '✓ UX_OfertaCarrera_OfertaCarrera creado';
END

-- Evita asociaciones duplicadas entre oferta y categoría
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'UX_OfertaCategoria_OfertaCategoria'
)
BEGIN
  CREATE UNIQUE INDEX UX_OfertaCategoria_OfertaCategoria
  ON OfertaCategoria (idOferta, idCategoria);
  PRINT '✓ UX_OfertaCategoria_OfertaCategoria creado';
END

------------------ ÍNDICES NO ÚNICOS ------------------

-- Mejorar filtros de postulaciones
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Postulacion_Perfil'
)
BEGIN
  CREATE INDEX IX_Postulacion_Perfil ON Postulacion (idPerfilCandidato);
  PRINT '✓ IX_Postulacion_Perfil creado';
END

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Postulacion_Oferta'
)
BEGIN
  CREATE INDEX IX_Postulacion_Oferta ON Postulacion (idOferta);
  PRINT '✓ IX_Postulacion_Oferta creado';
END

-- Índices para consultas frecuentes en Oferta
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Oferta_PerfilEmpresa'
)
BEGIN
  CREATE INDEX IX_Oferta_PerfilEmpresa ON Oferta (idPerfilEmpresa);
  PRINT '✓ IX_Oferta_PerfilEmpresa creado';
END

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Oferta_Modalidad'
)
BEGIN
  CREATE INDEX IX_Oferta_Modalidad ON Oferta (idModalidad);
  PRINT '✓ IX_Oferta_Modalidad creado';
END

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Oferta_TipoContrato'
)
BEGIN
  CREATE INDEX IX_Oferta_TipoContrato ON Oferta (idTipoContrato);
  PRINT '✓ IX_Oferta_TipoContrato creado';
END

-- Índices para relaciones
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_PerfilCandidato_Usuario'
)
BEGIN
  CREATE INDEX IX_PerfilCandidato_Usuario ON PerfilCandidato (idUsuario);
  PRINT '✓ IX_PerfilCandidato_Usuario creado';
END

IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_PerfilEmpresa_Usuario'
)
BEGIN
  CREATE INDEX IX_PerfilEmpresa_Usuario ON PerfilEmpresa (idUsuario);
  PRINT '✓ IX_PerfilEmpresa_Usuario creado';
END

-- Usuario por rol y activo
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_RolActivo'
)
BEGIN
  CREATE INDEX IX_Usuario_RolActivo ON Usuario (idRol, activo);
  PRINT '✓ IX_Usuario_RolActivo creado';
END

-- Notificaciones por usuario y estado
IF NOT EXISTS (
  SELECT 1 FROM sys.indexes WHERE name = 'IX_Notificacion_UsuarioLeido'
)
BEGIN
  CREATE INDEX IX_Notificacion_UsuarioLeido ON Notificacion (idUsuario, leido);
  PRINT '✓ IX_Notificacion_UsuarioLeido creado';
END

PRINT 'Todos los índices fueron creados correctamente.';