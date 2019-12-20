using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using Chat.Dto;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Chat.Controllers
{
	public class IndexController : ControllerBase
	{
        private readonly AuthService authService;
        private readonly ChatService chatService;
        private readonly SessionService sessionService;
        private readonly UploadService uploadService;

        public IndexController(AuthService authService, ChatService chatService, SessionService sessionService, UploadService uploadService)
		{
            this.authService = authService;
            this.chatService = chatService;
            this.sessionService = sessionService;
            this.uploadService = uploadService;
        }

		[HttpGet]
        [Route("")]
        public ContentResult Html()
		{
            string html;

            if (sessionService.IsAuthenticated())
                html = System.IO.File.ReadAllText("Web/index.html");
            else
                html = System.IO.File.ReadAllText("Web/landing.html");

            return new ContentResult
            {
                ContentType = "text/html",
                Content = html,
            };
		}

        [HttpGet]
        [Route("style.css")]
        public ContentResult Style()
        {
            var css = System.IO.File.ReadAllText("Web/style.css");

            return new ContentResult
            {
                ContentType = "text/css",
                Content = css,
            };
        }

        [HttpPost]
        [Route("send-message")]
        public ActionResult SendMessage([FromForm]string message)
        {
            chatService.SendMessage("main", message);
            return new RedirectResult("/");
        }

        [HttpPost]
        [Route("login")]
        public ActionResult Login([FromForm]string email, [FromForm]string password, [FromForm]string remember)
        {
            var user = new AccessDto
            {
                Email = email,
                Password = password,
                RememberMe = remember == "on" ? true : false,
            };

            if (authService.Login(user))
            {
                return new RedirectResult("/");
            }
            else
            {
                var html = System.IO.File.ReadAllText("Web/index.html");

                return new ContentResult
                {
                    StatusCode = 401,
                    ContentType = "text/html",
                    Content = html,
                };
            }
        }

        [HttpPost]
        [Route("logout")]
        public ActionResult Logout()
        {
            authService.Logout();
            return new RedirectResult("/");
        }

        [HttpPost]
        [Route("register")]
        public ActionResult Register([FromForm]string email, [FromForm]string username, [FromForm]string password)
        {
            if (authService.Register(email, username, password))
            {
                return new RedirectResult("/");
            }
            else
            {
                // Fix: user already exists
                return new RedirectResult("/");
            }
        }

        [HttpPost]
        [Route("upload-avatar")]
        public async Task<ActionResult> UploadAvatar(IFormFile avatar)
        {
            var success = await uploadService.UploadAvatar(avatar);

            return new RedirectResult("/");
        }
    }
}
