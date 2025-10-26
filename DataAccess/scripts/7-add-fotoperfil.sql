USE [db-bolsa-trabajo-local];
GO

-- ========= Agregar columna fotoPerfil a la tabla Usuario =========

-- Verificar si la columna ya existe antes de agregarla
IF COL_LENGTH('dbo.Usuario', 'fotoPerfil') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario
        ADD fotoPerfil NVARCHAR(MAX) NULL;
    
    PRINT 'Columna fotoPerfil agregada exitosamente a la tabla Usuario';
END
ELSE
BEGIN
    PRINT 'La columna fotoPerfil ya existe en la tabla Usuario';
END
GO
