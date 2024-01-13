using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Pago
{
    public int Id { get; set; }

    public int IdPedido { get; set; }

    public DateTime? FechaAlta { get; set; }

    public DateTime? FechaBaja { get; set; }

    public DateTime? FechaModificacion { get; set; }
    public string EstadoPago { get; set; }
    public long IdPagoMercadoPago { get; set; }

    public decimal? Total { get; set; }

    public virtual Pedido Pedido { get; set; }

}
