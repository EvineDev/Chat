using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using Chat.Dto;
using Chat.Db;
using Microsoft.AspNetCore.Http;

// Implementing this before other code feels so imprecise. I need to know what other code is reliant on.
// There is stuff like history of passwords / username that might be wanted to be stored seperatly.
namespace Chat.Controllers
{
	[Route("api/account")]
	[ApiController]
	public class AccountController : ControllerBase
	{
		public AccountController(IHttpContextAccessor httpcontextaccessor)
		{
			Console.WriteLine(httpcontextaccessor);
		}

		[HttpPost]
		[Route("{username}")]
		public ActionResult<string> Read(string username)
		{

			return new OkResult();
		}

		[HttpPost]
		[Route("create")]
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
}
