using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BiliRaffle
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel.Main;
        }


        private void TB_Url_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (string.IsNullOrEmpty(textBox.Text))
            {
                textBox.BorderBrush = new SolidColorBrush(Colors.Red);
                Lbl_Url_ErrorInfo.Text = "url不能为空！";
                Lbl_Url_ErrorInfo.Visibility = Visibility.Visible;
                return;
            }

            Regex reg = new Regex("(([|http://|https://]+[t.bilibili.com/|h.bilibili.com/|(www\\.|)?bilibili.com/video/(av|AV)|(www\\.|)?bilibili.com/audio/au|(www\\.|)?bilibili.com/read/cv]+\\d+)|(www\\.|)?bilibili.com/read/(BV|bv)[0-9a-zA-Z]{10})\\?*.*");
            if (reg.IsMatch(textBox.Text))
            {
                textBox.BorderBrush = new SolidColorBrush(Colors.Black);
                Lbl_Url_ErrorInfo.Visibility = Visibility.Collapsed;
                return;
            }

            textBox.BorderBrush = new SolidColorBrush(Colors.Red);
            Lbl_Url_ErrorInfo.Text = "非法url！";
            Lbl_Url_ErrorInfo.Visibility = Visibility.Visible;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
    }
}
