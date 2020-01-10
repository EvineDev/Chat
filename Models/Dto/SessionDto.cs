using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Chat.Dto
{
	public class SessionDto
	{
		[JsonPropertyName("id")]
		public Guid Id { get; set; }

		[JsonPropertyName("session-key")]
		public string SessionKey { get; set; }

		[JsonPropertyName("refresh-key")]
		public string RefreshKey { get; set; }
	}
}
