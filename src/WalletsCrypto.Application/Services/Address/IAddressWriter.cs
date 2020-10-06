using System.Collections.Generic;
using System.Threading.Tasks;
using WalletsCrypto.Domain.AddressModule;
using WalletsCrypto.Domain.SharedKernel;
using WalletsCrypto.Domain.TransactionModule;
using UnspentTx = WalletsCrypto.Domain.AddressModule.UnspentTransaction;

namespace WalletsCrypto.Application.Services.Address
{
    public interface IAddressWriter
    {
        Task<string> CreateAsync(string userId, CryptoCurrencyTypes cryptoCurrencyType);
        Task UpdateBalanceAsync(string addressId, decimal transactionValue, TransactionTypes transactionType, bool isConfirmed = false);
        Task AddUnspentTransactionAsync(string addressId, UnspentTx unspentTx);
        Task<IEnumerable<UnspentTransaction>> PopUnspentTransactions(string addressId, decimal amount);
        Task AddUnUsedUnspentTransactions(string addressId, List<UnspentTx> unspentTransactionsToAdd);
        Task AddChangeUnspentTransaction(string addressId, UnspentTx unspentTx);

    }
}