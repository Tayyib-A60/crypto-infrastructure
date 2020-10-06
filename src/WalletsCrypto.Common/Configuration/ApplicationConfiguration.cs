using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

namespace WalletsCrypto.Common.Configuration
{
    public static class ApplicationConfiguration
    {
        private static EventBusConfiguration _eventBusConfiguration;
        private static EthereumNodeConfiguration _ethereumNodeConfiguration;
        private static BitcoinNodeConfiguration _bitcoinNodeConfiguration;

        public static EventBusConfiguration EventBusConfiguration 
        {
            get
            {
                if (_eventBusConfiguration is null)
                    Initialize();
                return _eventBusConfiguration;
            } 
        }
        public static EthereumNodeConfiguration EthereumNodeConfiguration 
        {
            get
            {
                if (_ethereumNodeConfiguration is null)
                    Initialize();
                return _ethereumNodeConfiguration;
            }
        }
        public static BitcoinNodeConfiguration BitcoinNodeConfiguration 
        {
            get
            {
                if (_bitcoinNodeConfiguration is null)
                    Initialize();
                return _bitcoinNodeConfiguration;
            }
        }

        private static void Initialize()
        {
            var asm = Assembly.GetCallingAssembly();
            var dir = asm.Location.Replace($"{asm.GetName().Name}.dll", "");
            var configurationBuilder = new ConfigurationBuilder();

            var path = Path.Combine(dir, "Configuration", "appsettings.json");
            configurationBuilder.AddJsonFile(path, false);

            path = Path.Combine(dir, "Configuration", "appsettings.Development.json");
            configurationBuilder.AddJsonFile(path, false);
            var configuration = configurationBuilder.Build();
            _eventBusConfiguration = BindEventBusConfiguration(configuration);
            _ethereumNodeConfiguration = BindEthereumNodeConfiguration(configuration);
            _bitcoinNodeConfiguration = BindBitcoinNodeConfiguration(configuration);
        }

        private static BitcoinNodeConfiguration BindBitcoinNodeConfiguration(IConfiguration configuration)
        {
            var config = configuration.GetSection("BitcoinNode");
            return new BitcoinNodeConfiguration(
                config["RPCUrl"],
                config["ZMQUrl"],
                config["RPCUsername"],
                config["RPCPassword"],
                config["WalletPassword"],
                short.Parse(config["RPCRequestTimeout"]),
                config["Network"]);
        }

        private static EthereumNodeConfiguration BindEthereumNodeConfiguration(IConfiguration configuration)
        {
            var config = configuration.GetSection("EthereumNode");
            return new EthereumNodeConfiguration(
                config["HttpsUrl"],
                config["WebSocketUrl"]);
        }

        private static EventBusConfiguration BindEventBusConfiguration(IConfiguration configuration)
        {
            var config = configuration.GetSection("EventBus");
            return new EventBusConfiguration(
                config["HostName"],
                config["Username"],
                config["Password"],
                int.Parse(config["RetryCount"]),
                config["subScriptionClientName"]
             );
        }
    }
}
