using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Usuario
{
    public static string DominioAdmin = "frlp.utn.edu.ar";
    public static string DominioCandidato = "alu.frlp.utn.edu.ar";

    public int Id { get; set; }

    public string? Nombre { get; set; }

    public string Email { get; set; } = null!;

    public string? FotoPerfil { get; set; }

    public bool? Activo { get; set; }

    public int IdRol { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }

    public virtual Rol Rol { get; set; } = null!;
}
