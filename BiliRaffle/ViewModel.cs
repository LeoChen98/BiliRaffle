using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BiliRaffle
{
    internal class ViewModel : INotifyPropertyChanged
    {
        #region Private Fields

        private static ViewModel _viewmodel;
        private bool _AsPlugin = false;
        private RelayCommand<int> _ChangeNum;
        private bool _CheckFollow = false;
        private bool _Filter = true;
        private int _FilterCondition = 5;
        private bool _IsCommentEnabled = false;
        private bool _IsOneChance = true;
        private bool _IsRepliesInFloors = true;
        private bool _IsReposeEnabled = true;
        private bool _IsValid = false;
        private string _Msg;
        private RelayCommand _NoticeLogin;
        private int _Num = 1;
        private ICommand _Start;
        private string _Url;
        private string _whwnd;

        #endregion Private Fields

        #region Public Constructors

        public ViewModel()
        {
            PushMsg($"当前版本：{Assembly.GetExecutingAssembly().GetName().Version}");
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Public Properties

        /// <summary>
        /// 单例实例
        /// </summary>
        public static ViewModel Main
        {
            get
            {
                if (_viewmodel == null)
                {
                    _viewmodel = new ViewModel();
                }
                return _viewmodel;
            }
            set
            {
                _viewmodel = value;
            }
        }

        /// <summary>
        /// 以插件模式运行
        /// </summary>
        public bool AsPlugin
        {
            get
            {
                return _AsPlugin;
            }
            set
            {
                _AsPlugin = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AsPlugin"));
            }
        }

        private double _WndLeft;
        private double _WndTop;

        public double WndLeft
        {
            get { return _WndLeft; }
            set
            {
                _WndLeft = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WndLeft"));
            }
        }

        public double WndTop
        {
            get { return _WndTop; }
            set
            {
                _WndTop = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WndTop"));
            }
        }

        /// <summary>
        /// 修改中奖人数命令
        /// </summary>
        public RelayCommand<int> ChangeNum
        {
            get
            {
                if (_ChangeNum == null)
                {
                    _ChangeNum = new RelayCommand<int>((e) =>
                    {
                        Num = (int)e;
                    }, (e) =>
                     {
                         return e != null;
                     });
                }
                return _ChangeNum;
            }
        }

        /// <summary>
        /// 检查粉丝
        /// </summary>
        public bool CheckFollow
        {
            get { return _CheckFollow; }
            set
            {
                _CheckFollow = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CheckFollow"));
            }
        }

        /// <summary>
        /// 过滤抽奖号
        /// </summary>
        public bool Filter
        {
            get { return _Filter; }
            set
            {
                _Filter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Filter"));
            }
        }

        /// <summary>
        /// 抽奖号阈值
        /// </summary>
        public int FilterCondition
        {
            get { return _FilterCondition; }
            set
            {
                if (value < 1)
                    _FilterCondition = 1;
                else if (value > 10)
                    _FilterCondition = 10;
                else
                    _FilterCondition = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilterCondition"));
            }
        }

        /// <summary>
        /// 评论抽奖
        /// </summary>
        public bool IsCommentEnabled
        {
            get { return _IsCommentEnabled; }
            set
            {
                _IsCommentEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsCommentEnabled"));
            }
        }

        /// <summary>
        /// 不统计重复
        /// </summary>
        public bool IsOneChance
        {
            get { return _IsOneChance; }
            set
            {
                _IsOneChance = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsOneChance"));
            }
        }

        /// <summary>
        /// 评论楼中楼
        /// </summary>
        public bool IsRepliesInFloors
        {
            get { return _IsRepliesInFloors; }
            set
            {
                _IsRepliesInFloors = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRepliesInFloors"));
            }
        }

        /// <summary>
        /// 转发抽奖
        /// </summary>
        public bool IsReposeEnabled
        {
            get { return _IsReposeEnabled; }
            set
            {
                _IsReposeEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsReposeEnabled"));
            }
        }

        /// <summary>
        /// 指示是否有输入错误
        /// </summary>
        public bool IsValid
        {
            get { return _IsValid; }
            set { _IsValid = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsValid")); }
        }

        /// <summary>
        /// 抽奖信息
        /// </summary>
        public string Msg
        {
            get { return _Msg; }
            set
            {
                _Msg = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Msg"));
            }
        }

        /// <summary>
        /// 开始抽奖命令
        /// </summary>
        public ICommand NoticeLogin
        {
            get
            {
                if (_NoticeLogin == null)
                {
                    _NoticeLogin = new RelayCommand(() =>
                    {
                        System.Windows.Forms.MessageBox.Show("该功能稍后需要登录。");
                    });
                }
                return _NoticeLogin;
            }
        }

        /// <summary>
        /// 中奖人数
        /// </summary>
        public int Num
        {
            get { return _Num; }
            set
            {
                if (value > 0)
                {
                    _Num = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Num"));
                }
            }
        }

        /// <summary>
        /// 开始抽奖命令
        /// </summary>
        public ICommand Start
        {
            get
            {
                if (_Start == null)
                {
                    _Start = new RelayCommand(() =>
                    {
                        if (string.IsNullOrEmpty(Url))
                        {
                            System.Windows.Forms.MessageBox.Show("抽奖地址不能为空！");
                            return;
                        }
                        Task.Factory.StartNew(() => { Raffle.StartAsync(Url, Num, IsReposeEnabled, IsCommentEnabled, IsOneChance, CheckFollow, Filter, FilterCondition, IsRepliesInFloors); });
                    });
                }
                return _Start;
            }
        }

        /// <summary>
        /// 抽奖地址
        /// </summary>
        public string Url
        {
            get
            {
                return _Url;
            }
            set
            {
                _Url = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Url"));
            }
        }

        /// <summary>
        /// 父窗口，仅插件模式可用
        /// </summary>
        public string Whwnd
        {
            get { return _whwnd; }
            set
            {
                _whwnd = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Whwnd"));
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 推送信息
        /// </summary>
        /// <param name="msg">要推送的信息</param>
        [DebuggerStepThrough]
        public void PushMsg(string msg)
        {
            Msg += msg + "\r\n";
        }

        #endregion Public Methods
    }
}