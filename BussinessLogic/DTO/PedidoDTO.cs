using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PedidoDTO
    {
    public int Id { get; set; }

    public decimal? Total { get; set; }

    public long Orden_MercadoPago { get; set; }

    public virtual UsuarioDTO Usuario { get; set; }

    public virtual PagoDTO Pago { get; set; }

    public virtual List<PublicacionPedidoDTO> PublicacionPedido { get; set; } = new List<PublicacionPedidoDTO>();

    public virtual EnvioDTO Envio { get; set; }
    public DateTime? FechaAlta { get; set; }


    }
}

