using Newtonsoft.Json;
using NLogWrapper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.ReadModel.Persistence;
using UnspentTransaction = WalletsCrypto.Domain.AddressModule.UnspentTransaction;
using UnspentTransactionReadModel = WalletsCrypto.ReadModel.UnspentTransaction.UnspentTransaction;



namespace WalletsCrypto.Application.Handlers.UnSpentTransaction
{
    public interface IUnSpentTransactionUpdater
    {
        Task UpdateUnSpentTransactionToUnSpent(List<UnspentTransaction> unSpentTransactions);
        Task AddUnSpentTransaction(string addressId, UnspentTransaction unspentTx);

    }
public class UnSpentTransactionUpdater : IUnSpentTransactionUpdater
    {
        private readonly IRepository<UnspentTransactionReadModel> _unspentTransactionRepository;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UnSpentTransactionUpdater(IRepository<UnspentTransactionReadModel> unspentTransactionRepository)
        {
            _unspentTransactionRepository = unspentTransactionRepository;
        }

        public async Task UpdateUnSpentTransactionToUnSpent(List<UnspentTransaction> unSpentTransactions)
        {
            foreach (var tx in unSpentTransactions)
            {
                var unTx = await _unspentTransactionRepository.GetByQueryString($"SELECT * FROM UnspentTransactions WHERE TxHash = '{tx.Hash}'");
                unTx.IsSpent = false;
                _logger.Debug($"{JsonConvert.SerializeObject(unTx)}");
                await _unspentTransactionRepository.UpdateAsync(unTx);
            }
        }

        public async Task AddUnSpentTransaction(string addressId, UnspentTransaction unspentTx)
        {
            var tx = await _unspentTransactionRepository.GetByQueryString($"SELECT * FROM UnspentTransactions WHERE TxHash = '{unspentTx.Hash}'");

            if (tx?.TxHash != null)
            {
                _logger.Debug($"TransactionHash {tx.TxHash}, Amount {tx.Amount}");
            }

            tx = new UnspentTransactionReadModel
            {
                Id = Guid.NewGuid().ToString(),
                AddressId = addressId,
                TxHash = unspentTx.Hash,
                Amount = unspentTx.Value,
                IsSpent = false
            };

            await _unspentTransactionRepository.InsertAsync(tx);
            _logger.Debug("Change unspent tx added");
        }
    }
}
