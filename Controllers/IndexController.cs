using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using Chat.Dto;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;

namespace Chat.Controllers
{
	public class IndexController : ControllerBase
	{
        private readonly AuthService authService;
        private readonly ChatService chatService;
        private readonly SessionService sessionService;
        private readonly UploadService uploadService;
		private readonly FragmentService fragmentService;

		public IndexController(AuthService authService, ChatService chatService, SessionService sessionService, UploadService uploadService, FragmentService fragmentService)
		{
            this.authService = authService;
            this.chatService = chatService;
            this.sessionService = sessionService;
            this.uploadService = uploadService;
			this.fragmentService = fragmentService;
        }

		[HttpGet]
        [Route("")]
        public ContentResult Html()
		{
            string html;

			if (sessionService.IsAuthenticated())
			{
				var history = chatService.GetHistory("main");
				var userList = chatService.GetActiveUsers("main");
                html = fragmentService.Chat(history, userList);
			}
            else
			{
                html = fragmentService.Landing();
			}

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

		[HttpGet]
		[Route("main.js")]
		public ContentResult MainJs()
		{
			var js = System.IO.File.ReadAllText("Web/main.js");

			return new ContentResult
			{
				ContentType = "text/javascript",
				Content = js,
			};
		}

		[HttpGet]
		[Route("message-history/{board}")]
		public ContentResult History([FromRoute]string board)
		{
			var history = chatService.GetHistory(board);
			var result = new ContentResult { ContentType = "text/html", Content = "" };

			foreach (var message in history)
			{
				result.Content += fragmentService.RenderHtmlMessage(message);
			}

			return result;
		}

		[HttpGet]
		[Route("message-poll/{board}/{lastId}")]
		public ContentResult HistoryPoll([FromRoute]string board, [FromRoute]Guid? lastId)
		{
			if (lastId == null)
			{
				return new ContentResult { ContentType = "text/html", Content = "", StatusCode = 204 };
			}

			var history = chatService.GetHistory(board, lastId);
			if (history.Any() == false)
			{
				return new ContentResult { ContentType = "text/html", Content = "", StatusCode = 204 };
			}

			var result = new ContentResult { ContentType = "text/html", Content = "" };

			foreach (var message in history)
			{
				result.Content += fragmentService.RenderHtmlMessage(message);
			}

			return result;
		}

		[HttpGet]
		[Route("active-users/{board}")]
		public ContentResult ActiveUsers([FromRoute]string board)
		{
			var userList = chatService.GetActiveUsers(board);
			var result = new ContentResult { ContentType = "text/html", Content = "" };

			foreach (var user in userList)
			{
				result.Content += fragmentService.RenderHtmlUser(user);
			}

			return result;
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
                var html = System.IO.File.ReadAllText("Web/chat.html");

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
        public ActionResult Register([FromForm]string email, [FromForm]string username, [FromForm]string password, [FromForm]string capcha)
        {
            if (authService.Register(email, username, password, capcha))
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
