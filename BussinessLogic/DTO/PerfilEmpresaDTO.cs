namespace BussinessLogic.DTO;

public class PerfilEmpresaDTO
{
    // Campos de PerfilEmpresa
    public int Id { get; set; }
    public int IdUsuario { get; set; }
    public string? Descripcion { get; set; }
    public string RazonSocial { get; set; } = null!;
    public string? Cuit { get; set; }
    public int? IdEstadoValidacion { get; set; }
    public DateTime FechaAlta { get; set; }
    public DateTime FechaModificacion { get; set; }
    public DateTime? FechaBaja { get; set; }

    // Derivados/relacionados desde Usuario
    public string? Nombre { get; set; }
    public string? Email { get; set; }
    public bool? UsuarioActivo { get; set; }
    public int? IdRol { get; set; }
    public string? FotoPerfil { get; set; }

    // Derivados/relacionados desde Rol
    public string? RolNombre { get; set; }
    public string? RolCodigo { get; set; }

    // Derivados/relacionados desde EstadoValidacion
    public string? EstadoValidacionNombre { get; set; }
    public string? EstadoValidacionCodigo { get; set; }

    // Campos adicionales de contacto (si existen en la base de datos)
    public string? Telefono { get; set; }
    public string? Localidad { get; set; }

    // Lista de ofertas publicadas por la empresa
    public List<OfertaDTO>? Ofertas { get; set; }

    // Calculado por el backend
    public int? PorcentajePerfil { get; set; }
}

