using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BussinessLogic.DTO.Search
{
    public class PreferenceMercadoPagoDTO
    {
        public int IdDomicilio { get; set; }
        public List<SearchPublicacionCarritoDTO> Publicaciones { get; set; }
    }


}