using System;

namespace WalletsCrypto.Bitcoin.Watcher.BackgroundServices
{
    public class ServiceDowntimeException : Exception
    {
        public ServiceDowntimeException(string message) : base(message)
        {

        }
    }
}
