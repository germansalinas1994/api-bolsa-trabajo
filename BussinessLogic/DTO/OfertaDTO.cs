using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class OfertaDTO
    {
        public int Id { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        public string? NombreLocalidad { get; set; }

        public string NombreProvincia { get; set; }

        public string NombreEmpresa { get; set; }

        public string TipoContrato { get; set; }

        public string Modalidad { get; set; }

        public string FechaInicio { get; set; }

        public string FechaFin { get; set; }
        

     


    }
}

