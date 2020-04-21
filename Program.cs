using System;
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
            if (args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    switch (args[i])
                    {
                        case "-p":
                            ViewModel.Main.AsPlugin = true;
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
                                }
                            });
                            break;

                        case "-c":
                            Raffle.Cookies = args[++i];
                            break;

                        case "-w":
                            ViewModel.Main.Whwnd = args[++i];
                            break;

                        default:
                            break;
                    }
                }
            }
            app.Run(new MainWindow());
        }

        #endregion Private Methods
    }
}