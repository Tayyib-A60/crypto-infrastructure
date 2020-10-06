using System.Threading.Tasks;
using WalletsCrypto.Infrastructure.EventBus.EventBus.IntegrationEvents;

namespace WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    public interface IIntegrationEventHandler
    {
    }
}
