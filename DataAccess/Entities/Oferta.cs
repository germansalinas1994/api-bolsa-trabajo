

namespace DataAccess.Entities;

public partial class Oferta
{

    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public int IdPerfilEmpresa { get; set; }

    public string Titulo { get; set; } = null!;

    public int IdModalidad { get; set; }

    public int IdTipoContrato { get; set; }

    public DateTime FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? IdLocalidad { get; set; }

    public DateTime FechaAlta { get; set; }

    public DateTime FechaModificacion { get; set; }

    public DateTime? FechaBaja { get; set; }


    public virtual Localidad? Localidad { get; set; }

    public virtual Modalidad Modalidad { get; set; } = null!;

    public virtual PerfilEmpresa PerfilEmpresa { get; set; } = null!;

    public virtual TipoContrato TipoContrato { get; set; } = null!;

    public virtual ICollection<Postulacion> Postulaciones { get; set; }
}
