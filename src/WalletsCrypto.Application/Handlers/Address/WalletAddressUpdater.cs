using Microsoft.Data.SqlClient;
using NLogWrapper;
using System;
using System.Configuration;
using System.Threading.Tasks;
using WalletsCrypto.ReadModel.WalletsCryptoDbModels;

namespace WalletsCrypto.Application.Handlers.Address
{
    public interface IWalletsAddressUpdater
    {
        Task<bool> UpdateAddressBalance(decimal availableBalance, decimal bookBalance, string addressId);

        Task<bool> CreateCreditTransaction(WalletCryptoTransaction walletCryptoTransaction);

        Task<CryptoCurrencyWallet> GetCryptoWallet(string addressId);

        Task<bool> UpdateTransactionStatus(string transactionHash);
    }
    public class WalletsAddressUpdater: IWalletsAddressUpdater
    {
        private SqlConnection _sqlConnection { get; set; }
        private string _dbConnectionString => ConfigurationManager.ConnectionStrings["WalletsCryptoDbServerConnectionString"].ToString();
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public WalletsAddressUpdater()
        {
            _sqlConnection = new SqlConnection(_dbConnectionString);
        }

        public async Task<bool> UpdateAddressBalance(decimal availableBalance, decimal bookBalance, string addressId)
        {
            await  _sqlConnection.OpenAsync();
            _logger.Debug($"Updating adress {addressId}'s balance to {availableBalance}");

            try
            {
                var command = new SqlCommand($"update [dbo].[CryptoCurrencyWallets] set AvailableBalance={availableBalance}, LedgerBalance={bookBalance}, HasPendingUpdate=0, DateUpdated='{DateTime.Now}' where AddressId='{addressId}'", _sqlConnection);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.ToString()}");
                return false;
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public async Task<bool> UpdateTransactionStatus(string transactionHash)
        {
            await _sqlConnection.OpenAsync();
            _logger.Debug($"Updating transaction with hash {transactionHash}");

            try
            {
                var command = new SqlCommand($"update [dbo].[CryptoWalletTransactions] set FinalStatus='SUCCESSFUL' where TransactionHash='{transactionHash}'", _sqlConnection);
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.ToString()}");
                return false;
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        public async Task<CryptoCurrencyWallet> GetCryptoWallet(string addressId)
        {
            await _sqlConnection.OpenAsync();

            var cryptocurrencyWallet = new CryptoCurrencyWallet();

            try
            {
                var command = new SqlCommand($"select * from [dbo].[CryptoCurrencyWallets] where AddressId='{addressId}'", _sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        cryptocurrencyWallet = new CryptoCurrencyWallet
                        {
                            Address = reader["Address"].ToString(),
                            AddressId = reader["AddressId"].ToString(),
                            CurrencyType = ParseToCurrencyType(reader["CurrencyType"].ToString()),
                            AvailableBalance = Decimal.Parse(reader["AvailableBalance"].ToString()),
                            LedgerBalance = Decimal.Parse(reader["LedgerBalance"].ToString()),
                            WalletUserId = reader["WalletUserId"].ToString()
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.ToString()}");
            }
            finally
            {
                _sqlConnection.Close();
            }

            return cryptocurrencyWallet;
        }

        public async Task<bool> ShouldCreateCreditTransaction(string senderAddress)
        {
            await _sqlConnection.OpenAsync();

            var cryptocurrencyWallet = new CryptoCurrencyWallet();

            try
            {
                var command = new SqlCommand($"select * from [dbo].[CryptoCurrencyWallets] where Address='{senderAddress}'", _sqlConnection);
                SqlDataReader reader = command.ExecuteReader();

                if (reader.HasRows)
                {
                    reader.Close();
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.ToString()}");
            }
            finally
            {
                _sqlConnection.Close();
            }

            return false;
        }

        public async Task<bool> CreateCreditTransaction(WalletCryptoTransaction walletCryptoTransaction)
        {
            await _sqlConnection.OpenAsync();

            try
            {
                var command = new SqlCommand($"INSERT INTO [dbo].[CryptoWalletTransactions] (DestinationAddress, FinalStatusTimeStamp, FinalStatus, Status, DateCreated, CurrentBalance, PreviousBalance, Narration, CurrencyType, AmountInNaira, AmountInDollars, Amount, TransactionChannel, TransactionReference, TransactionType, DestinationAddressId, SourceAddressId, WalletUserId, Category, TransactionFee, TransactionHash) VALUES ({walletCryptoTransaction.DestinationAddress}, {walletCryptoTransaction.FinalStatusTimeStamp}, {walletCryptoTransaction.FinalStatus}, {walletCryptoTransaction.Status}, {walletCryptoTransaction.DateCreated}, {walletCryptoTransaction.CurrentBalance}, {walletCryptoTransaction.PreviousBalance}, {walletCryptoTransaction.Narration}, 0, {walletCryptoTransaction.AmountInNaira}, {walletCryptoTransaction.AmountInDollars}, {walletCryptoTransaction.Amount}, {walletCryptoTransaction.TransactionChannel}, {walletCryptoTransaction.TransactionReference}, {walletCryptoTransaction.TransactionType}, {walletCryptoTransaction.DestinationAddressId}, {walletCryptoTransaction.SourceAddressId}, {walletCryptoTransaction.WalletUserId}, {walletCryptoTransaction.Category}, {walletCryptoTransaction.TransactionHash}, {walletCryptoTransaction.TransactionFee}) ", _sqlConnection);

                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected > 0)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.Error($"{ex.ToString()}");
                return false;
            }
            finally
            {
                _sqlConnection.Close();
            }
        }

        private CryptoCurrencyType ParseToCurrencyType(string typeString)
        {
            switch (typeString)
            {
                case "0": return CryptoCurrencyType.Bitcoin;
                case "1": return CryptoCurrencyType.Ethereum;
                default: return CryptoCurrencyType.Invalid;
            }
        }
    }
}
