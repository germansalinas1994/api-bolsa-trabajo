using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PublicacionPedidoDTO
    {
        public PublicacionDTO Publicacion { get; set; }


        public int Cantidad { get; set; }

        public decimal Precio { get; set; }

        public decimal SubTotal { get { return Cantidad * Precio; }}


    }
}

