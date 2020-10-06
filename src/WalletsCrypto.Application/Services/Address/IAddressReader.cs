using System.Collections.Generic;
using System.Threading.Tasks;

namespace WalletsCrypto.Application.Services.Address
{
    public interface IAddressReader
    {
        Task<ReadModel.Address.Address> GetByIdAsync(string id);
        Task<ReadModel.Address.Address> GetByBlockchainAddress(string address);
        Task<IEnumerable<ReadModel.Address.Address>> GetByUserId(string userId);
        Task<IEnumerable<ReadModel.UnspentTransaction.UnspentTransaction>> GetUnspentTransactionsByAddress(string address);
    }
}
