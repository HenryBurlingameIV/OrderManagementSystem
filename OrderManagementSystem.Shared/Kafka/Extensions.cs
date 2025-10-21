using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Kafka
{
    public static class Extensions
    {
        public static void AddProducer<TMessage>(this IServiceCollection services, IConfigurationSection config, string configSectionName)
        {
            services.Configure<KafkaSettings>(configSectionName, config);


            services.AddSingleton<IKafkaProducer<TMessage>, KafkaProducer<TMessage>>(provider =>
            {
                var settings = provider.GetRequiredService<IOptionsMonitor<KafkaSettings>>();
                return new KafkaProducer<TMessage>(settings, configSectionName);
            });
        }

        public static void AddConsumer<TMessage, THandler>(this IServiceCollection services, IConfigurationSection config) where THandler : class, IMessageHandler<TMessage>
        {
            services.Configure<KafkaConsumerSettings>(config);
            services.AddHostedService<KafkaConsumer<TMessage>>();
            services.AddScoped<IMessageHandler<TMessage>, THandler>();
        }
    }
}
