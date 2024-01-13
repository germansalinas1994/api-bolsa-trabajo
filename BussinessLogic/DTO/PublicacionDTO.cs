using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class PublicacionDTO
    {
    public int IdPublicacion { get; set; }

    // public decimal? Precio { get; set; }

    public int Stock { get; set; }
    public ProductoDTO? IdProductoNavigation { get; set; } = null!; 


    //esta cantidad es para que pueda viajar la cantidad de producto que tiene el carrito al front
    public int Cantidad { get; set; }
    
    public SucursalDTO? IdSucursalNavigation { get; set; } = null!;

    }
}

