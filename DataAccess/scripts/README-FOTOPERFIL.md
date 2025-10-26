# Cambios realizados: Agregar foto de perfil a usuarios

## Resumen
Se ha agregado la funcionalidad de **foto de perfil** para usuarios (tanto candidatos como empresas) en la aplicación de bolsa de trabajo.

## Cambios en Backend (.NET)

### 1. Base de Datos
**Archivo:** `7-add-fotoperfil.sql`
- Agrega la columna `fotoPerfil` (NVARCHAR(MAX)) a la tabla `Usuario`
- Permite valores NULL
- Incluye validación para evitar duplicación si ya existe la columna

**Para ejecutar:**
```sql
-- En SQL Server Management Studio o herramienta similar
USE [db-bolsa-trabajo-local];
GO
-- Ejecutar el script 7-add-fotoperfil.sql
```

### 2. Entidades y DTOs
**Archivos modificados:**
- `DataAccess/Entities/Usuario.cs` - Agregada propiedad `FotoPerfil`
- `BussinessLogic/DTO/UsuarioDTO.cs` - Agregado campo `FotoPerfil`
- `BussinessLogic/DTO/PerfilCandidatoDTO.cs` - Agregado campo `FotoPerfil`
- `BussinessLogic/DTO/PerfilEmpresaDTO.cs` - Agregado campo `FotoPerfil`

### 3. Controladores - Nuevos Endpoints
**Archivo:** `API-BolsaTrabajo/Controllers/CandidatoController.cs`
```csharp
[HttpPost]
[Route("upload_foto_perfil")]
public async Task<ApiResponse> UploadFotoPerfil([FromForm] IFormFile foto, [FromQuery] int perfilId)
```

**Archivo:** `API-BolsaTrabajo/Controllers/EmpresaController.cs`
```csharp
[HttpPost]
[Route("upload_foto_perfil")]
public async Task<ApiResponse> UploadFotoPerfil([FromForm] IFormFile foto, [FromQuery] int perfilId)
```

**Validaciones implementadas:**
- Solo permite imágenes: JPG, JPEG, PNG, GIF, WEBP
- Tamaño máximo: 2MB
- Validación de Content-Type y extensión de archivo
- Conversión automática a Base64 para almacenamiento

## Cambios en Frontend (Next.js + TypeScript)

### 1. Tipos TypeScript
**Archivos modificados:**
- `types/dto/perfilCandidatoDTO.ts` - Agregado campo `fotoPerfil?: string | null`
- `types/dto/perfilEmpresaDTO.ts` - Agregado campo `fotoPerfil?: string | null`

### 2. Endpoints
**Archivo:** `services/Generics/endpoints.ts`
```typescript
CANDIDATO: {
  // ... otros endpoints
  UPLOAD_FOTO_PERFIL: `/Candidato/upload_foto_perfil`,
}
EMPRESA: {
  // ... otros endpoints
  UPLOAD_FOTO_PERFIL: `/Empresa/upload_foto_perfil`,
}
```

### 3. Servicios
**Archivo:** `services/candidato.service.ts`
```typescript
async uploadFotoPerfil(file: File, perfilId: number): Promise<string>
```

**Archivo:** `services/empresa.service.ts`
```typescript
async uploadFotoPerfil(file: File, perfilId: number): Promise<string>
```

### 4. Componentes de Perfil

**Archivo:** `app/estudiante/perfil/page.tsx`
- Avatar muestra la foto de perfil si existe, o la inicial del nombre
- Botón circular con icono de cámara sobre el avatar
- Upload inmediato al seleccionar archivo
- Muestra notificación de éxito/error
- Recarga automática del perfil después de subir

**Archivo:** `app/empresa/perfil/[perfilId]/page.tsx`
- Funcionalidad similar al perfil de estudiante
- Solo el dueño del perfil puede ver y usar el botón de cámara
- Uso del hook `useAuth()` para validar permisos

## Flujo de Usuario

### Para Candidatos/Estudiantes:
1. Navegar a "Mi Perfil"
2. Click en el icono de cámara sobre el avatar
3. Seleccionar una imagen (JPG, PNG, GIF, WEBP, máx 2MB)
4. La foto se sube automáticamente
5. Se muestra notificación de éxito
6. El avatar se actualiza con la nueva foto

### Para Empresas:
1. Navegar a "Perfil de Empresa"
2. (Solo si es el dueño del perfil) Click en el icono de cámara sobre el avatar
3. Seleccionar una imagen
4. Proceso idéntico al de candidatos

## Formato de Almacenamiento
- **Base de Datos:** String Base64 en columna `fotoPerfil` (NVARCHAR(MAX))
- **Frontend:** Se muestra usando: `data:image/jpeg;base64,${perfil.fotoPerfil}`
- **API:** Recibe archivo como FormData y convierte a Base64

## Consideraciones de Seguridad
✅ Validación de tipo de archivo (Content-Type y extensión)
✅ Límite de tamaño (2MB)
✅ Solo formatos de imagen permitidos
✅ Validación de perfilId
✅ Control de permisos en frontend (solo dueño puede editar)

## Testing Recomendado
1. ✅ Subir imagen válida (JPG, PNG)
2. ⚠️ Intentar subir archivo no permitido (PDF, ZIP)
3. ⚠️ Intentar subir imagen > 2MB
4. ✅ Verificar que la foto se muestre correctamente después de subir
5. ✅ Verificar que usuarios no autorizados no vean el botón de cámara
6. ✅ Verificar almacenamiento en base de datos

## Próximos Pasos (Opcional)
- [ ] Implementar eliminación de foto de perfil
- [ ] Agregar recorte/edición de imagen antes de subir
- [ ] Optimización de imágenes (compresión automática)
- [ ] Almacenamiento en servicio externo (S3, Azure Blob, Cloudinary)
- [ ] Caché de imágenes en CDN

## Notas de Migración
Si tienes datos existentes en la base de datos, ejecuta el script de migración:
```bash
# Desde SQL Server
sqlcmd -S localhost -d db-bolsa-trabajo-local -i "DataAccess/scripts/7-add-fotoperfil.sql"
```

La columna acepta NULL, por lo que los usuarios existentes no se verán afectados.
