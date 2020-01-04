using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using Chat.Dto;

namespace Chat.Controllers
{
	[Route("api")]
	[ApiController]
	public class ChatController : ControllerBase
	{
		private ChatService chatService;

		public ChatController(ChatService chatService)
		{
			this.chatService = chatService;
		}

		[HttpGet]
		[Route("message-history/{board}")]
		public ActionResult<IEnumerable<MessageDto>> History(string board)
		{
			return chatService.GetHistory(board);
		}

		[HttpGet]
		[Route("emote/{emote}")]
		public IActionResult Emote(string emote)
		{
			BinaryDto avatar;
			if (emote == "yay")
				avatar = chatService.GetAvatar(Guid.Parse("aefee463-0294-47ad-b936-f1418515d0f9"));
			else
				return new ContentResult { StatusCode = 404 };

			return new FileContentResult(avatar.Data, avatar.ContentType);
		}

		[HttpPost]
		[Route("send-message")]
		public void SendMessage([FromBody]MessageDto value)
		{
			chatService.SendMessage(value.Board, value.Message);
		}

        [HttpGet]
        [Route("active-users/{board}")]
        public ActionResult<List<UserDto>> ActiveUsers(string board)
        {
            return chatService.GetActiveUsers(board);
        }

        [HttpGet]
        [Route("avatar/{userId}")]
        public IActionResult Avatar(Guid userId)
        {
            var avatar = chatService.GetAvatar(userId);

            return new FileContentResult(avatar.Data, avatar.ContentType);
        }
	}
}
