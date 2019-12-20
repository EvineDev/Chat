using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Db;
using Chat.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace Chat.Service
{
    public class AccessService
    {
        private readonly RequestDelegate next;

        private readonly List<string> publicPaths = new List<string>
        {
            "/",
            "/style.css",

            "/login",
            "/logout",
            "/register",
            "/api/login",
        };

        public AccessService(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, SessionService sessionService, AuthService authService)
        {
            var cookie = context.Request.Cookies[AuthService.AUTH_SESSION];

            var session = authService.AuthenticateSession();
            if (session != null)
            {
                sessionService.SetSession(session);
                await next(context);
            }
            else if (IsPublicPath(context))
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = 401;
            }
        }

        private bool IsPublicPath(HttpContext context)
        {
            var path = context.Request.Path;
            foreach (var publicPath in publicPaths)
            {
                if (String.Equals(path, publicPath, StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}
