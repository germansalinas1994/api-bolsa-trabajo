public class PostulacionCandidatoDTO
{
    // Datos de la Postulación
    public int IdPostulacion { get; set; }
    public string EstadoPostulacion { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public string? CartaPresentacion { get; set; }
    public DateTime FechaPostulacion { get; set; }

    // Datos del Candidato
    public int IdCandidato { get; set; }
    public string? NombreCandidato { get; set; }
    public string? Email { get; set; }
    public string? DescripcionPerfil { get; set; }
    public string? GeneroNombre { get; set; }
    public string? CarreraNombre { get; set; }
    public int? AnioEgreso { get; set; }
    public string? Cv { get; set; }
    public string? FotoPerfil { get; set; }

    // 🔹 Competencias del Candidato
    public List<string>? Competencias { get; set; }

    // 🔹 Datos de la Oferta
    public int IdOferta { get; set; }
    public string? TituloOferta { get; set; }
    public string? DescripcionOferta { get; set; }

    //  Información complementaria
    public string? Modalidad { get; set; }
    public string? TipoContrato { get; set; }
    public string? Localidad { get; set; }
    public string? Provincia { get; set; }
    public string? Pais { get; set; }
}
