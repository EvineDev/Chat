using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using Chat.Dto;
using Chat.Db;
using Microsoft.AspNetCore.Http;

namespace Chat.Controllers
{
	[Route("api")]
	[ApiController]
	public class AuthController : ControllerBase
	{
        private AuthService authService;

		public AuthController(AuthService authService)
		{
            this.authService = authService;
        }

		[HttpPost]
		[Route("login")]
		public ActionResult<string> Login([FromBody]AccessDto user)
		{
            if (authService.Login(user))
			    return new OkResult();
            else
                return new UnauthorizedResult();
		}

		[HttpPost]
		[Route("logout")]
		public ActionResult Create([FromBody]UserDto user)
		{

			return new OkResult();
		}

		[HttpPut]
		[Route("update")]
		public ActionResult Update([FromBody]UserDto user)
		{

			return new OkResult();
		}

		[HttpDelete]
		[Route("delete")]
		public ActionResult Delete(Guid userId)
		{
			return new OkResult();
		}
	}

	public class AccessDto
	{
        public string Email { get; set; }

        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
