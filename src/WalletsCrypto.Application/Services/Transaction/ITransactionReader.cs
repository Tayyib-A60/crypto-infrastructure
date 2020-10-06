using System.Collections.Generic;
using System.Threading.Tasks;

namespace WalletsCrypto.Application.Services.Transaction
{
    public interface ITransactionReader
    {
        Task<ReadModel.Transaction.Transaction> GetByIdAsync(string id);
        Task<IEnumerable<ReadModel.Transaction.Transaction>> GetByUserId(string userId);
        Task<IEnumerable<ReadModel.Transaction.Transaction>> GetByAddressId(string addressId);
    }
}
