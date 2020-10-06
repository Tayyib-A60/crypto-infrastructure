using System.Threading.Tasks;

namespace WalletsCrypto.Domain.PubSub
{
    public interface ITransientDomainEventPublisher
    {
        Task PublishAsync<T>(T publishedEvent);
    }
}