using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Shared.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Kafka
{
    public class KafkaProducer<TMessage> : IKafkaProducer<TMessage>
    {
        private readonly IProducer<Guid, TMessage> _producer;
        private readonly string _topic;
        public KafkaProducer(IOptions<KafkaSettings> settings) 
        {
            var conf = new ProducerConfig()
            {
                BootstrapServers = settings.Value.BootstrapServers,
            };
            _producer = new ProducerBuilder<Guid, TMessage>(conf)
                .SetValueSerializer(new KafkaJsonSerializer<TMessage>())
                .SetKeySerializer(new KafkaGuidSerializer())
                .Build();

            _topic = settings.Value.Topic;

        }

        public async Task ProduceAsync(Guid key, TMessage message, CancellationToken cancellationToken)
        {
            await _producer.ProduceAsync(_topic, new Message<Guid, TMessage>()
            {
                Key = key,
                Value = message
            }, cancellationToken);
        }

        public void Dispose()
        {
            _producer.Dispose();
        }
    }
}
