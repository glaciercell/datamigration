using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

namespace datamigration
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IDocumentClient DocumentClient { get;set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
           {
               c.SwaggerDoc("v1", new Info { Title = "Database Migrator", Version = "v1" });
           });

            services.AddMvc();
            services.AddMemoryCache();
            services.AddSingleton<IConfiguration>(Configuration);

            var endpoint = Configuration.GetSection("documentdb").GetSection("source").GetSection("endpointurl").Value;
            var authkey = Configuration.GetSection("documentdb").GetSection("source").GetSection("authkey").Value;

            DocumentClient = new DocumentClient(new Uri(endpoint), authkey);
            services.AddSingleton<IDocumentClient>(DocumentClient);
         //   services.AddSingleton<IDocumentClient>(DocumentClient => new DocumentClient(new Uri(endpoint), authkey));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseStaticFiles();
                app.UseMvc();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint(Configuration["swagger:hostvirtualapplication"] + "/swagger/v1/swagger.json", Configuration["Swagger:Description"]);
                    });
                app.UseMvc(routes =>
           {
               routes.MapRoute(
                         name: "Migrator",
                         template: "{controller=Migrator}");
           });
            }
        }
    }
}
