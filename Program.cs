using DmCommons;
using System;
using System.Diagnostics;
using System.Management;
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

        private static string GetCPUInfo()
        {
            using (ManagementClass cimobject = new ManagementClass("Win32_Processor"))
            {
                using (ManagementObjectCollection moc = cimobject.GetInstances())
                {
                    string strCpuID = null;
                    foreach (ManagementObject mo in moc)
                    {
                        strCpuID = mo.Properties["ProcessorId"].Value.ToString();
                        break;
                    }
                    return strCpuID;
                }
            }
        }

        private static string GetMainDriveId()
        {
            using (ManagementClass mc = new ManagementClass("Win32_PhysicalMedia"))
            {
                using (ManagementObjectCollection moc = mc.GetInstances())
                {
                    string strID = null;
                    foreach (ManagementObject mo in moc)
                    {
                        strID = mo.Properties["SerialNumber"].Value.ToString();
                        break;
                    }
                    return strID;
                }
            }
        }

        /// <summary>
        /// 获得16位的MD5加密
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string GetMD5_16(string input)
        {
            return GetMD5_32(input).Substring(8, 16);
        }

        /// 获得32位的MD5加密 </summary> <param name="input"></param> <returns></returns>
        private static string GetMD5_32(string input)
        {
            MD5 md5 = MD5.Create();
            byte[] data = md5.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sb.AppendFormat("{0:X2}", data[i]);
            }
            return sb.ToString();
        }

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

            if (!ViewModel.Main.AsPlugin) User_Statistics();
            app.Run(new MainWindow());
        }

        private static void User_Statistics()
        {
            string cpu = GetCPUInfo();
            string drive = GetMainDriveId();
            string info = GetMD5_16(cpu + drive);
            string json = $"{{\"pid\":119,\"version\":{Assembly.GetExecutingAssembly().GetName().Version.Revision},\"token\":\"{info}\"}}";
            Http.PostBody("https://cloud.api.zhangbudademao.com/public/User_Statistics", json, null, "application/json; charset=UTF-8");
        }

        #endregion Private Methods
    }
}