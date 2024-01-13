using System;
using DataAccess.Entities;
using Microsoft.AspNetCore.Http;

namespace BussinessLogic.DTO
{
    public class ProductoDTO
    {
        public int IdProducto { get; set; }

        public string? Descripcion { get; set; }

        public decimal? Precio { get; set; }


        public int? idCategoria { get; set; }

        public string? UrlImagen { get; set; }

        public string Nombre { get; set; }
        public CategoriaDTO? IdCategoriaNavigation { get; set; }

        public IFormFile? Archivo { get; set; }



    }
}

