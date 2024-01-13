using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Estadopublicacion
{
    public int IdEstadoPublicacion { get; set; }

    public string? Descripcion { get; set; }

    public virtual ICollection<Publicacion> Publicacion { get; set; } = new List<Publicacion>();
}
