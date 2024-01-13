using System;
using System.Collections.Generic;

namespace DataAccess.Entities;

public partial class Envio
{
    public int Id { get; set; }

    public int IdPedido { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime? FechaBaja { get; set; }

    public DateTime FechaModificacion { get; set; }

    public int IdEstadoEnvio { get; set; }

    public int IdDomicilio { get; set; }
    public virtual Domicilio Domicilio { get; set; }
    public virtual Estadoenvio EstadoEnvio { get; set; }

    public virtual Pedido Pedido { get; set; }

}
