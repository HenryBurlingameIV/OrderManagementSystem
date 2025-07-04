using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public static void AddProducer<TMessage>(this IServiceCollection services, IConfigurationSection config)
        {
            services.Configure<KafkaSettings>(config);
            services.AddSingleton<IKafkaProducer<TMessage>, KafkaProducer<TMessage>>();
        }
    }
}
