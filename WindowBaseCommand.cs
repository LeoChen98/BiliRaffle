using System.Windows;

namespace BiliRaffle
{
    internal class WindowBaseCommand
    {
        #region Public Properties

        public RelayCommand<Window> Close
        {
            get
            {
                return new RelayCommand<Window>((sender) =>
                {
                    sender.Close();
                }, (sender) =>
                {
                    return sender != null;
                });
            }
        }

        public RelayCommand<Window> MinSize
        {
            get
            {
                return new RelayCommand<Window>((sender) =>
                {
                    sender.WindowState = WindowState.Minimized;
                },(sender)=>
                {
                    return sender != null;
                });
            }
        }

        #endregion Public Properties
    }
}