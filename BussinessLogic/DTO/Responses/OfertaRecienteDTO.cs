using System;
using System.ComponentModel.DataAnnotations;

namespace BussinessLogic.DTO {

    public class OfertaRecienteDTO
    {
        public List<OfertaDTO> Ofertas { get; set; }
        public int CantidadOfertas { get; set; }
    }   
} 
