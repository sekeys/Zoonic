using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.Features;
using Zoonic.Web;
using Zoonic.Web.Handlers;

namespace Common
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)

                .AddEnvironmentVariables();

            //Zoonic.Configuration.ConfigurationStartup.RootConfigurePath =
            //  System.IO.Path.Combine(env.ContentRootPath, "setting.json");
            Zoonic.Web.Route.ProcessorFactory.Register(new Zoonic.Web.Route.DefaultProcessorFactory());
            Zoonic.Web.Route.ProcessorFactory.Factory.Cache("test",typeof(TestProcessor));
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; private set; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.Configure<FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
                options.BufferBodyLengthLimit = int.MaxValue;
            });
            services.AddCors();
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            
            app.UseZoonic(new RouteHandler(),new ProcessHandler());
        }
    }
}
