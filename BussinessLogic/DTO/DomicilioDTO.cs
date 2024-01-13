using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class DomicilioDTO
    {

        public int IdDomicilio { get; set; }

        public string Altura { get; set; }

        public string? Calle { get; set; }

        public string? Aclaracion { get; set; }

        public string? Departamento { get; set; }
        public string CodigoPostal { get; set; }

        public string DescripcionCompleta
        {
            get
            {

                if(!Calle.Contains("Calle"))
                {
                    return "Calle" + " " + Calle + " " + "Nro" + " " + Altura + " " + Departamento;
                }
                else{
                    return Calle + " " + "Nro" + " " + Altura + " " + Departamento;
                }

            }
        }


    }
}

