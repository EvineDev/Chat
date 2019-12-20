using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Chat.Dto
{
	public class MessageDto {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("board")]
        public string Board { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }
    }
}
