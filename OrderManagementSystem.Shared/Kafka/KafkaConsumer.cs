using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceProvider _serviceProvider;

        public KafkaConsumer(IOptions<KafkaConsumerSettings> settings, IServiceProvider serviceProvider)
        {
            var conf = new ConsumerConfig()
            {
                BootstrapServers = settings.Value.BootstrapServers,
                GroupId = settings.Value.GroupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };

            _topic = settings.Value.Topic;

            _consumer = new ConsumerBuilder<string, TMessage>(conf)
                .SetValueDeserializer(new KafkaJsonDeserializer<TMessage>())
                .Build();

            _serviceProvider = serviceProvider;

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {

            return Task.Run(() => ConsumeAsync(stoppingToken));
        }

        private async Task ConsumeAsync(CancellationToken stoppingToken)
        {
            _consumer.Subscribe(_topic);
            Log.Information($"Subscribed to topic: {_topic}");
            try
            {
                
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var result = _consumer.Consume(TimeSpan.FromSeconds(10));
                        if (result == null) continue;
                        Log.Information("Received message. Key: {MessageKey}, Offset: {Offset}",
                            result.Message.Key, result.Offset);
                        using var scope = _serviceProvider.CreateScope();
                        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler<TMessage>>();
                        await handler.HandleAsync(result.Value, stoppingToken);
                        _consumer.Commit(result);
                    }
                    catch (ConsumeException ex)
                    {
                        Log.Error(ex, $"Consume error: {ex.Error.Reason}.");
                    }
     
                }

            }
            catch (OperationCanceledException)
            {
                Log.Information("Consumer stopped");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unexpected background service error.");
                throw;
            }
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _consumer?.Close();
            return base.StopAsync(cancellationToken);
        }
    }
}
