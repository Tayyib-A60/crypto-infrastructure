using System.Threading.Tasks;
using WalletsCrypto.Domain.AddressModule;
using Transaction = WalletsCrypto.Domain.TransactionModule.Transaction;

namespace WalletsCrypto.Infrastructure.Providers
{
    public interface IBlockchainProvider
    {
        Task<string> Broadcast(Transaction transaction, Address address);
        decimal GetFee();
    }
}
