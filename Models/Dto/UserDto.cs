using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Chat.Dto
{
	public class UserDto
	{
        [JsonPropertyName("avatarId")]
        public Guid AvatarId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
}
