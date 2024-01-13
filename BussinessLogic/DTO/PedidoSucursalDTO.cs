using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PedidoSucursalDTO
    {
    public int Id { get; set; }

    public decimal? Total { get; set; }

    public long Orden_MercadoPago { get; set; }

    public virtual string EmailUsuario { get; set; }

    public virtual List<DetallePedidoDTO> DetallePedido { get; set; } = new List<DetallePedidoDTO>();

    public virtual string? EstadoEnvio { get; set; }

    public virtual int? idEstadoEnvio { get; set; }
    public DateTime? Fecha { get; set; }


    }
}

