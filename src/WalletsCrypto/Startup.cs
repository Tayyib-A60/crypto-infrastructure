using Autofac;
using EventStore.ClientAPI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System;
using System.IO;
using System.Reflection;
using WalletsCrypto.Application.Handlers.Address;
using WalletsCrypto.Application.Handlers.IntegrationEventHandlers;
using WalletsCrypto.Application.Handlers.Transaction;
using WalletsCrypto.Application.Handlers.User;
using WalletsCrypto.Application.PubSub;
using WalletsCrypto.Application.Services;
using WalletsCrypto.Application.Services.Address;
using WalletsCrypto.Application.Services.Transaction;
using WalletsCrypto.Application.Services.User;
using WalletsCrypto.Common.Configuration;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.EventStore;
using WalletsCrypto.Domain.Persistence;
using WalletsCrypto.Domain.Persistence.EventStore;
using WalletsCrypto.Domain.PubSub;
using WalletsCrypto.Domain.TransactionModule;
using WalletsCrypto.Domain.UserModule;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.EventBus.EventBus;
using WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;
using WalletsCrypto.Infrastructure.EventBus.EventBusRabbitMQ;
using WalletsCrypto.ReadModel.Persistence;
using ReadAddress = WalletsCrypto.ReadModel.Address.Address;
using ReadTransaction = WalletsCrypto.ReadModel.Transaction.Transaction;
using ReadUser = WalletsCrypto.ReadModel.User.User;
using ReadUnspentTransaction = WalletsCrypto.ReadModel.UnspentTransaction.UnspentTransaction;
using Transaction = WalletsCrypto.Domain.TransactionModule.Transaction;
using System.Net;

namespace WalletsCrypto
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // var canConvert = Int32.TryParse(_eventStorePort, out int port);
            // var ipAddress = IPAddress.Parse("18.220.207.215");
            
            // var settings = ConnectionSettings.Create().FailOnNoServerResponse().DisableServerCertificateValidation();
            // var EventStoreConn = EventStoreConnection.Create(settings, new IPEndPoint(ipAddress, 2113), "WalletsCrypto");

            // EventStoreConn.ConnectAsync().GetAwaiter().GetResult();

            var eventStoreConnection = EventStoreConnection.Create(
                connectionString: Configuration.GetValue<string>("EventStore:ConnectionString"),
                builder: ConnectionSettings.Create().KeepReconnecting().EnableVerboseLogging()
                .FailOnNoServerResponse().DisableServerCertificateValidation(), // TODO: Fix!!
                connectionName: Configuration.GetValue<string>("EventStore:ConnectionName"));

            eventStoreConnection.ConnectAsync().GetAwaiter().GetResult();
            services.AddSingleton(eventStoreConnection);

            // DapperMapper typemaps
            // Dapper.SqlMapper.AddTypeMap(typeof(decimal), System.Data.DbType.Decimal);

            services.AddTransient(x => new SqlConnection(Configuration.GetConnectionString("SqlServerConnectionString")));
            services.AddTransient<IReadOnlyRepository<ReadUser>, SqlServerRepository<ReadUser>>();
            services.AddTransient<IRepository<ReadUser>, SqlServerRepository<ReadUser>>();
            services.AddTransient<IReadOnlyRepository<ReadAddress>, SqlServerRepository<ReadAddress>>();
            services.AddTransient<IRepository<ReadAddress>, SqlServerRepository<ReadAddress>>();
            services.AddTransient<IRepository<ReadUnspentTransaction>, SqlServerRepository<ReadUnspentTransaction>>();
            services.AddTransient<IReadOnlyRepository<ReadTransaction>, SqlServerRepository<ReadTransaction>>();
            services.AddTransient<IReadOnlyRepository<ReadUnspentTransaction>, SqlServerRepository<ReadUnspentTransaction>>();
            services.AddTransient<IRepository<ReadTransaction>, SqlServerRepository<ReadTransaction>>();
            services.AddTransient<ITransientDomainEventPublisher, TransientDomainEventPubSub>();
            services.AddTransient<ITransientDomainEventSubscriber, TransientDomainEventPubSub>();
            services.AddTransient<IRepository<Address, AddressId>, EventSourcingRepository<Address, AddressId>>();
            services.AddTransient<IRepository<User, UserId>, EventSourcingRepository<User, UserId>>();
            services.AddTransient<IRepository<Transaction, TransactionId>, EventSourcingRepository<Transaction, TransactionId>>();
            services.AddTransient<IDomainEventHandler<UserId, UserCreatedEvent>, UserUpdater>();
            services.AddTransient<IDomainEventHandler<AddressId, AddressCreatedEvent>, AddressUpdater>();
            services.AddTransient<IDomainEventHandler<AddressId, AddressBalanceUpdatedEvent>, AddressUpdater>();
            services.AddTransient<IDomainEventHandler<AddressId, UnspentTransactionAddedEvent>, AddressUpdater>();
            services.AddTransient<IDomainEventHandler<AddressId, UnspentTransactionRemovedEvent>, AddressUpdater>();
            services.AddTransient<IDomainEventHandler<TransactionId, TransactionCreatedEvent>, TransactionUpdater>();
            services.AddSingleton<IEventStore, EventStoreEventStore>();
            services.AddTransient<IAddressWriter, AddressWriter>();
            services.AddTransient<IUserWriter, UserWriter>();
            services.AddTransient<IUserReader, UserReader>();
            services.AddTransient<IAddressReader, AddressReader>();
            services.AddTransient<ITransactionWriter, TransactionWriter>();
            services.AddTransient<ITransactionReader, TransactionReader>();
            services.AddTransient<IWalletsAddressUpdater, WalletsAddressUpdater>();
            services.AddStackExchangeRedisCache(options => { options.Configuration = "redis,password=ADMIN12345";});
            services.AddTransient<ICacheStorage, RedisCache>();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Wallets.Africa Crypto API",
                    Description = "Bitcoin and Ethereum API for Wallets.Africa",
                    TermsOfService = new Uri("https://wallets.africa"),
                    Contact = new OpenApiContact
                    {
                        Name = "Erbaver Engineering",
                        Email = "engineering@erbaver.com",
                        Url = new Uri("https://erbaver.com"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under LICX",
                        Url = new Uri("https://example.com/license"),
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddControllers();

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

           
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            // Register your own things directly with Autofac
            // builder.AddMyCustomService();

            //...
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Wallets.Africa Cryto API");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            ConfigureEventBus(app);

        }

        private void ConfigureEventBus(IApplicationBuilder app)
        {
            var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();

            eventBus.Subscribe<EthereumTransactionReceivedIntegrationEvent, EthereumTransactionReceivedIntegrationEventHandler>();
            eventBus.Subscribe<BitcoinTransactionReceivedIntegrationEvent, BitcoinTransactionReceivedIntegrationEventHandler>();
        }

        private void RegisterEventBus(IServiceCollection services, EventBusConfiguration eventBusConfiguration)
        {
            var subscriptionClientName = eventBusConfiguration.HostName;

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
            services.AddTransient<EthereumTransactionReceivedIntegrationEventHandler>();
            services.AddTransient<BitcoinTransactionReceivedIntegrationEventHandler>();
        }

    }
}
