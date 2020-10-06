using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using WalletsCrypto.Helpers;
using NLog.Extensions.Logging;

namespace WalletsCrypto
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
             .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                    logging.AddEventSourceLogger();
                    logging.AddNLog();
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
                    {
                        webBuilder.UseUrls("https://*:4001", "http://*:4000");
                    }
                    else
                        webBuilder.UseUrls("https://127.0.0.1:4001", "http://127.0.0.1:4000");
                    webBuilder.UseStartup<Startup>();
                    // webBuilder.UseSerilog((context, config) =>
                    // {
                    //     config.ReadFrom.Configuration(context.Configuration);
                    // });
                    webBuilder.UseKestrel(options =>
                    {
                        options.Listen(IPAddress.Any, 4001, listenoptions =>
                        {
                            var certificate = FileHelper.LoadCertificate();
                            listenoptions.UseHttps(certificate);
                        });
                    });
                });
    }
}
