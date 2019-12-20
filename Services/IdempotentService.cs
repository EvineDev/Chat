using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Db;
using Chat.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace Chat.Service
{
    // Ensure duplicated requests will be filtered.
    public class IdempotentService
    {
        private RequestDelegate next;
        HttpContext context;

        public IdempotentService(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            // TODO: Implement
            this.context = context;
            await next(context);
        }
    }
}
