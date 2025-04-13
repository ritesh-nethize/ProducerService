using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProducerService.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .CreateLogger();

try
{
    Log.Information("Starting ProducerService...");

    Host.CreateDefaultBuilder(args)
        .UseSerilog() // << Use Serilog instead of built-in logging
        .ConfigureServices((context, services) =>
        {
            services.AddHttpClient();
            services.AddSingleton<IKafkaProducer,KafkaProducer>();
            services.AddHostedService<PostFetcherWorker>();
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "ProducerService terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}