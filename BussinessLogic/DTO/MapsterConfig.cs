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
                .Map(d => d.Id, s => s.Id)
                .Map(d => d.Titulo, s => s.Titulo)
                .Map(d => d.Descripcion, s => s.Descripcion)
                .Map(d => d.NombreLocalidad, s => s.Localidad != null ? s.Localidad.Nombre : null)
                .Map(d => d.NombreEmpresa, s => s.PerfilEmpresa.Usuario.Nombre)
                .Map(d => d.TipoContrato, s => s.TipoContrato.Nombre)
                .Map(d => d.Modalidad, s => s.Modalidad.Nombre)
                .Map(d => d.FechaInicio, s => s.FechaInicio.ToShortDateString())
                .Map(d => d.FechaFin, s => s.FechaFin.HasValue ? s.FechaFin.Value.ToShortDateString() : "");


            TypeAdapterConfig<TipoContrato, TipoContratoDTO>
             .NewConfig()
             .Map(d => d.Codigo, s => s.Codigo)
             .Map(d => d.Descripcion, s => s.Nombre);

            TypeAdapterConfig<Modalidad, ModalidadDTO>
                .NewConfig()
                .Map(d => d.Codigo, s => s.Codigo)
                .Map(d => d.Descripcion, s => s.Nombre);

            TypeAdapterConfig<Carrera, CarreraDTO>
                .NewConfig()
                .Map(d => d.Codigo, s => s.Codigo)
                .Map(d => d.Descripcion, s => s.Nombre);

            TypeAdapterConfig<Postulacion, PostulacionDTO>
                .NewConfig()
                .Map(d => d.EstadoPostulacion,
                    s => s.Historial != null ? s.Historial .OrderByDescending(h => h.FechaModificacion)
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


        }
    }
}