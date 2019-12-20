using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using Chat.Dto;
using Microsoft.Extensions.Configuration;

namespace Chat.Controllers
{
	[Route("api")]
	[ApiController]
	public class TestController : ControllerBase
	{
		private MailService mailService;
        private IConfiguration config;

        public TestController(MailService mailService, IConfiguration config)
		{
			this.mailService = mailService;
            this.config = config;
        }

		[HttpPost]
		[Route("test/mail")]
		public void SendMailTest()
		{
			mailService.SendMailTest(config["Mail:TestUser"], "Test Mail", "<h1>Test Title</h1>\n<p>yus 2</p>");
		}
    }
}
