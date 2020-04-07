using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BiliRaffle
{
    class Program
    {

        [STAThread]
        private static void Main(string[] args)
        {
            Application app = new Application();
            if (args.Length > 0)
            {
                int i = 0;
                switch (args[i++])
                {
                    case "-p":
                        ViewModel.Main.AsPlugin = true;
                        break;
                    case "-c":
                        Raffle.Cookies = args[i++];
                        break;
                    default:
                        break;
                }
            }
            app.Run(new MainWindow());
        }
    }
}
