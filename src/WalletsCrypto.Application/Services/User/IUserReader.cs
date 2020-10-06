using System.Threading.Tasks;

namespace WalletsCrypto.Application.Services.User
{
    public interface IUserReader
    {
        Task<ReadModel.User.User> GetByIdAsync(string id);
    }
}