using System;
using System.Security.Authentication;
using GreenPipes;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NetCoreIntegrationTestsSample.Expenses;

namespace NetCoreIntegrationTestsSample.Infrastructure
{
    public static class MassTransitSetupExtensions
    {
        public static void SetupMassTransit(
            this IServiceCollection services,
            RabbitMqConfig rabbitMqConfig)
        {
            services.AddMassTransit(configurator =>
            {
                configurator.AddConsumer<CreateExpenseConsumer>();

                EndpointConvention.Map<Contracts.Expenses.V1.Messages.CreateExpenseCommand>(
                    new Uri($"exchange:{rabbitMqConfig.LocalQueue}"));
                
                configurator.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.SetQueueArgument("x-expires", null);
                        cfg.AutoDelete = false;
                        cfg.Durable = true;
                        cfg.PrefetchCount = 10;
                        
                        cfg.UseMessageRetry(r => r.Interval(3, 100));
    
                        cfg.Host(new Uri(rabbitMqConfig.HostUrl), h => {
    
                            h.Heartbeat(5);

                            h.Username(rabbitMqConfig.User);
                            h.Password(rabbitMqConfig.Password);
                        });
                        
                        cfg.ReceiveEndpoint(rabbitMqConfig.LocalQueue, e =>
                        {
                            e.ConfigureConsumeTopology = false;
                            e.ConfigureConsumer<CreateExpenseConsumer>(context);
                        });
                    });     
            });

            services.AddMassTransitHostedService();
        }
    }
}