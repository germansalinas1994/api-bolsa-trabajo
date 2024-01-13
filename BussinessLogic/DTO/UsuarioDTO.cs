using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class UsuarioDTO
    {

        public int IdUsuario { get; set; }
        public string? Nombre { get; set; }

        public string? Apellido { get; set; }

        public string Email { get; set; } = null!;

        public int? Dni { get; set; }

        public string? Telefono { get; set; }


    }
}

