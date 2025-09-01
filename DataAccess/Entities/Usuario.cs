using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;


    public bool? Activo { get; set; }

    public int IdRol { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }

    public virtual Rol Rol { get; set; } = null!;
}
