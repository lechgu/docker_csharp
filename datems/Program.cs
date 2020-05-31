using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace datems
{
    class Program
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
        static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder
                    .UseUrls("http://0.0.0.0:5000")
                    .UseStartup<Program>();
            });

            builder.Build().Run();
        }
    }

    public class TimeResponse
    {
        public DateTime Current { get { return DateTime.UtcNow; } }
    }
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Ok(new TimeResponse());
        }
    }
}