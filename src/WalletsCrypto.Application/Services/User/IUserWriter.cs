using System.Threading.Tasks;

namespace WalletsCrypto.Application.Services.User
{
    public interface IUserWriter
    {
        Task CreateAsync(string userId);
    }
}