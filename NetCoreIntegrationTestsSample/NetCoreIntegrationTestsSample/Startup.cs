using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using NetCoreIntegrationTestsSample.Expenses;
using NetCoreIntegrationTestsSample.Infrastructure;

namespace NetCoreIntegrationTestsSample
{
    public class Startup
    {
        private IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            _configuration = (IConfiguration) serviceProvider.GetService(typeof(IConfiguration));
            
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new OpenApiInfo()
                        {
                            Title = "IntegrationTestsSample",
                            Version = "v1"
                        }
                    );
                    c.CustomSchemaIds(x => x.FullName);
                    
                }
            );

            services.AddControllers();
       
            var rabbitMqConfig = _configuration.GetSection(ConfigKeys.RabbitMq).Get<RabbitMqConfig>();
                
            services.SetupMassTransit(rabbitMqConfig);
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterInstance(_configuration);
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.Application).Get<ApplicationConfig>());
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.Database).Get<DatabaseConfig>());
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.RabbitMq).Get<RabbitMqConfig>());
            builder.RegisterInstance(_configuration.GetSection(ConfigKeys.ExchangeService).Get<ExchangeServiceConfig>());
            
            builder.RegisterModule(new NetCoreIntegrationTestsSampleAutofacModule());
            
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ErrorHandlingMiddleware>();
            
            app.UseRouting();

            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context => { await context.Response.WriteAsync("Hello World!"); });
                endpoints.MapControllers();
            });
            
            app.UseSwagger();

            app.UseSwaggerUI(
                c => c.SwaggerEndpoint(
                    "/swagger/v1/swagger.json", "IntegrationTestsSample v1"
                )
            );
            
        }
    }
}