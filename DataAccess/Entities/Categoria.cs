using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Categoria
{
    public int IdCategoria { get; set; }

    public string? Descripcion { get; set; }

    public DateTime FechaDesde { get; set; }

    public DateTime? FechaHasta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public string? Nombre { get; set; }

    public bool? Agrupador { get; set; }

    public virtual ICollection<Producto> Producto { get; set; } = new List<Producto>();
}
