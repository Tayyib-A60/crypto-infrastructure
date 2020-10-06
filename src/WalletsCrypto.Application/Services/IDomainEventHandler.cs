using System.Threading.Tasks;
using WalletsCrypto.Domain.Core;

namespace WalletsCrypto.Application.Services
{
    public interface IDomainEventHandler<TAggregateId, TEvent>
        where TEvent: IDomainEvent<TAggregateId>
    {
        Task HandleAsync(TEvent @event);
    }
}