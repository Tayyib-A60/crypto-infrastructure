using System.Collections.Generic;
using System.Threading.Tasks;
using WalletsCrypto.Domain.Core;
using WalletsCrypto.Domain.Persistence;
using WalletsCrypto.Domain.PubSub;
using WalletsCrypto.Domain.UserModule;

namespace WalletsCrypto.Application.Services.User
{
    public class UserWriter : IUserWriter
    {
        private readonly IRepository<Domain.UserModule.User, UserId> _userRepository;
        private readonly ITransientDomainEventSubscriber _subscriber;
        private readonly IEnumerable<IDomainEventHandler<UserId, UserCreatedEvent>> _userCreatedEventHandlers;
        public UserWriter(
            IRepository<Domain.UserModule.User, UserId> userRepository,
            ITransientDomainEventSubscriber subscriber,
            IEnumerable<IDomainEventHandler<UserId, UserCreatedEvent>> userCreatedEventHandlers)
        {

            _subscriber = subscriber;
            _userRepository = userRepository;
            _userCreatedEventHandlers = userCreatedEventHandlers;
        }
        public async Task CreateAsync(string userId)
        {
            var user = new Domain.UserModule.User(new UserId(userId));
            _subscriber.Subscribe<UserCreatedEvent>(async @event => await HandleAsync(_userCreatedEventHandlers, @event));
            await _userRepository.SaveAsync(user);
        }

        public async Task HandleAsync<T>(IEnumerable<IDomainEventHandler<UserId, T>> handlers, T @event)
            where T : IDomainEvent<UserId>
        {
            foreach (var handler in handlers)
            {
                await handler.HandleAsync(@event);
            }
        }
    }
}