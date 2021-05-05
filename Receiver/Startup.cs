using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.Http;

namespace Receiver
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Example Event Receiver");
                });

                endpoints.MapPost("/", async context =>
                {
                    logger.LogInformation("Request received via {Protocol}", context.Request.Protocol);

                    using StreamContent content = new StreamContent(context.Request.Body);
                    string body = await content.ReadAsStringAsync();

                    using StreamWriter outputFile = new StreamWriter($"{DateTime.Now:yyyy-MM-dd--HH-mm-ss-fff}.txt");
                    await outputFile.WriteAsync(body);
                });
            });
        }
    }
}
