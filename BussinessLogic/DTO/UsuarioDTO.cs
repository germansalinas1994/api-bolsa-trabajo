using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class UsuarioDTO
    {
        public int? Id { get; set; }
        public string? Email { get; set; }
        public string? Nombre { get; set; }
        public int? IdRol { get; set; }
        public string? FotoPerfil { get; set; }

        public string? Activo { get; set; }

        public string? FechaAlta { get; set; }
        public string? FechaBaja { get; set; }

        public string?  RolNombre { get; set; }
        



    }

}

