using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class EnvioDTO
    {
        public int Id { get; set; }

        public virtual DomicilioDTO Domicilio { get; set; }
        public virtual EstadoEnvioDTO EstadoEnvio { get; set; }

        public virtual string DescripcionEnvio { get { 
            if (EstadoEnvio.IdEstadoEnvio == 1)
            {
                return "En preparación";
            }
            else if (EstadoEnvio.IdEstadoEnvio == 2)
            {
                return "En camino";
            }
            else if (EstadoEnvio.IdEstadoEnvio == 3)
            {
                return "Entregado";
            }
            else
            {
                return "Cancelado";
            }

         } }


    }
}

