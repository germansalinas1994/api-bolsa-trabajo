using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class MercadoPagoDevSettings
    {
        public string AccessToken { get; set; }
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; }
        public string PendingUrl { get; set; }

        public string NotificationUrl { get; set; }
    }
}

