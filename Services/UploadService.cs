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
using System.IO;
using Microsoft.AspNetCore.Http;

namespace Chat.Service
{
    public class UploadService
    {
        private DatabaseContext dbContext;
        private SessionService sessionService;

        public UploadService(DatabaseContext dbContext, SessionService sessionService)
        {
            this.dbContext = dbContext;
            this.sessionService = sessionService;
        }

        public async Task<bool> UploadAvatar(IFormFile avatar)
        {
            var allowList = new string[] { "image/png", "image/jpg", "image/jpeg" };

            var contentTypeFiltered = allowList
                .Where(x => String.Equals(avatar.ContentType, x, StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault();

            if (contentTypeFiltered == null)
                throw new Exception("Content type not allowed");

            // Fix: How do I ensure that the image isn't malicious file of any kind.
            //      Can a file that has the img tag be malicious?
            if (avatar.Length > 512L * 1024L)
                return false;

            using var mem = new MemoryStream();
            await avatar.CopyToAsync(mem);

            var image = mem.ToArray();

            var session = sessionService.GetSession();
            session.User.Avatar = image;
            session.User.ContentType = contentTypeFiltered;
            dbContext.SaveChanges();

            return true;
        }

        private string CheckContentType(string input, string type)
        {
            if (String.Equals(input, type, StringComparison.InvariantCultureIgnoreCase))
                return type;

            return null;
        }
    }
}
