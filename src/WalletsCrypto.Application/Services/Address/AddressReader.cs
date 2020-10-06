using System.Collections.Generic;
using System.Threading.Tasks;
using WalletsCrypto.ReadModel.Persistence;

namespace WalletsCrypto.Application.Services.Address
{
    public class AddressReader : IAddressReader
    {
        private readonly IReadOnlyRepository<ReadModel.Address.Address> _addressRepository;
        private readonly IReadOnlyRepository<ReadModel.UnspentTransaction.UnspentTransaction> _unspentTransactionRepository;
        public AddressReader(
            IReadOnlyRepository<ReadModel.Address.Address> addressRepository,
            IReadOnlyRepository<ReadModel.UnspentTransaction.UnspentTransaction> unspentTransactionRepository)
        {
            _addressRepository = addressRepository;
            _unspentTransactionRepository = unspentTransactionRepository;
        }
        public async Task<ReadModel.Address.Address> GetByIdAsync(string id)
        {
            return await _addressRepository.GetByIdAsync(id);
        }

        public async Task<ReadModel.Address.Address> GetByBlockchainAddress(string address)
        {
            return await _addressRepository.GetByQueryString($"SELECT * FROM Addresses WHERE BlockChainAddress = '{address}'");
        }

        public async Task<IEnumerable<ReadModel.Address.Address>> GetByUserId(string userId)
        {
            return await _addressRepository.GetAllByQueryString($"SELECT * FROM Addresses WHERE UserId = '{userId}'");
        }

        public async Task<IEnumerable<ReadModel.UnspentTransaction.UnspentTransaction>> GetUnspentTransactionsByAddress(string address)
        {
            return await _unspentTransactionRepository.GetAllByQueryString($"SELECT * FROM UnspentTransactions WHERE AddressId = '{address}' AND IsSpent = '0'");
        }
    }
}
