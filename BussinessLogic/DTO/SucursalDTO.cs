using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class SucursalDTO
    {

        public int IdSucursal { get; set; }
        public string EmailSucursal { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public string? Direccion { get; set; }
        public string? Nombre { get; set; }



    }
}

