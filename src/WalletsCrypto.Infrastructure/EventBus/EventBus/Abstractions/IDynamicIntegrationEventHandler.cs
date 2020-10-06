using System.Threading.Tasks;

namespace WalletsCrypto.Infrastructure.EventBus.EventBus.Abstractions
{
    public interface IDynamicIntegrationEventHandler
    {
        Task Handle(dynamic eventData);
    }
}
