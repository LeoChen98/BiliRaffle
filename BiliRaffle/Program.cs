using DmCommons;
using Sentry;
using System;
using System.Diagnostics;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BiliRaffle
{
    internal class Program
    {
        #region Private Methods

        [STAThread]
        private static void Main(string[] args)
        {
            System.Windows.Application app = new System.Windows.Application();

            //初始化统计组件
            SentrySdk.Init(options =>
            {
                // A Sentry Data Source Name (DSN) is required.
                // See https://docs.sentry.io/product/sentry-basics/dsn-explainer/
                // You can set it in the SENTRY_DSN environment variable, or you can set it in code here.
                options.Dsn = "https://acdc5324b9bd28eb137011a3b84c8f76@o4507091526352896.ingest.us.sentry.io/4507095501570048";

                // When debug is enabled, the Sentry client will emit detailed debugging information to the console.
                // This might be helpful, or might interfere with the normal operation of your application.
                // We enable it here for demonstration purposes when first trying Sentry.
                // You shouldn't do this in your applications unless you're troubleshooting issues with Sentry.
                options.Debug = false;

                // This option is recommended. It enables Sentry's "Release Health" feature.
                options.AutoSessionTracking = true;

                // Enabling this option is recommended for client applications only. It ensures all threads use the same global scope.
                options.IsGlobalModeEnabled = true;

                // This option will enable Sentry's tracing features. You still need to start transactions and spans.
                options.EnableTracing = true;

                // Example sample rate for your transactions: captures 10% of transactions
                options.TracesSampleRate = 0.1;
            });

            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-p":
                            ViewModel.Main.AsPlugin = true;
                            Process p = Process.GetProcessById(int.Parse(args[++i]));
                            Task.Factory.StartNew(() =>
                            {
                                while (true)
                                {
                                    switch (Console.ReadLine())
                                    {
                                        case "Q":
                                        case "q":
                                            Environment.Exit(1);
                                            break;

                                        default:
                                            break;
                                    }

                                    if (p.HasExited) Environment.Exit(2);

                                    Thread.Sleep(1000);
                                }
                            });
                            break;

                        case "-c":
                            Raffle.Cookies = args[++i];
                            break;

                        case "-w":
                            ViewModel.Main.Whwnd = args[++i];
                            break;

                        case "-l":
                            ViewModel.Main.WndLeft = double.Parse(args[++i]);
                            break;

                        case "-t":
                            ViewModel.Main.WndTop = double.Parse(args[++i]);
                            break;

                        default:
                            break;
                    }
                }
            }

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

            app.Run(new MainWindow());
        }

        #endregion Private Methods
    }
}