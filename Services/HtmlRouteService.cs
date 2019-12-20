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
    // Routes post request as rest request. For pure html pages.
    // Post: /fput/api/user -> Put: /api/user

    // TODO: I think I need to redirect the user with a get request after the post request has been completed.
    //       Should I just assume the post request can't have a redirect itself?
    //       Internal vs external domain.

    public class HtmlRouteService
    {
        private readonly RequestDelegate next;
        HttpContext context;

        private readonly List<string> methodList = new List<string>
        {
            "get",
            "post",
            "put",
            "head",
            "delete",
            "patch",
            "options",
            "trace",
        };

        public HtmlRouteService(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            this.context = context;
            if (String.Equals(context.Request.Method, "post", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var method in methodList)
                {
                    if (FixupUrl(method))
                    {
                        await next(context);
                        return;
                    }
                }
            }

            await next(context);
        }

        private bool FixupUrl(string method)
        {
            if (context.Request.Path.StartsWithSegments("/F" + method))
            {
                var urlPath = context.Request.Path.ToUriComponent();
                var newPath = urlPath.Substring(method.Length + 2);

                context.Request.Path = newPath;
                context.Request.Method = method;

                return true;
            }

            return false;
        }
    }
}
