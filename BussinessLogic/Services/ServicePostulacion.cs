using System;
using System.Security.Cryptography.X509Certificates;
using BussinessLogic.DTO;
using DataAccess.IRepository;
using DataAccess.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;
using AutoWrapper.Wrappers;
using System.Net;

namespace BussinessLogic.Services
{
    public class ServicePostulacion : GenericService
    {
        private readonly ServicePublicacion _servicePublicacion;
        private readonly ServiceEmail _serviceEmail;

        public ServicePostulacion(
      IUnitOfWork unitOfWork,
      ServicePublicacion servicePublicacion,
      ServiceEmail serviceEmail)
      : base(unitOfWork)
        {
            _servicePublicacion = servicePublicacion;
            _serviceEmail = serviceEmail;
        }
        public async Task CrearPostulacion(PostulacionDTO data, string email)
        {
            bool commitRealizado = false;
            Usuario usuario = null;
            PerfilCandidato perfilCandidato = null;
            Oferta oferta = null;


            try
            {
                await _unitOfWork.BeginTransactionAsync();
                usuario = (await _unitOfWork.GenericRepository<Usuario>()
                   .GetByCriteria(u => u.Email == email)).FirstOrDefault();

                if (usuario == null)
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);


                perfilCandidato = (await _unitOfWork.GenericRepository<PerfilCandidato>()
                   .GetByCriteria(u => u.IdUsuario == usuario.Id)).FirstOrDefault();

                if (perfilCandidato == null)
                    throw new ApiException("El perfil de candidato no existe para el usuario", (int)HttpStatusCode.NotFound);


                //busco que no exista una postulacion igual para la misma oferta y candidato
                var postulacionExistente = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteria(p => p.IdOferta == data.IdOferta && p.IdPerfilCandidato == perfilCandidato.Id && p.FechaBaja == null)).FirstOrDefault();
                if (postulacionExistente != null)
                    throw new ApiException("Ya existe una postulación para esta oferta con el mismo perfil de candidato", (int)HttpStatusCode.Conflict);


                //Recupero la oferta
                oferta = await _servicePublicacion.GetPublicacionEntidadById(data.IdOferta.Value);
                //recupero el candidato
                if (oferta == null)
                    throw new ApiException("La oferta no existe", (int)HttpStatusCode.NotFound);


                Postulacion nuevaPostulacion = new();
                nuevaPostulacion.IdOferta = oferta.Id;
                nuevaPostulacion.IdPerfilCandidato = perfilCandidato.Id;
                nuevaPostulacion.CartaPresentacion = data.CartaPresentacion;
                nuevaPostulacion.Observacion = data.Observacion;
                nuevaPostulacion.FechaAlta = DateTime.Now;
                nuevaPostulacion.FechaModificacion = DateTime.Now;
                Postulacion postulacionPersistida = await _unitOfWork.GenericRepository<Postulacion>().Insert(nuevaPostulacion);

                //produzco un error aproposito para verificar que funcione la transaccion


                PostulacionHistorial historial = new();
                historial.IdPostulacion = postulacionPersistida.Id;

                EstadoPostulacion estadoIniciada = await _unitOfWork.GenericRepository<EstadoPostulacion>().GetById(EstadoPostulacion.IdEstadoIniciada);
                if (estadoIniciada == null)
                    throw new ApiException("El estado 'Iniciada' no está definido en la base de datos.", (int)HttpStatusCode.NotFound);

                historial.IdEstadoPostulacion = estadoIniciada.Id;
                historial.FechaAlta = DateTime.Now;
                historial.FechaModificacion = DateTime.Now;
                historial.Motivo = "Postulación creada";
                await _unitOfWork.GenericRepository<PostulacionHistorial>().Insert(historial);


                //envio el correo de notificacion al candidato
                // Envío de correos automáticos

                await _unitOfWork.CommitAsync();


