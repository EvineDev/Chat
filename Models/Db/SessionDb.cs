using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Service;
using System.ComponentModel.DataAnnotations;

namespace Chat.Db
{
    public class SessionDb : BaseDb
    {
        [Required]
        public UserDb User { get; set; }

        public byte[] SessionKey { get; set; }
        
        [Required]
        public DateTime Created { get; set; }

        [Required]
        // Fix: RefreshKey should perhaps be deleted after some time. Perhaps after a new key is generated and the new key has been send to the server again.
        //      Perhaps have a "next" refresh key
        // Fix: Race conditions, if 2 requests goes at the same time it will generate 2 refresh-keys at once.
        //      accept, but don't generate a new refresh-key to soon after the previously generated refresh-key. (10 second timeout)
        public byte[] RefreshKey { get; set; }

        [Required]
        public byte[] RefreshSalt { get; set; }

        //public SessionDb NextSession { get; set; }
    }
}
