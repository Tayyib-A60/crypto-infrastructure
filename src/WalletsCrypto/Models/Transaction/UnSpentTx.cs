using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WalletsCrypto.Models.Transaction
{
    public class AddUnSpentTx
    {
        [Required]
        public string Hash { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public decimal Value { get; set; }

        [Required]
        public string AddressId { get; set; }
    }
}
