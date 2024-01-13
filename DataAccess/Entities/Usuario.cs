using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? Nombre { get; set; }

    public string? Apellido { get; set; }

    public string Email { get; set; } = null!;

    public int? Dni { get; set; }

    public string? Telefono { get; set; }
    public DateTime FechaAlta { get; set; }
    public DateTime? FechaBaja { get; set; }
    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<Domicilio> Domicilio { get; set; } = new List<Domicilio>();
    public virtual ICollection<Pedido> Pedido { get; set; } = new List<Pedido>();
}
