using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using System.ComponentModel.DataAnnotations;

namespace Chat.Db
{
	public class BaseDb
    {
        [Required]
        public Guid Id { get; set; }
	}
}
