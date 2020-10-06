using NBitcoin;
using System;

namespace WalletsCrypto.Common.Configuration
{
    public class BitcoinNodeConfiguration
    {
        public BitcoinNodeConfiguration(string rpcUrl, string zmqUrl, string rpcUsername, 
            string rpcPassword, string walletPassword, short rpcRequestTimeout, string network)
        {
            if (string.IsNullOrWhiteSpace(rpcUrl))
                throw new ArgumentNullException(nameof(rpcUrl));

            if (string.IsNullOrWhiteSpace(zmqUrl))
                throw new ArgumentNullException(nameof(zmqUrl));

            if (string.IsNullOrWhiteSpace(rpcUsername))
                throw new ArgumentNullException(nameof(rpcUsername));

            if (string.IsNullOrWhiteSpace(rpcPassword))
                throw new ArgumentNullException(nameof(rpcPassword));

            if (string.IsNullOrWhiteSpace(walletPassword))
                throw new ArgumentNullException(nameof(walletPassword));

            if (rpcRequestTimeout < 1)
                throw new ArgumentOutOfRangeException(nameof(rpcRequestTimeout));

            if (string.IsNullOrWhiteSpace(network))
                throw new ArgumentNullException(nameof(network));

            switch (network.ToLower())
            {
                case "testnet":
                    Network = Network.TestNet;
                    break;
                case "main":
                    Network = Network.Main;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(network));
            }


            RPCUrl = rpcUrl;
            ZMQUrl = zmqUrl;
            RPCUsername = rpcUsername;
            RPCPassword = rpcPassword;
            WalletPassword = walletPassword;
            RPCRequestTimeout = rpcRequestTimeout;
           
        }
        public string RPCUrl { get; }
        public string ZMQUrl { get; }
        public string RPCUsername { get; }
        public string RPCPassword { get; }
        public string WalletPassword { get; }
        public short RPCRequestTimeout { get; }
        public Network Network { get; }
        
    }
}
