using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OrderManagementSystem.Shared.Contracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Kafka
{
    public class KafkaConsumer<TMessage> : BackgroundService
    {
        private readonly string _topic;
        private readonly IConsumer<string, TMessage> _consumer;
        private readonly IMessageHandler<TMessage> _handler;

        public KafkaConsumer(IOptions<KafkaConsumerSettings> settings, IMessageHandler<TMessage> handler)
        {
            var conf = new ConsumerConfig()
            {
                BootstrapServers = settings.Value.BootstrapServers,
                GroupId = settings.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _topic = settings.Value.Topic;

            _consumer = new ConsumerBuilder<string, TMessage>(conf)
                .SetValueDeserializer(new KafkaJsonDeserializer<TMessage>())
                .Build();

            _handler = handler;

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            return Task.Run(() => ConsumeAsync(stoppingToken));
        }

        private async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var result = _consumer.Consume();
                    await _handler.HandleAsync(result.Value!, stoppingToken);
                    _consumer.Commit(result);
                }

            }
            catch (OperationCanceledException)
            {
                Log.Information("Consumer stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected background service error.");
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer?.Close();
            return base.StopAsync(cancellationToken);
        }
    }
}
