using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Chat.Service;
using Chat.Db;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Chat
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(Configuration["ConnectionString"]));

            services.AddScoped<MailService>();
            services.AddScoped<AuthService>();
            services.AddScoped<SessionService>();
            services.AddScoped<ChatService>();
            services.AddScoped<UploadService>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (String.Equals(env.EnvironmentName, "Development", StringComparison.InvariantCultureIgnoreCase))
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<HtmlRouteService>();
            app.UseMiddleware<AccessService>();
            app.UseMiddleware<IdempotentService>();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.Run(async context =>
            {
                context.Response.StatusCode = 404;
                //context.Response.ContentType = "text/json";
                var b = Encoding.ASCII.GetBytes("Not found");
                await context.Response.Body.WriteAsync(b, 0, b.Length);
            });
        }
    }
}
