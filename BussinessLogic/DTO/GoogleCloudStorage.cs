using System;
using DataAccess.Entities;

namespace BussinessLogic.DTO
{
    public class GoogleCloudStorage
    {
        public string BucketName { get; set; }
        public string CredentialsFilePath { get; set; }
    }
}

