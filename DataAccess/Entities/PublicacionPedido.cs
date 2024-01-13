using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataAccess.Entities;

public partial class PublicacionPedido
{
    [Key]
    public int IdPublicacionPedido { get; set; }

    [Required]
    [ForeignKey("Pedido")]
    public int IdPedido { get; set; }

    [Required]
    [ForeignKey("Publicacion")]
    public int IdPublicacion { get; set; }

    public int Cantidad { get; set; }

    public decimal Precio { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime? FechaBaja { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual Pedido Pedido { get; set; }

    public virtual Publicacion Publicacion { get; set; }



}