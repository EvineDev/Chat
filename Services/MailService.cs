using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Db;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;

namespace Chat.Service
{
    public class MailService
    {
        public IConfiguration config;
        private readonly IHostEnvironment env;

        // Fix: Is "IWebHostEnvironment" preferable over "IHostEnvironment"?
        public MailService(IConfiguration config, IHostEnvironment env)
        {
            this.config = config.GetSection("Mail");
            this.env = env;
        }

        public void SendMailTest(string to, string subject, string content)
        {
            if (env.IsDevelopment() == false)
                throw new Exception("Don't. Test mail is disabled");

            var mail = new MailMessage
            {
                From = new MailAddress(config["Username"]),
                Subject = subject,
                IsBodyHtml = true,
                Body = content,
            };
            mail.To.Add(to);

            using var SmtpServer = new SmtpClient(config["Host"])
            {
                Port = int.Parse(config["Port"]),
                Credentials = new System.Net.NetworkCredential(config["Username"], config["Password"]),
                EnableSsl = true
            };

            SmtpServer.Send(mail);
        }
    }
}
