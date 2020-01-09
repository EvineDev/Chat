using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using System.ComponentModel.DataAnnotations;

namespace Chat.Db
{
    public class LogoutTokenDb : BaseDb
    {
        [Required]
        public UserDb User { get; set; }

        [Required]
        public DateTime Created { get; set; }

		[Required]
        public byte[] TokenKey { get; set; }
	}
}
