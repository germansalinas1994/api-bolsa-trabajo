using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Pedido
{
    public int Id { get; set; }

    public int? IdUsuario { get; set; }
    public int? IdSucursalPedido { get; set; }
    public DateTime? FechaAlta { get; set; }

    public DateTime? FechaBaja { get; set; }

    public DateTime? FechaModificacion { get; set; }
    public decimal? Total { get; set; }

    public long Orden_MercadoPago { get; set; }

    public virtual Usuario Usuario { get; set; }

    public virtual Pago Pago { get; set; }

    public virtual Envio? Envio { get; set; }
    public virtual Sucursal? Sucursal { get; set; }

    public virtual ICollection<PublicacionPedido> PublicacionPedido { get; set; } = new List<PublicacionPedido>();


}
