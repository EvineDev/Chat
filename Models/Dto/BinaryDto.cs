using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Chat.Dto
{
	public class BinaryDto {
        [JsonPropertyName("contentType")]
        public string ContentType { get; set; }

        [JsonPropertyName("data")]
        public byte[] Data { get; set; }
    }
}
