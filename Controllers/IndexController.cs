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
                html = System.IO.File.ReadAllText("Web/chat.html");
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

		[HttpGet]
		[Route("message-history/{board}")]
		public ContentResult History([FromRoute]string board)
		{
			var history = chatService.GetHistory(board);
			var result = new ContentResult { ContentType = "text/html", Content = "" };

			foreach (var message in history)
			{
				result.Content += RenderHtmlMessage(message);
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
				result.Content += RenderHtmlMessage(message);
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
				result.Content += RenderHtmlUser(user);
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


		// Fix: Move to helper class.
		private string RenderHtmlMessage(MessageDto message)
		{
			var messageHtml = EncodeHtml(message.Message);
			var messageFinal = RenderHtmlUrl(messageHtml);

			var html = @$"
<div class='message-overall-container' data-message-id='{message.Id}'>
	<div class='message-container'>
		<span class='message-avatar-container'><img class='message-avatar' src='/api/avatar/{message.AvatarId}'></span>
		<p class='message-content'>
			<time class='message-time' datetime='{message.Created.ToString("yyyy-MM-ddTHH:mm:ssZ")}'>({message.Created.ToString("HH:mm")} UTC)</time>
			<span class='message-username'>{EncodeHtml(message.Username)}</span>:
			<span class='message-text'>{messageFinal}</span>
		</p>
	</div>
	{RenderHtmlMessageImages(message)}
</div>";

			return html;
		}

		private string RenderHtmlUser(UserDto user)
		{
			var html = $@"
<div class='userlist-container'>
	<span class='userlist-avatar-container'><img class='userlist-avatar' src='/api/avatar/{user.AvatarId}'></span>
	<span>{EncodeHtml(user.Username)}</span>
</div>";

			return html;
		}

		private string RenderHtmlUrl(string message)
		{
			var regex = new Regex("url\"(.*?)\"");
			var result = regex.Replace(message, x => {
				var urlText = x.Groups[1].ToString();
				var urlLink = urlText;
				// Fix: This is sorta ghetto, but it works as long as there isn't any :// in another part of the url.
				if (urlLink.Contains("://") == false)
					urlLink = "https://" + urlLink;

				var html = $"<a href='{EncodeUrl(urlLink)}' target='_blank' rel='noreferrer noopener'>{EncodeHtml(urlText)}</a>";
				return html;
			});

			return result;
		}

		private string RenderHtmlMessageImages(MessageDto message)
		{
			var result = "";
			var regex = new Regex("img\"(.*?)\"");
			var matchList = regex.Matches(message.Message);
			foreach (Match match in matchList)
			{
				result += $"<div class='image-container'><img class='image-message' src='{EncodeUrl(match.Groups[1].ToString())}'></div>";
			}

			return result;
		}

		private string EncodeHtml(string text)
		{
			// Fix: make good
			return text
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}

		private string EncodeUrl(string text)
		{
			// Fix: make good
			return text
				.Replace("\"", "%22")
				.Replace("\'", "%27")
				.Replace("`", "%60")
				.Replace("\\", "\\\\");
		}

	}
}
