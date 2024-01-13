using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

/// <summary>
/// 						
/// </summary>
public partial class Publicacion
{
    public int IdPublicacion { get; set; }

    public DateTime FechaDesde { get; set; }
    public DateTime FechaActualizacion { get; set; }

    public DateTime? FechaHasta { get; set; }

    // public float? Precio { get; set; }

    public int? IdProducto { get; set; }

    public int? Stock { get; set; }

    public int? IdSucursal { get; set; }

    public virtual Producto? IdProductoNavigation { get; set; }

    public virtual Sucursal? IdSucursalNavigation { get; set; }
}
