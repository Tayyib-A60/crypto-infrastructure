using System.Threading.Tasks;
using WalletsCrypto.Application.Services;
using WalletsCrypto.Domain.UserModule;
using WalletsCrypto.ReadModel.Persistence;
using UserReadModel = WalletsCrypto.ReadModel.User.User;


namespace WalletsCrypto.Application.Handlers.User
{
    public class UserUpdater : IDomainEventHandler<UserId, UserCreatedEvent>
    {
        private readonly IRepository<UserReadModel> _userRepository;

        public UserUpdater(IRepository<UserReadModel> userRepository)
        {

            _userRepository = userRepository;
        }

        public async Task HandleAsync(UserCreatedEvent @event)
        {

            await _userRepository.InsertAsync(new UserReadModel
            {
                Id = @event.AggregateId.IdAsStringWithoutPrefix()
            });
        }
    }
}