                EnviarMailNotificacionPostulacion(perfilCandidato, oferta, estadoIniciada);



            }
            catch (ApiException)
            {
                await _unitOfWork.RollbackAsync();

                throw; // Re-lanzar la excepción ApiException sin envolverla nuevamente
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }

        }

        private async Task EnviarMailNotificacionPostulacion(PerfilCandidato perfilCandidato, Oferta oferta, EstadoPostulacion estadoIniciada)
        {
            var emailEmpresa = oferta?.PerfilEmpresa?.Usuario?.Email ?? "noreply@utnfrlp.edu.ar";
            var emailCandidato = perfilCandidato?.Usuario?.Email ?? "noreply@utnfrlp.edu.ar";
            var titulo = oferta?.Titulo ?? "Oferta laboral";
            var razonSocial = oferta?.PerfilEmpresa?.RazonSocial ?? "Sin Razón Social";
            var nombreCandidato = perfilCandidato?.Usuario?.Nombre ?? "Sin Nombre";




            string cuerpoHtmlCandidato = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif; color: #333; background-color: #f5f5f5; padding: 20px;'>
                                <table align='center' width='600' cellpadding='0' cellspacing='0' 
                                    style='background-color: #fff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                                <tr>
                                    <td style='background-color: #003366; padding: 20px; text-align: center;'>
                                    <img src='https://www.frlp.utn.edu.ar/sites/default/files/LOGO%20VERTICAL_1.jpg' 
                                        alt='UTN FRLP Logo' width='120' style='display:block; margin:auto; border-radius:5px;' />
                                    <h2 style='color: #fff; margin-top: 10px;'>Bolsa de Trabajo - UTN FRLP</h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 30px;'>
                                    <p>Hola <b>{perfilCandidato.Usuario.Nombre}</b>,</p>
                                    <p>
                                        Tu postulación a la oferta <b>{oferta.Titulo}</b> de la empresa <b>{oferta.PerfilEmpresa.RazonSocial}</b> 
                                        ha sido registrada exitosamente en el sistema.
                                    </p>
                                    <p>
                                        El estado actual de tu postulación es: 
                                        <b style='color:#003366'>{estadoIniciada.Nombre}</b>.
                                    </p>
                                    <p>
                                        Podrás seguir su evolución desde tu panel en la 
                                        <a href='https://bolsadetrabajo.utnfrlp.edu.ar' 
                                        style='color: #003366; text-decoration: none; font-weight: bold;'>
                                        Bolsa de Trabajo UTN FRLP
                                        </a>.
                                    </p>
                                    <p style='margin-top: 30px; color: #555; font-size: 14px;'>
                                        Este es un mensaje automático. Por favor, no respondas a este correo.
                                    </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #f0f0f0; text-align: center; padding: 15px; font-size: 12px; color: #666;'>
                                    © {DateTime.Now.Year} Universidad Tecnológica Nacional - Facultad Regional La Plata<br/>
                                    <a href='https://www.frlp.utn.edu.ar' style='color: #003366; text-decoration: none;'>www.frlp.utn.edu.ar</a>
                                    </td>
                                </tr>
                                </table>
                            </body>
                            </html>";

            string cuerpoHtmlEmpresa = $@"
                            <html>
                            <body style='font-family: Arial, sans-serif; color: #333; background-color: #f5f5f5; padding: 20px;'>
                                <table align='center' width='600' cellpadding='0' cellspacing='0' 
                                    style='background-color: #fff; border-radius: 8px; box-shadow: 0 0 10px rgba(0,0,0,0.1);'>
                                <tr>
                                    <td style='background-color: #003366; padding: 20px; text-align: center;'>
                                    <img src='https://www.frlp.utn.edu.ar/sites/default/files/LOGO%20VERTICAL_1.jpg' 
                                        alt='UTN FRLP Logo' width='120' style='display:block; margin:auto; border-radius:5px;' />
                                    <h2 style='color: #fff; margin-top: 10px;'>Bolsa de Trabajo - UTN FRLP</h2>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='padding: 30px;'>
                                    <p>Estimado/a representante de <b>{oferta.PerfilEmpresa.RazonSocial}</b>,</p>
                                    <p>
                                        Se ha recibido una nueva postulación para la oferta <b>{oferta.Titulo}</b>.
                                    </p>
                                    <p>
                                        El candidato <b>{perfilCandidato.Usuario.Nombre}</b> ha enviado su solicitud y 
                                        se encuentra actualmente en el estado <b style='color:#003366'>{estadoIniciada.Nombre}</b>.
                                    </p>
                                    <p>
                                        Puede revisar los detalles de la postulación ingresando a su panel de empresa en la 
                                        <a href='https://bolsadetrabajo.utnfrlp.edu.ar' 
                                        style='color: #003366; text-decoration: none; font-weight: bold;'>
                                        Bolsa de Trabajo UTN FRLP
                                        </a>.
                                    </p>
                                    <p style='margin-top: 30px; color: #555; font-size: 14px;'>
                                        Este es un mensaje automático de notificación. No responda a este correo.
                                    </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style='background-color: #f0f0f0; text-align: center; padding: 15px; font-size: 12px; color: #666;'>
                                    © {DateTime.Now.Year} Universidad Tecnológica Nacional - Facultad Regional La Plata<br/>
                                    <a href='https://www.frlp.utn.edu.ar' style='color: #003366; text-decoration: none;'>www.frlp.utn.edu.ar</a>
                                    </td>
                                </tr>
                                </table>
                            </body>
                            </html>";

            _serviceEmail.EnviarCorreoAsync(
               destinatario: emailEmpresa,
               asunto: "Nueva postulación recibida",
               cuerpoHtml: cuerpoHtmlEmpresa
           );

            _serviceEmail.EnviarCorreoAsync(
               destinatario: emailCandidato,
               asunto: "Postulación enviada con éxito",
               cuerpoHtml: cuerpoHtmlCandidato
           );
        }

        public async Task<List<PostulacionDTO>> GetPostulaciones(string email)
        {
            try
            {
                int idUsuario = (await _unitOfWork.GenericRepository<Usuario>()
                    .GetByCriteria(u => u.Email == email)).FirstOrDefault().Id;

                if (idUsuario == 0)
                {
                    throw new ApiException("El usuario no existe", (int)HttpStatusCode.NotFound);
                }

                int idPerfil = (await _unitOfWork.GenericRepository<PerfilCandidato>()
                    .GetByCriteria(u => u.IdUsuario == idUsuario)).FirstOrDefault().Id;

                List<Postulacion> postulaciones = (await _unitOfWork
                    .GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        x => x.IdPerfilCandidato == idPerfil && x.FechaBaja == null &&
                        x.Oferta.FechaBaja == null &&
                        x.Oferta.PerfilEmpresa.FechaBaja == null &&
                        x.Oferta.FechaInicio <= DateTime.Now &&
                        (x.Oferta.FechaFin == null || x.Oferta.FechaFin >= DateTime.Now) &&
                        x.Oferta.PerfilEmpresa.IdEstadoValidacion == EstadoValidacion.IdEstadoAprobada,
                        q => q
                            .Include(p => p.Oferta)
                            .ThenInclude(to => to.TipoContrato)
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.PerfilEmpresa)
                            .Include(p => p.Oferta)
                                .ThenInclude(m => m.Modalidad)
                            .Include(p => p.Historial
                                .OrderByDescending(h => h.FechaModificacion)
                                .Take(1))
                                .ThenInclude(h => h.EstadoPostulacion)
                            .Include(p => p.PerfilCandidato)

                    )).OrderByDescending(f => f.FechaModificacion).ToList();


                return postulaciones.Adapt<List<PostulacionDTO>>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<PostulacionDTO> GetPostulacionById(int idPostulacion)
        {
            try
            {
                Postulacion _postulacion = await _unitOfWork.GenericRepository<Postulacion>().GetByIdIncludingSpecificRelations(idPostulacion,
                q => q.Include(h => h.Historial).ThenInclude(ep => ep.EstadoPostulacion)
                .Include(o => o.Oferta).ThenInclude(pf => pf.PerfilEmpresa)
                .Include(o => o.Oferta).ThenInclude(m => m.Modalidad)
                .Include(o => o.Oferta).ThenInclude(tc => tc.TipoContrato)
                .Include(o => o.Oferta).ThenInclude(l => l.Localidad).ThenInclude(p => p.Provincia).ThenInclude(pa => pa.Pais)
                .Include(pc => pc.PerfilCandidato)
                 );
                /*                 Postulacion _postulacion = await _unitOfWork.GenericRepository<Postulacion>().GetById(idPostulacion); 
                 */
                if (_postulacion == null)
                    throw new ApiException("No existe la postulación", (int)HttpStatusCode.NotFound);
                return _postulacion.Adapt<PostulacionDTO>();
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET último mes por idPerfilCandidato
        public async Task<IList<PostulacionDTO>> GetUltimoMesByEstudiante(int idPerfilCandidato, CancellationToken ct = default)
        {
            try
            {
                var desde = DateTime.UtcNow.AddDays(-30);

                var postulaciones = await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.IdPerfilCandidato == idPerfilCandidato &&
                             p.FechaBaja == null &&
                             p.FechaAlta >= desde,
                        include: q => q
                            .AsNoTracking()
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.PerfilEmpresa)
                                    .ThenInclude(pe => pe.Usuario)
                    );

                var postIds = postulaciones.Select(p => p.Id).Distinct().ToList();
                if (postIds.Count == 0) return new List<PostulacionDTO>();

                var historiales = await _unitOfWork.GenericRepository<PostulacionHistorial>()
                    .GetByCriteriaIncludingSpecificRelations(
                        h => postIds.Contains(h.IdPostulacion),
                        include: q => q
                            .AsNoTracking()
                            .Include(h => h.EstadoPostulacion)
                    );

                var estados = historiales
                    .GroupBy(h => h.IdPostulacion)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Where(h => h.FechaBaja == null)
                              .OrderByDescending(h => h.FechaAlta)
                              .Select(h => h.EstadoPostulacion.Nombre)
                              .FirstOrDefault()
                           ?? g.OrderByDescending(h => h.FechaAlta)
                               .Select(h => h.EstadoPostulacion.Nombre)
                               .FirstOrDefault()
                           ?? "En revisión"
                    );

                var ctx = new MapContext();
                ctx.Parameters["Estados"] = estados;

                var result = postulaciones
                .OrderByDescending(p => p.FechaModificacion)
                .ToList()
                .BuildAdapter()                           // <-- crea el adaptador para esta conversión
                .AddParameters("Estados", estados)        // <-- pasa parámetros al mapeo (MapContext.Parameters)
                .AdaptToType<List<PostulacionDTO>>();     // <-- destino

                return result;
            }
            catch (ApiException) { throw; }
            catch (Exception ex) { throw new ApiException(ex); }
        }

        public async Task<IList<PostulacionDTO>> GetPostulacionesPorOferta(int idOferta)
        {
            try
            {
                var postulaciones = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.IdOferta == idOferta && p.FechaBaja == null,
                        q => q
                            .Include(p => p.PerfilCandidato)
                                .ThenInclude(pc => pc.Usuario)
                            .Include(p => p.Historial)
                                .ThenInclude(h => h.EstadoPostulacion)
                    )).ToList();

                var resultado = postulaciones.Select(p => new PostulacionDTO
                {
                    Id = p.Id,
                    IdPerfilCandidato = p.IdPerfilCandidato,
                    IdOferta = p.IdOferta,
                    CartaPresentacion = p.CartaPresentacion,
                    Observacion = p.Observacion,
                    EstadoPostulacion = p.Historial?
                        .OrderByDescending(h => h.FechaAlta)
                        .FirstOrDefault()?.EstadoPostulacion?.Nombre ?? "Sin estado",
                    FechaPostulacion = p.FechaAlta.ToString("yyyy-MM-dd"),
                    TituloOferta = p.Oferta?.Titulo,
                    NombreEmpresa = p.Oferta?.PerfilEmpresa?.RazonSocial
                }).OrderByDescending(p => p.FechaPostulacion).ToList();

                return resultado;
            }
            catch (ApiException) { throw; }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener postulaciones de la oferta: {ex.Message}", ex);
            }
        }

        public async Task<IList<PostulacionCandidatoDTO>> GetPostulacionesCandidatosEmpresa(string email)
        {
            try
            {
                var empresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>()
                    .GetByCriteriaIncludingSpecificRelations(
                        e => e.Usuario.Email == email && e.FechaBaja == null,
                        q => q.Include(u => u.Usuario)
                    ))
                    .FirstOrDefault();

                if (empresa == null)
                    throw new Exception($"No se encontró ninguna empresa asociada al email: {email}");

                var postulaciones = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.Oferta.IdPerfilEmpresa == empresa.Id && p.FechaBaja == null,
                        q => q
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.Modalidad)
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.TipoContrato)
                            .Include(p => p.Oferta)
                                .ThenInclude(o => o.Localidad)
                                    .ThenInclude(l => l.Provincia)
                                        .ThenInclude(pr => pr.Pais)
                            .Include(p => p.Historial.OrderByDescending(h => h.FechaAlta).Take(1))
                                .ThenInclude(h => h.EstadoPostulacion)
                            .Include(p => p.PerfilCandidato)
                                .ThenInclude(pc => pc.Usuario)
                            .Include(p => p.PerfilCandidato)
                                .ThenInclude(pc => pc.Carrera)
                            .Include(p => p.PerfilCandidato)
                                .ThenInclude(pc => pc.Genero)
                    )).ToList();

                var result = postulaciones.Select(p => new PostulacionCandidatoDTO
                {
                    // Postulación
                    IdPostulacion = p.Id,
                    EstadoPostulacion = p.Historial
                        .OrderByDescending(h => h.FechaAlta)
                        .FirstOrDefault()?.EstadoPostulacion?.Nombre ?? "Sin estado",
                    Observacion = p.Observacion,
                    CartaPresentacion = p.CartaPresentacion,
                    FechaPostulacion = p.FechaAlta,

                    // Candidato
                    IdCandidato = p.PerfilCandidato?.Id ?? 0,
                    NombreCandidato = p.PerfilCandidato?.Usuario?.Nombre ?? "Candidato desconocido",
                    Email = p.PerfilCandidato?.Usuario?.Email,
                    DescripcionPerfil = p.PerfilCandidato?.Descripcion,
                    GeneroNombre = p.PerfilCandidato?.Genero?.Nombre,
                    CarreraNombre = p.PerfilCandidato?.Carrera?.Nombre,
                    AnioEgreso = p.PerfilCandidato?.AnioEgreso,
                    Cv = p.PerfilCandidato?.Cv != null ? Convert.ToBase64String(p.PerfilCandidato.Cv) : null,

                    // Oferta
                    IdOferta = p.Oferta?.Id ?? 0,
                    TituloOferta = p.Oferta?.Titulo,
                    DescripcionOferta = p.Oferta?.Descripcion,

                    // Extras
                    Modalidad = p.Oferta?.Modalidad?.Nombre,
                    TipoContrato = p.Oferta?.TipoContrato?.Nombre,
                    Localidad = p.Oferta?.Localidad?.Nombre,
                    Provincia = p.Oferta?.Localidad?.Provincia?.Nombre,
                    Pais = p.Oferta?.Localidad?.Provincia?.Pais?.Nombre,
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener postulaciones de la empresa: {ex.Message}", ex);
            }
        }

        public async Task CambiarEstadoPostulacion(int idPostulacion, string nombreEstado, string emailEmpresa)
        {
            bool commitRealizado = false;

            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // 1️⃣ Validar empresa existente
                var empresa = (await _unitOfWork.GenericRepository<PerfilEmpresa>()
                    .GetByCriteriaIncludingSpecificRelations(
                        e => e.Usuario.Email == emailEmpresa && e.FechaBaja == null,
                        q => q.Include(e => e.Usuario)
                    ))
                    .FirstOrDefault();

                if (empresa == null)
                    throw new ApiException($"No se encontró ninguna empresa activa asociada al email '{emailEmpresa}'.", 
                        (int)HttpStatusCode.NotFound);

                // 2️⃣ Validar que la postulación exista y pertenezca a una oferta de la empresa
                var postulacion = (await _unitOfWork.GenericRepository<Postulacion>()
                    .GetByCriteriaIncludingSpecificRelations(
                        p => p.Id == idPostulacion && p.FechaBaja == null,
                        q => q.Include(p => p.Oferta)
                    ))
                    .FirstOrDefault();

                if (postulacion == null)
                    throw new ApiException($"No se encontró la postulación con ID {idPostulacion}.", 
                        (int)HttpStatusCode.NotFound);

                if (postulacion.Oferta == null || postulacion.Oferta.IdPerfilEmpresa != empresa.Id)
                    throw new ApiException("La postulación no pertenece a una oferta publicada por esta empresa.", 
                        (int)HttpStatusCode.Forbidden);

                // 3️⃣ Buscar el estado por nombre (tabla EstadoPostulacion)
                var estado = (await _unitOfWork.GenericRepository<EstadoPostulacion>()
                    .GetByCriteria(e => e.Nombre.ToLower() == nombreEstado.ToLower()))
                    .FirstOrDefault();

                if (estado == null)
                    throw new ApiException($"No se encontró un estado con el nombre '{nombreEstado}'.", 
                        (int)HttpStatusCode.NotFound);

                // 4️⃣ Crear nuevo registro en PostulacionHistorial
                PostulacionHistorial historial = new()
                {
                    IdPostulacion = postulacion.Id,
                    IdEstadoPostulacion = estado.Id,
                    Motivo = "Interacción empresa",
                    FechaAlta = DateTime.Now,
                    FechaModificacion = DateTime.Now,
                    FechaBaja = null
                };

                await _unitOfWork.GenericRepository<PostulacionHistorial>().Insert(historial);

                // 5️⃣ Guardar cambios y confirmar transacción
                await _unitOfWork.CommitAsync();
                commitRealizado = true;
            }
            catch (ApiException)
            {
                throw; // No envolvemos ApiException para preservar su mensaje y código
            }
            catch (Exception ex)
            {
                throw new ApiException($"Error al cambiar el estado de la postulación: {ex.Message}");
            }
            finally
            {
                if (!commitRealizado)
                    await _unitOfWork.RollbackAsync();
            }
        }
    }
}