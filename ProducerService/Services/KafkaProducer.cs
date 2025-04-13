using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Confluent.Kafka;
using ProducerService.Models;

namespace ProducerService.Services
{
    public class KafkaProducer : IKafkaProducer
    {
        private readonly string _bootstrapServers = "localhost:9092";
        private readonly string _topic = "sample-topic";

        public async Task ProduceAsync(Post postData)
        {
            var config = new ProducerConfig { BootstrapServers = _bootstrapServers };

            using var producer = new ProducerBuilder<Null, string>(config).Build();
            var json = JsonSerializer.Serialize(postData);

            await producer.ProduceAsync(_topic, new Message<Null, string> { Value = json });
        }
    }
}
