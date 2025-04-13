using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ProducerService.Services;
using ProducerService.Models;


namespace ProducerService.Test
{
    public class PostFetcherWorkerUnitTest
    {
        private readonly Mock<ILogger<PostFetcherWorker>> _mockLogger;
        private readonly Mock<IKafkaProducer> _mockKafkaProducer;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly HttpClient _httpClient;

        public PostFetcherWorkerUnitTest()
        {
            _mockLogger = new Mock<ILogger<PostFetcherWorker>>();
            _mockKafkaProducer = new Mock<IKafkaProducer>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();

            var mockHttpHandler = new Mock<HttpMessageHandler>();
            var testPost = new Models.Post { id = 1, title = "Test", body = "Body", userId = 1 };
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new List<Post> { testPost }))
            };

            mockHttpHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(fakeResponse);

            _httpClient = new HttpClient(mockHttpHandler.Object);
            _mockHttpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(_httpClient);
        }

        [Fact]
        public async Task PostFetcherWorker_CallsApi_AndPublishesToKafka()
        {
            // Arrange (all setup is done in constructor)
            var worker = new PostFetcherWorker(_mockLogger.Object, _mockKafkaProducer.Object, _mockHttpClientFactory.Object);

            // Act
            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            await worker.StartAsync(cts.Token);

            // Assert
            _mockKafkaProducer.Verify(p => p.ProduceAsync(It.Is<Post>(x => x.id == 1)), Times.AtLeastOnce);
        }
    }
}