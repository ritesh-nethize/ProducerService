using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProducerService.Models;

namespace ProducerService.Services
{
    public class PostFetcherWorker : BackgroundService
    {
        private readonly ILogger<PostFetcherWorker> _logger;
        private readonly IKafkaProducer _kafkaProducer;
        private readonly IHttpClientFactory _httpClientFactory;

        public PostFetcherWorker(ILogger<PostFetcherWorker> logger, IKafkaProducer kafkaProducer, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _httpClientFactory = httpClientFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var httpClient = _httpClientFactory.CreateClient();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var posts = await httpClient.GetFromJsonAsync<List<Post>>("https://jsonplaceholder.typicode.com/posts", cancellationToken: stoppingToken);

                    if (posts != null)
                    {
                        foreach (var post in posts)
                        {
                            await _kafkaProducer.ProduceAsync(post);
                            _logger.LogInformation("Published post ID {PostId}", post.id);
                        }
                    }

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Delay between fetches
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during API fetch or Kafka publish");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // Backoff delay
                }
            }
        }
    }
}
