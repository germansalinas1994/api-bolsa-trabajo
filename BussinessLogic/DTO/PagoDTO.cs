using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PagoDTO
    {
    public int Id { get; set; }

    public string EstadoPago { get; set; }
    public long IdPagoMercadoPago { get; set; }

    public decimal? Total { get; set; }

    // public virtual PedidoDTO Pedido { get; set; }


    }
}

