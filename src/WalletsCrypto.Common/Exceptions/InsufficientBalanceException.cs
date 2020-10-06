using System;

namespace WalletsCrypto.Common.Exceptions
{
    public class InsufficientBalanceException : Exception
    {
        public override string Message => "This address contains insufficient available balance for ths requested transaction";
    }
}
