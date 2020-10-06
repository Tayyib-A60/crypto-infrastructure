using System.Threading.Tasks;
using WalletsCrypto.ReadModel.Persistence;
using WalletsCrypto.ReadModel.User;

namespace WalletsCrypto.Application.Services.User
{
    public class UserReader : IUserReader
    {
        private readonly IReadOnlyRepository<ReadModel.User.User> _userRepository;

        public UserReader(IReadOnlyRepository<ReadModel.User.User> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ReadModel.User.User> GetByIdAsync(string id)
        {
            return await _userRepository.GetByIdAsync(id);
        }
    }
}