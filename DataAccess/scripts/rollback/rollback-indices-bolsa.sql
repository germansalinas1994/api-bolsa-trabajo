USE [db-bolsa-trabajo-local];
SET NOCOUNT ON;

-- Eliminación segura de índices

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Postulacion_OfertaCandidato')
DROP INDEX UX_Postulacion_OfertaCandidato ON Postulacion;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_OfertaCarrera_OfertaCarrera')
DROP INDEX UX_OfertaCarrera_OfertaCarrera ON OfertaCarrera;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_OfertaCategoria_OfertaCategoria')
DROP INDEX UX_OfertaCategoria_OfertaCategoria ON OfertaCategoria;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Postulacion_Perfil')
DROP INDEX IX_Postulacion_Perfil ON Postulacion;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Postulacion_Oferta')
DROP INDEX IX_Postulacion_Oferta ON Postulacion;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Oferta_PerfilEmpresa')
DROP INDEX IX_Oferta_PerfilEmpresa ON Oferta;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Oferta_Modalidad')
DROP INDEX IX_Oferta_Modalidad ON Oferta;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Oferta_TipoContrato')
DROP INDEX IX_Oferta_TipoContrato ON Oferta;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PerfilCandidato_Usuario')
DROP INDEX IX_PerfilCandidato_Usuario ON PerfilCandidato;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_PerfilEmpresa_Usuario')
DROP INDEX IX_PerfilEmpresa_Usuario ON PerfilEmpresa;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_RolActivo')
DROP INDEX IX_Usuario_RolActivo ON Usuario;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Notificacion_UsuarioLeido')
DROP INDEX IX_Notificacion_UsuarioLeido ON Notificacion;

PRINT '↩️ Rollback completado: Todos los índices fueron eliminados.';