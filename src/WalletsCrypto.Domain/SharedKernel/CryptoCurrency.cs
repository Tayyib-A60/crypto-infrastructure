using System;

namespace WalletsCrypto.Domain.SharedKernel
{
    public class CryptoCurrency
    {
        public decimal Value { get; private set; }

        private CryptoCurrency()
        {
        }

        private CryptoCurrency(decimal value)
        {
            Value = value;
        }

        public static CryptoCurrency NewCryptoCurrency(decimal value)
        {
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            return new CryptoCurrency(value);
        }

        public static CryptoCurrency operator+ (CryptoCurrency a, CryptoCurrency b)
        {
            var sum = a.Value + b.Value;
            return new CryptoCurrency(sum);
        }

        public static CryptoCurrency operator- (CryptoCurrency a, CryptoCurrency b)
        {
            var diff = a.Value - b.Value;
            if (diff < 0) throw new InvalidOperationException();
            return new CryptoCurrency(diff);
        }



    }
}
