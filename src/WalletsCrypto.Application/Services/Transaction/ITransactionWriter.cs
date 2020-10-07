using System.Threading.Tasks;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.TransactionModule;

namespace WalletsCrypto.Application.Services.Transaction
{
    public interface ITransactionWriter
    {
        Task<(string, string)> CreateAsync(string userId, string addressId, string transactionAddress, decimal transactionAmount, TransactionTypes transactionType = TransactionTypes.Debit, int? index = null,string transactionHash = null);
        Task ReAddUnspentTx(string addressId, string hash, decimal value, int index);
    }
}
