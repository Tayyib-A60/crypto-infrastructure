using System.Collections.Generic;
using System.Threading.Tasks;
using WalletsCrypto.ReadModel.Persistence;

namespace WalletsCrypto.Application.Services.Transaction
{
    public class TransactionReader : ITransactionReader
    {
        private readonly IReadOnlyRepository<ReadModel.Transaction.Transaction> _transactionRepository;
        public TransactionReader(IReadOnlyRepository<ReadModel.Transaction.Transaction> transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<ReadModel.Transaction.Transaction>> GetByAddressId(string addressId)
        {
            return await _transactionRepository.GetAllByQueryString($"SELECT * FROM Transactions WHERE AddressId = '{addressId}'");
        }

        public async Task<ReadModel.Transaction.Transaction> GetByIdAsync(string id)
        {
            return await _transactionRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ReadModel.Transaction.Transaction>> GetByUserId(string userId)
        {
            return await _transactionRepository.GetAllByQueryString($"SELECT * FROM Transactions WHERE UserId = '{userId}'");
        }
    }
}
