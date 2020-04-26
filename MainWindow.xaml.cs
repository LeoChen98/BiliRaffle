using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace BiliRaffle
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel.Main;
        }

        #endregion Public Constructors

        #region Public Methods

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hwnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        public static extern IntPtr GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool BRePaint);
        #endregion Public Methods

        #region Private Methods

        private void CB_Condition_Comment_Click(object sender, RoutedEventArgs e)
        {
            Regex regx = new Regex("(([|http://|https://]+[|h.bilibili.com/|(www\\.|)?bilibili.com/video/(av|AV)|(www\\.|)?bilibili.com/audio/au|(www\\.|)?bilibili.com/read/cv]+\\d+)|(www\\.|)?bilibili.com/video/(BV|bv)[0-9a-zA-Z]{10})\\?*.*");
            if (regx.IsMatch(TB_Url.Text) && !ViewModel.Main.IsCommentEnabled)
            {
                System.Windows.Forms.MessageBox.Show("无法关闭，存在只支持评论抽奖的项目！");
                ViewModel.Main.IsCommentEnabled = true;
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void TB_Num_x_GotFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == "-1")
                (sender as TextBox).Text = "";
            else
                if (ViewModel.Main.ChangeNum.CanExecute((sender as TextBox).Text))
                ViewModel.Main.ChangeNum.Execute((sender as TextBox).Text);
        }

        private void TB_Num_x_LostFocus(object sender, RoutedEventArgs e)
        {
            if ((sender as TextBox).Text == "")
                (sender as TextBox).Text = "-1";
        }

        private void TB_Num_x_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex re = new Regex("[^0-9]+");

            e.Handled = re.IsMatch(e.Text);
        }

        private void TB_Url_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                return;
            }

            Regex reg = new Regex("(([|http://|https://]+[t.bilibili.com/|h.bilibili.com/|(www\\.|)?bilibili.com/video/(av|AV)|(www\\.|)?bilibili.com/audio/au|(www\\.|)?bilibili.com/read/cv]+\\d+)|(www\\.|)?bilibili.com/video/(BV|bv)[0-9a-zA-Z]{10})\\?*.*");
            if (reg.IsMatch(textBox.Text))
            {
                textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x81, 0x81, 0x81));
            }
            else
                textBox.BorderBrush = new SolidColorBrush(Colors.Red);

            Regex regx = new Regex("(([|http://|https://]+[|h.bilibili.com/|(www\\.|)?bilibili.com/video/(av|AV)|(www\\.|)?bilibili.com/audio/au|(www\\.|)?bilibili.com/read/cv]+\\d+)|(www\\.|)?bilibili.com/video/(BV|bv)[0-9a-zA-Z]{10})\\?*.*");
            if (regx.IsMatch(textBox.Text))
            {
                ViewModel.Main.IsCommentEnabled = true;
            }

            Regex regt = new Regex("([|http://|https://]t.bilibili.com/\\d+)\\?*.*");
            if (regt.IsMatch(textBox.Text))
                CB_Condition_Repose.IsEnabled = true;
            else
            {
                ViewModel.Main.IsReposeEnabled = false;
                CB_Condition_Repose.IsEnabled = false;
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Main.AsPlugin)
            {
                IntPtr hWnd = new WindowInteropHelper(this).Handle;
                Height = 411;
                ShowInTaskbar = false;
                if (Environment.Is64BitProcess)
                {
                    IntPtr WndLong = GetWindowLongPtr(hWnd, -20);
                    SetWindowLongPtr(hWnd, -20, new IntPtr(WndLong.ToInt64() | 0x00000080));
                }
                else
                {
                    IntPtr WndLong = GetWindowLong(hWnd, -20);
                    SetWindowLong(hWnd, -20, new IntPtr(WndLong.ToInt64() | 0x00000080));
                }
                SetParent(hWnd, new IntPtr(int.Parse(ViewModel.Main.Whwnd, System.Globalization.NumberStyles.HexNumber)));
                MoveWindow(hWnd, (int)ViewModel.Main.WndLeft, (int)ViewModel.Main.WndTop, (int)Width, (int)Height, true);
            }
            else
            {
                string new_ver = CheckUpdate();
                if (!string.IsNullOrEmpty(new_ver) && new_ver != Assembly.GetExecutingAssembly().GetName().Version.ToString()) ViewModel.Main.PushMsg($"检查到新版本{new_ver}，请前往【https://github.com/LeoChen98/BiliRaffle/releases/tag/{new_ver}】下载。");
            }
        }

        private string CheckUpdate()
        {
            string result = "";
            HttpWebRequest req = null;
            HttpWebResponse rep = null;
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                req = (HttpWebRequest)WebRequest.Create("https://github.com/LeoChen98/BiliRaffle/releases/latest");
                req.AllowAutoRedirect = false;
                rep = (HttpWebResponse)req.GetResponse();
                result = new Regex("\\d+?.\\d+?.\\d+?.\\d+?").Match(rep.Headers["Location"]).Value;
            }
            finally
            {
                if (rep != null) rep.Close();
                if (req != null) req.Abort();
            }

            return result;
        }

        #endregion Private Methods
    }
}