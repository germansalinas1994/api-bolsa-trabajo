using System;
using System.ComponentModel.DataAnnotations;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class UsuarioDTO
    {
        public int? Id { get; set; }
        public string? Email { get; set; }
        public int? IdRol { get; set; }



    }

}

