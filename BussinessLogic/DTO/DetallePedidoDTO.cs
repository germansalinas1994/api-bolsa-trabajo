using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class DetallePedidoDTO
    {
        public int Id { get; set; }
        public string? NombreProducto { get; set; }


        public int Cantidad { get; set; }

        public decimal Precio { get; set; }

        public decimal SubTotal { get; set; }

    }
}

