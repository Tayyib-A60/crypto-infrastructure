using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using WalletsCrypto.Bitcoin.Watcher.BackgroundServices;
using WalletsCrypto.Bitcoin.Watcher.Channels;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.EventBus.EventBus;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBusRabbitMQ;
using NLog.Extensions.Logging;

namespace WalletsCrypto.Bitcoin.Watcher
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddNLog();
                })
                 .ConfigureContainer<ContainerBuilder>(builder =>
                 {
                 })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<TransactionFeeUpdater>();
                    services.AddHostedService<ProcessTransactionsService>();
                    services.AddHostedService<NewBlocksDownloader>();
                    services.AddSingleton<BlockHashTransferChannel>();
                    services.AddStackExchangeRedisCache(options =>
                    {
                        options.Configuration = "redis,password=ADMIN12345";
                    });
                    services.AddSingleton<ICacheStorage, RedisCache>();
                    services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                    {
                        var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                        var factory = new ConnectionFactory()
                        {
                            HostName = ApplicationConfiguration.EventBusConfiguration.HostName,
                            DispatchConsumersAsync = true
                        };

                        factory.UserName = ApplicationConfiguration.EventBusConfiguration.Username;
                        factory.Password = ApplicationConfiguration.EventBusConfiguration.Password;

                        var retryCount = ApplicationConfiguration.EventBusConfiguration.RetryCount;
                        return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                    });
                    RegisterEventBus(services, ApplicationConfiguration.EventBusConfiguration);
                });

        private static void RegisterEventBus(IServiceCollection services, EventBusConfiguration eventBusConfiguration)
        {
            var subscriptionClientName = eventBusConfiguration.SubscriptionClientName;
            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                var retryCount = eventBusConfiguration.RetryCount;
                return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, iLifetimeScope, eventBusSubcriptionsManager, subscriptionClientName, retryCount);
            });
            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();
        }
    }
}
