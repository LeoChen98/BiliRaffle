using DmCommons;
using Newtonsoft.Json.Linq;
using QRCoder;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace BiliRaffle
{
    /// <summary>
    /// LoginWindow.xaml 的交互逻辑
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Private Fields

        private static LoginWindow _instance;
        private Timer Monitor, Refresher;

        #endregion Private Fields

        #region Private Constructors

        private LoginWindow()
        {
            InitializeComponent();
        }

        #endregion Private Constructors

        #region Public Properties

        public static LoginWindow Instance
        {
            get
            {
                if (_instance == null) _instance = new LoginWindow();
                return _instance;
            }
        }

        #endregion Public Properties

        #region Public Methods

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// 显示模式窗口
        /// </summary>
        /// <param name="info">提示信息</param>
        /// <returns>操作是否成功</returns>
        public bool? ShowDialog(string info)
        {
            lbl_stauts.Dispatcher.Invoke(delegate () { lbl_stauts.Visibility = Visibility.Visible; lbl_stauts.Content = info; });
            return ShowDialog();
        }

        /// <summary>
        /// 显示模式窗口
        /// </summary>
        /// <returns>操作是否成功</returns>
        public void ShowDialog(MethodFlag f)
        {
            ShowDialog();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 获取登陆二维码并显示
        /// </summary>
        private void GetQrcode()
        {
        re:
            //获取二维码要包含的url
            string str = Http.GetBody("https://passport.bilibili.com/x/passport-login/web/qrcode/generate", null, "https://passport.bilibili.com/login");
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);

                if ((int)obj["code"] == 0)
                {
                    // 生成二维码的内容
                    string strCode = obj["data"]["url"].ToString();
                    QRCodeGenerator qrGenerator = new QRCodeGenerator();
                    QRCodeData qrCodeData = qrGenerator.CreateQrCode(strCode, QRCodeGenerator.ECCLevel.Q);
                    QRCode qrcode = new QRCode(qrCodeData);

                    //生成二维码位图
                    Bitmap qrCodeImage = qrcode.GetGraphic(5, Color.Black, Color.White, null, 0, 6, false);

                    qrcodeBox.Dispatcher.Invoke(delegate ()
                    {
                        IntPtr myImagePtr = qrCodeImage.GetHbitmap();     //创建GDI对象，返回指针

                        BitmapSource imgsource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(myImagePtr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());  //创建imgSource

                        DeleteObject(myImagePtr);

                        qrcodeBox.Source = imgsource;
                    });

                    Monitor = new Timer(MonitorCallback, obj["data"]["qrcode_key"].ToString(), 1000, 1000);
                    Refresher = new Timer(RefresherCallback, null, 180000, Timeout.Infinite);
                }
            }
            else goto re;
        }

        /// <summary>
        /// 监视器回调
        /// </summary>
        /// <param name="o">qrcodeKey</param>
        private void MonitorCallback(object o)
        {
            string qrcodeKey = o.ToString();

            string str = Http.GetBody($"https://passport.bilibili.com/x/passport-login/web/qrcode/poll?qrcode_key={qrcodeKey}", referer:"https://passport.bilibili.com/login");
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);

                if (obj.Property("code") != null && (int)obj["code"] == 0)
                {
                    switch ((int)obj["data"]["code"])
                    {
                        case 86101://未扫描
                            break;

                        case 86090://已扫描
                            lbl_stauts.Dispatcher.Invoke(delegate () { lbl_stauts.Visibility = Visibility.Visible; });
                            break;

                        case 0://登陆成功
                            //关闭监视器
                            Monitor.Change(Timeout.Infinite, Timeout.Infinite);
                            Refresher.Change(Timeout.Infinite, Timeout.Infinite);

                            string Querystring = Regex.Split(obj["data"]["url"].ToString(), "\\?")[1];
                            string[] KeyValuePair = Regex.Split(Querystring, "&");
                            string cookies = "";
                            for (int i = 0; i < KeyValuePair.Length - 1; i++)
                            {
                                cookies += KeyValuePair[i] + "; ";
                            }
                            cookies = cookies.Substring(0, cookies.Length - 2);
                            Raffle.Cookies = cookies;

                            Dispatcher.Invoke(delegate ()
                            {
                                DialogResult = true;
                                Close();
                            });
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// 二维码过期刷新
        /// </summary>
        /// <param name="o">忽略</param>
        private void RefresherCallback(object o)
        {
            GetQrcode();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            GetQrcode();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _instance = null;
        }

        #endregion Private Methods

        #region Public Classes

        public class MethodFlag { };

        #endregion Public Classes
    }
}