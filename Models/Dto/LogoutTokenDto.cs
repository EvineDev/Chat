using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Chat.Dto
{
	public class LogoutTokenDto
	{
		[JsonPropertyName("id")]
		public Guid Id { get; set; }

		[JsonPropertyName("tokenKey")]
		public string TokenKey { get; set; }
	}
}
