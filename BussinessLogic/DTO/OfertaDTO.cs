using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class OfertaDTO
    {
        public string Titulo { get; set; }
        public string Descripcion { get; set; }

        public LocalidadDTO Localidad { get; set; }



    }
}

