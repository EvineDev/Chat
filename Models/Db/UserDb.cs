using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using System.ComponentModel.DataAnnotations;

namespace Chat.Db
{
    public class UserDb : BaseDb
    {
        // Fix: Do username#0000, make sure the user generated is unique.
        [Required]
        public string Username { get; set; }

        // Fix: Allow multiple Emails
        // Fix: Is there a way to say emails must unique when it's stored
        [Required]
        public string Email { get; set; }

        [Required]
        public byte[] Password { get; set; }

        [Required]
        public byte[] Salt { get; set; }

        public byte[] Avatar { get; set; }

        public string ContentType { get; set; }
    }
}
