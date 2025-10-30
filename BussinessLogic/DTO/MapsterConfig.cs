using Mapster;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class MapsterConfig
    {
        public static void RegisterMappings()
        {
            var config = TypeAdapterConfig.GlobalSettings;

            config.Default
                .IgnoreNullValues(true)
                .NameMatchingStrategy(NameMatchingStrategy.Flexible);


            // Oferta -> OfertaDTO (incluye Localidad)
            TypeAdapterConfig<Oferta, OfertaDTO>
                .NewConfig()
                .Map(d => d.NombreLocalidad, s => s.Localidad != null ? s.Localidad.Nombre : null)
                .Map(d => d.NombreProvincia, s => s.Localidad != null && s.Localidad.Provincia != null ? s.Localidad.Provincia.Nombre : null)
                .Map(d => d.NombreEmpresa, s => s.PerfilEmpresa.RazonSocial)
                .Map(d => d.TipoContrato, s => s.TipoContrato.Nombre)
                .Map(d => d.Modalidad, s => s.Modalidad.Nombre)
                .Map(d => d.FechaInicio, s => s.FechaInicio.ToShortDateString())
                .Map(d => d.FechaFin, s => s.FechaFin.HasValue ? s.FechaFin.Value.ToShortDateString() : "");


            TypeAdapterConfig<TipoContrato, TipoContratoDTO>
             .NewConfig()
             .Map(d => d.Id, s => s.Id)
             .Map(d => d.Codigo, s => s.Codigo)
             .Map(d => d.Descripcion, s => s.Nombre);

            TypeAdapterConfig<Modalidad, ModalidadDTO>
                .NewConfig()
                .Map(d => d.Id, s => s.Id)
                .Map(d => d.Codigo, s => s.Codigo)
                .Map(d => d.Descripcion, s => s.Nombre);

            TypeAdapterConfig<Carrera, CarreraDTO>
                .NewConfig()
                .Map(d => d.Id, s => s.Id)
                .Map(d => d.Codigo, s => s.Codigo)
                .Map(d => d.Nombre, s => s.Nombre);

            // CrearOfertaDTO -> Oferta (ignora IdPerfilEmpresa ya que se asigna manualmente)
            TypeAdapterConfig<CrearOfertaDTO, Oferta>
                .NewConfig()
                .Ignore(d => d.Id)
                .Ignore(d => d.IdPerfilEmpresa)
                .Ignore(d => d.FechaAlta)
                .Ignore(d => d.FechaModificacion)
                .Ignore(d => d.FechaBaja)
                .Ignore(d => d.Localidad)
                .Ignore(d => d.Modalidad)
                .Ignore(d => d.PerfilEmpresa)
                .Ignore(d => d.TipoContrato)
                .Ignore(d => d.Postulaciones);

            TypeAdapterConfig<Postulacion, PostulacionDTO>
                .NewConfig()
                .Map(d => d.EstadoPostulacion,
                    s => s.Historial != null ? s.Historial.OrderByDescending(h => h.FechaModificacion)
                            .Select(h => h.EstadoPostulacion.Nombre)
                            .FirstOrDefault() : string.Empty)

                .Map(d => d.FechaPostulacion, s => s.FechaAlta.ToString("dd/MM/yyyy"))

                // Mapeos de Oferta y relaciones
                .Map(d => d.NombreEmpresa, s => s.Oferta.PerfilEmpresa.RazonSocial)
                .Map(d => d.TituloOferta, s => s.Oferta.Titulo)
                .Map(d => d.DescripcionOferta, s => s.Oferta.Descripcion)
                .Map(d => d.DescripcionModalidad, s => s.Oferta.Modalidad.Nombre)
                .Map(d => d.DescripcionTipoContrato, s => s.Oferta.TipoContrato.Nombre)
                .Map(d => d.DescripcionLocalidad, s => s.Oferta.Localidad.Nombre)
                .Map(d => d.DescripcionProvincia, s => s.Oferta.Localidad.Provincia.Nombre)
                .Map(d => d.DescripcionPais, s => s.Oferta.Localidad.Provincia.Pais.Nombre);

            TypeAdapterConfig<EstadoValidacion, EstadoValidacionDTO>
                         .NewConfig()

                         .Map(d => d.Descripcion, s => s.Nombre);
            TypeAdapterConfig<Carrera, CarreraDTO>
                .NewConfig()
                .Map(d => d.Descripcion, s => s.Nombre);

            TypeAdapterConfig<Usuario, UsuarioDTO>
                 .NewConfig()
                 .Map(d => d.RolNombre, s => s.Rol != null ? s.Rol.Nombre : null)
                 .Map(d => d.FechaAlta, s => s.FechaAlta.ToShortDateString().ToString())
                 .Map(d => d.FechaBaja, s => s.FechaBaja != null ? s.FechaBaja.Value.ToShortDateString().ToString() : null)
                 .Map(d => d.Activo, s => s.Activo != null ? (bool)s.Activo ? "Sí" : "No" : "No")
                 ;

            TypeAdapterConfig<PerfilCandidato, PerfilCompletoDTO>
               .NewConfig()
               .Map(d => d.Email, s => s.Usuario != null ? s.Usuario.Email : null)
               .Map(d => d.Nombre, s => s.Usuario != null ? s.Usuario.Nombre : null)
               .Map(d => d.IdRol, s => s.Usuario != null ? s.Usuario.IdRol : 0)
               .Map(d => d.FechaAlta, s => s.Usuario != null ? s.Usuario.FechaAlta.ToShortDateString() : null)
               .Map(d => d.FechaBaja, s => s.Usuario != null && s.Usuario.FechaBaja != null ? s.Usuario.FechaBaja.Value.ToShortDateString() : null)
               .Map(d => d.RolNombre, s => s.Usuario != null && s.Usuario.Rol != null ? s.Usuario.Rol.Nombre : null)
               .Map(d => d.DescripcionCandidato, s => s.Descripcion)
               .Map(d => d.NombreCarrera, s => s.Carrera != null ? s.Carrera.Nombre : null)
               .Map(d => d.Legajo, s => s.Legajo)
               .Map(d => d.Genero, s => s.Genero != null ? s.Genero.Nombre : null)
               ;

            TypeAdapterConfig<PerfilEmpresa, PerfilCompletoDTO>
              .NewConfig()
              .Map(d => d.Email, s => s.Usuario != null ? s.Usuario.Email : null)
              .Map(d => d.Nombre, s => s.Usuario != null ? s.Usuario.Nombre : null)
              .Map(d => d.IdRol, s => s.Usuario != null ? s.Usuario.IdRol : 0)
              .Map(d => d.Activo, s => s.Usuario != null && s.Usuario.Activo != null ? (bool)s.Usuario.Activo ? "Sí" : "No" : "No")
              .Map(d => d.RolNombre, s => s.Usuario != null && s.Usuario.Rol != null ? s.Usuario.Rol.Nombre : null)
              .Map(d => d.FechaAlta, s => s.Usuario != null ? s.Usuario.FechaAlta.ToShortDateString() : null)
              .Map(d => d.FechaBaja, s => s.Usuario != null && s.Usuario.FechaBaja != null ? s.Usuario.FechaBaja.Value.ToShortDateString() : null)
              .Map(d => d.RolNombre, s => s.Usuario != null && s.Usuario.Rol != null ? s.Usuario.Rol.Nombre : null)
              .Map(d => d.DescripcionEmpresa, s => s.Descripcion)
              .Map(d => d.EstadoValidacion, s => s.EstadoValidacion != null ? s.EstadoValidacion.Nombre : null)
              ;

            TypeAdapterConfig<Usuario, PerfilCompletoDTO>
          .NewConfig()
              .Map(d => d.Activo, s => s.Activo != null ? (bool)s.Activo ? "Sí" : "No" : "No")
                .Map(d => d.RolNombre, s => s.Rol != null ? s.Rol.Nombre : null)
                .Map(d => d.FechaAlta, s => s.FechaAlta.ToShortDateString())
                .Map(d => d.FechaBaja, s => s.FechaBaja != null ? s.FechaBaja.Value.ToShortDateString() : null)

          ;

        }
    }
}