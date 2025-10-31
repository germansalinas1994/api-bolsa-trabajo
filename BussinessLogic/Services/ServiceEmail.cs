using Microsoft.Extensions.Options;
using BussinessLogic.DTO.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using AutoWrapper.Wrappers;

namespace BussinessLogic.Services
{
    public class ServiceEmail
    {
        private readonly IConfiguration _config;

        public ServiceEmail(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
        {
            try
            {
                var remitente = _config["EmailSettings:Remitente"];
                var servidor = _config["EmailSettings:SmtpServer"];
                var puerto = int.Parse(_config["EmailSettings:SmtpPort"]);
                var usuario = _config["EmailSettings:SmtpUsername"];
                var clave = _config["EmailSettings:SmtpPasswordFactores"];

                var email = new MimeMessage();
                email.From.Add(new MailboxAddress("UTN Bolsa de Trabajo", remitente)); // 👈 nombre + mail
                email.To.Add(MailboxAddress.Parse(destinatario));
                email.Subject = asunto;
                email.Body = new TextPart("html") { Text = cuerpoHtml };

                using var smtp = new SmtpClient();

                Console.WriteLine($"Conectando a {servidor}:{puerto}...");
                await smtp.ConnectAsync(servidor, puerto, SecureSocketOptions.StartTls);

                Console.WriteLine("Autenticando...");
                await smtp.AuthenticateAsync(usuario, clave);

                Console.WriteLine("Enviando correo...");
                await smtp.SendAsync(email);

                Console.WriteLine("Correo enviado correctamente");
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error al enviar correo: {ex.Message}");
                throw new Exception("Error al enviar el correo electrónico.", ex);
            }
        }
    }
}