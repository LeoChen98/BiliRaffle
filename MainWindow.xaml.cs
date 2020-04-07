using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
        }

        #endregion Public Constructors

        #region Private Methods

        private void Label_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void Label_MouseUp(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void Label_MouseUp_1(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        private void TB_Msg_TextChanged(object sender, TextChangedEventArgs e)
        {
            (sender as TextBox).ScrollToEnd();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Main.PushMsg("当前版本：" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }

        #endregion Private Methods
    }
}