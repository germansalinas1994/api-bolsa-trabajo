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
                .Map(d => d.Titulo, s => s.Titulo)
                .Map(d => d.Descripcion, s => s.Descripcion)
                .Map(d => d.NombreLocalidad, s => s.Localidad != null ? s.Localidad.Nombre : null)
                .Map(d => d.NombreProvincia, s => s.Localidad.Provincia.Nombre);
                
        }
    }
}