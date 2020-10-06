using RabbitMQ.Client;
using System;

namespace WalletsCrypto.Infrastructure.EventBus.EventBusRabbitMQ
{
    public interface IRabbitMQPersistentConnection
        : IDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}
