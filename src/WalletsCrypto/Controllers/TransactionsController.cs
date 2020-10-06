using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using WalletsCrypto.Application.Services.Transaction;
using WalletsCrypto.Common.Exceptions;
using WalletsCrypto.Infrastructure.Cache;
using WalletsCrypto.Infrastructure.Providers;
using WalletsCrypto.Models.Transaction;

namespace WalletsCrypto.Controllers
{
    public class TransactionsController : BaseController
    {
        private readonly ITransactionReader _transactionReader;
        private readonly ITransactionWriter _transactionWriter;
        private readonly ILogger<TransactionsController> _logger;
        private readonly ICacheStorage _cacheStorage;
        public TransactionsController(
            ITransactionWriter transactionWriter,
            ITransactionReader transactionReader,
            ILogger<TransactionsController> logger,
            ICacheStorage cacheStorage)
        {
            _logger = logger;
            _transactionReader = transactionReader;
            _transactionWriter = transactionWriter;
            _cacheStorage = cacheStorage;
        }

        [HttpGet("{Id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(string id)
        {
            try
            {
                var transaction = await _transactionReader.GetByIdAsync(id);
                return Ok(new { transaction });
            }
            catch (Exception e)
            {

                return BadRequest(new { e.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetByUserIdAsync(string userId)
        {
            try
            {
                var transactions = await _transactionReader.GetByUserId(userId);
                return Ok(new { transactions });
            }
            catch (Exception e)
            {

                return BadRequest(new { e.Message });
            }
        }

        [HttpGet("address/{addressId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetByAddressIdAsync(string addressId)
        {
            try
            {
                var transactions = await _transactionReader.GetByAddressId(addressId);
                return Ok(new { transactions });
            }
            catch (Exception e)
            {

                return BadRequest(new { e.Message });
            }
        }

        [HttpGet("fee/{cryptoType}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTransactionFee(string cryptoType)
        {
            try
            {
                switch (cryptoType)
                {
                    case "btc":
                        var cacheValue = await _cacheStorage.RetrieveAsync("CURRENT_BITCOIN_TRANSACTION_FEE");
                        var transactionFee = 0.00007m;
                        //var transactionFee = decimal.Parse(cacheValue);
                        //var fee = BlockchainProvider.InitializeFactories()
                        //        .ExecuteCreation(Domain.SharedKernel.CryptoCurrencyTypes.BTC)
                        //        .GetFee();

                        return Ok(new { transactionFee });
                    case "eth":

                        var fee = BlockchainProvider.InitializeFactories()
                                .ExecuteCreation(Domain.SharedKernel.CryptoCurrencyTypes.ETH)
                                .GetFee();

                        return Ok(new { fee });

                    default:
                        return BadRequest("crypto type not supportted");
                }
            }
            catch (Exception e)
            {
                return BadRequest(new { e.Message });
            }
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PostAsync([FromBody]CreateTransactionModel model)
        {
            //if(!ModelState.IsValid)
            //{
            //    return BadRequest("Invalid Parameters");
            //}

            //if(model.TransactionAmount !> 0)
            //{
            //    return BadRequest("Invalid Transaction Amount");
            //}
            _logger.LogDebug($"{JsonConvert.SerializeObject(model)}");
           
            try
            {
                var id = await _transactionWriter.CreateAsync(model.UserId, model.AddressId, model.DestinationAddress, model.TransactionAmount);

                if(id.Length < 1)
                {
                    var res = new ObjectResult("Transaction not processed")
                    {
                        StatusCode = 417
                    };
                    _logger.LogDebug($"{JsonConvert.SerializeObject(res)}");
                    return res;
                }

                var transaction = await _transactionReader.GetByIdAsync(id);

                _logger.LogDebug($"{JsonConvert.SerializeObject(transaction)}");
                
                return Ok(new { transaction });
            }
            catch (InsufficientBalanceException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception e)
            {
                return BadRequest(new { e.Message });
            }
            
        }

        [HttpPost("reAddUnSpentTx")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult>PostUnSpentTransactionAsync([FromBody] AddUnSpentTx model)
        {

            try
            {
                await _transactionWriter.ReAddUnspentTx(model.AddressId, model.Hash, model.Value, model.Index);

                return Ok(new { model });
            }
            catch (Exception e)
            {
                return BadRequest(new { e.Message });
            }
        }
    }
}
