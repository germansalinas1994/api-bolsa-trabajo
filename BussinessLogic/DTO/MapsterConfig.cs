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


        }
    }
}