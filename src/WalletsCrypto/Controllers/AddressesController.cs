using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WalletsCrypto.Application.Services.Address;
using WalletsCrypto.Models.Address;

namespace WalletsCrypto.Controllers
{
    public class AddressesController : BaseController
    {
        private readonly ILogger<AddressesController> _logger;
        private readonly IAddressWriter _addressWriter;
        private readonly IAddressReader _addressReader;
        public AddressesController(
            IAddressWriter addressWriter,
            IAddressReader addressReader,
            ILogger<AddressesController> logger)
        {
            _logger = logger;
            _addressWriter = addressWriter;
            _addressReader = addressReader;
        }

        /// <summary>
        /// Creates a new Adrress.
        /// </summary>
        /// <param name="model"></param>
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> PostAsync([FromBody] CreateAddressModel model)
        {
            var id = await _addressWriter.CreateAsync(model.UserId, model.CryptoCurrencyType);
            var address = await _addressReader.GetByIdAsync(id);
            _logger.LogDebug($"{JsonConvert.SerializeObject(address)}");
            return Ok(new { address });
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetAsync(string id)
        {
            var address = await _addressReader.GetByIdAsync(id);
            return Ok(new { address });
        }

        [HttpGet("user/{userId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetByUserIdAsync(string userId)
        {
            var addresses = await _addressReader.GetByUserId(userId);
            return Ok(new { addresses });
        }

        [HttpGet("untx/{addressId}")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetUnspentTransactionsByAddressIdAsync(string addressId)
        {
            var txs = await _addressReader.GetUnspentTransactionsByAddress(addressId);
            if (txs == null || txs.Count() < 1)
                return Ok(new { address_id = addressId, count = 0, sum = 0, values = new List<int>() });

            var count = txs.Count();
            var sum = txs.Sum(tx => tx.Amount);
            var values = txs.Select(tx => tx.Amount);

            return Ok(new { addresses_id = addressId, count, sum, values });
        }

    }
}