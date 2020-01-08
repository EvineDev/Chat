using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using System.ComponentModel.DataAnnotations;

namespace Chat.Db
{
	public class MessageDb : BaseDb
	{
		[Required]
		public UserDb User { get; set; }

		public SessionDb Session { get; set; }

        [Required]
        public string Board { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public DateTime Created { get; set; }
    }
}
