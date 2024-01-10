using DmCommons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static DmCommons.ErrorReport;

#pragma warning disable CS0649

namespace BiliRaffle
{
    internal class Raffle
    {
        #region Private Fields

        private static readonly object LOCK_ADDUID = new object();

        /// <summary>
        /// 抽奖动态判断正则组
        /// </summary>
        private static readonly Regex[] RaffleRegexGroups = { new("抽奖"), new("随机选.*?送") };

        /// <summary>
        /// Oid正则
        /// </summary>
        private static readonly Regex reg_Oid = new Regex(@"""rid_str"":""(\d+)""");

        private static readonly string REPO = "LeoChen98/BiliRaffle";
        private static string _Cookies;
        private static string _UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";
        private static List<string> uids;

        private static readonly int[] MixinKeyEncTab =
        {
            46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49, 33, 9, 42, 19, 29, 28, 14, 39,
            12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40, 61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63,
            57, 62, 11, 36, 20, 34, 44, 52
        };
        private static string _imgKey = "";
        private static string _subKey = "";

        #endregion Private Fields

        #region Public Properties

        public static string Cookies
        {
            get
            {
                if (string.IsNullOrEmpty(_Cookies))
                {
                    if (!System.Windows.Application.Current.Dispatcher.Invoke(() => { return (bool)LoginWindow.Instance.ShowDialog(); }))
                    {
                        return null;
                    }
                    return _Cookies;
                }
                else
                {
                    return _Cookies;
                }
            }
            set
            {
                _Cookies = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// 开始抽奖
        /// </summary>
        /// <param name="urlText">Url</param>
        /// <param name="num">中奖人数</param>
        /// <param name="IsReposeEnabled"></param>
        /// <param name="IsCommentEnabled"></param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="CheckFollow">需要关注</param>
        /// <param name="Filter">过滤抽奖号</param>
        /// <param name="FilterCondition">抽奖号阈值</param>
        public static void Start(string urlText, int num, bool IsReposeEnabled, bool IsCommentEnabled, bool OneChance = false, bool CheckFollow = false, bool Filter = true, int FilterCondition = 5, bool IsRepliesInFloors = true)
        {
            if (CheckFollow && Cookies == null)
            {
                ViewModel.Main.PushMsg("账号未登录，请登录账号或关闭需登录功能后再试！");
                return;
            }
            try
            {
                ViewModel.Main.PushMsg("---------抽奖开始---------");
                var urls = urlText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                uids = new List<string>();

                int flag = 0;
                List<string> T_ids = new List<string>();
                List<string> H_ids = new List<string>();
                List<string> V_ids = new List<string>();
                List<string> C_ids = new List<string>();
                List<string> A_ids = new List<string>();
                List<string> O_ids = new List<string>();

                foreach (var urlRaw in urls)
                {
                    var url = urlRaw.Split(new char[] { '?', '#' })[0];
                    string[] tmp = url.Split('/');
                    if (tmp.Length < 4) continue;

                    switch (tmp[2])
                    {
                        case "t.bilibili.com":
                            if (T_ids.Count == 0) flag += 1;
                            string tid = Get_T_Id(tmp[3]);
                            if (!string.IsNullOrEmpty(tid)) T_ids.Add(tid);
                            break;

                        case "h.bilibili.com":
                            if (H_ids.Count == 0) flag += 2;
                            H_ids.Add(tmp[3]);
                            break;

                        case "www.bilibili.com":
                            switch (tmp[3])
                            {
                                case "video":
                                    if (V_ids.Count == 0) flag += 4;
                                    V_ids.Add(tmp[4]);
                                    break;

                                case "read":
                                    if (C_ids.Count == 0) flag += 8;
                                    C_ids.Add(tmp[4]);
                                    break;

                                case "audio":
                                    if (A_ids.Count == 0) flag += 16;
                                    A_ids.Add(tmp[4]);
                                    break;

                                case "opus":
                                    if (O_ids.Count == 0) flag += 32;
                                    O_ids.Add(tmp[4]);
                                    break;

                                default:
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }

                ViewModel.Main.PushMsg("---------抽奖设置---------");
                switch (flag)
                {
                    case 1:
                    case 32:
                        if (IsReposeEnabled && IsCommentEnabled)
                            ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", T_ids)}\r\n抽奖类型：动态转发/评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}");
                        else if (IsReposeEnabled)
                            ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", T_ids)}\r\n抽奖类型：动态转发抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        else
                            ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", T_ids)}\r\n抽奖类型：动态评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 2:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", H_ids)}\r\n抽奖类型：画簿评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 4:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", V_ids)}\r\n抽奖类型：视频评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 8:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", C_ids)}\r\n抽奖类型：专栏评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 16:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", A_ids)}\r\n抽奖类型：音频评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    default:
                        string str = "";
                        if (T_ids.Count > 0) str += $"动态：{string.Join(",", T_ids)}\r\n";
                        if (H_ids.Count > 0) str += $"画簿：{string.Join(",", H_ids)}\r\n";
                        if (V_ids.Count > 0) str += $"视频：{string.Join(",", V_ids)}\r\n";
                        if (C_ids.Count > 0) str += $"专栏：{string.Join(",", C_ids)}\r\n";
                        if (A_ids.Count > 0) str += $"音频：{string.Join(",", A_ids)}\r\n";
                        if (O_ids.Count > 0) str += $"综合动态：{string.Join(",", O_ids)}\r\n";
                        ViewModel.Main.PushMsg($"抽奖地址：\r\n{str}抽奖类型：综合抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;
                }

                ViewModel.Main.PushMsg("---------抽奖信息---------");

                T_Raffle_r(T_ids.ToArray(), OneChance);
                T_Raffle_c(T_ids.ToArray(), OneChance, IsRepliesInFloors);
                H_Raffle(H_ids.ToArray(), OneChance, IsRepliesInFloors);
                V_Raffle(V_ids.ToArray(), OneChance, IsRepliesInFloors);
                C_Raffle(C_ids.ToArray(), OneChance, IsRepliesInFloors);
                A_Raffle(A_ids.ToArray(), OneChance, IsRepliesInFloors);
                O_Raffle_r(O_ids.ToArray(), OneChance);
                O_Raffle_c(O_ids.ToArray(), OneChance, IsRepliesInFloors);

                string[] rs = DoRaffle(uids.ToArray(), num, CheckFollow, Filter, FilterCondition);
                ViewModel.Main.PushMsg("---------中奖名单---------");
                foreach (string i in rs)
                {
                    ViewModel.Main.PushMsg(GetUName(i) + "(uid:" + i + ")");
                }
                ViewModel.Main.PushMsg("---------抽奖结束---------");
            }
            catch (WebException wex)
            {
                if (wex.Message.Contains("412"))
                    System.Windows.Forms.MessageBox.Show($"B站服务器已拒绝访问，请稍后重试。\r\n详细信息：\r\n{wex.Message}");
                else
                    System.Windows.Forms.MessageBox.Show($"网络错误！请检查网络连接。\r\n详细信息：\r\n{wex.Message}");
            }
            catch (Exception ex)
            {
                Github.Send(REPO, new ExceptionEx(ex.Message, ex, new object[] { urlText, num, IsReposeEnabled, IsCommentEnabled, OneChance, CheckFollow, Filter, FilterCondition, IsRepliesInFloors }));
            }
        }

        /// <summary>
        /// 开始抽奖（异步）
        /// </summary>
        /// <param name="urlText">Url</param>
        /// <param name="num">中奖人数</param>
        /// <param name="IsReposeEnabled"></param>
        /// <param name="IsCommentEnabled"></param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="CheckFollow">需要关注</param>
        /// <param name="Filter">过滤抽奖号</param>
        /// <param name="FilterCondition">抽奖号阈值</param>
        public static async void StartAsync(string urlText, int num, bool IsReposeEnabled, bool IsCommentEnabled, bool OneChance = false, bool CheckFollow = false, bool Filter = true, int FilterCondition = 5, bool IsRepliesInFloors = true)
        {
            if (CheckFollow && Cookies == null)
            {
                ViewModel.Main.PushMsg("账号未登录，请登录账号或关闭需登录功能后再试！");
                return;
            }
            try
            {
                ViewModel.Main.PushMsg("---------抽奖开始---------");
                var urls = urlText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                uids = new List<string>();

                int flag = 0;
                List<string> T_ids = new List<string>();
                List<string> H_ids = new List<string>();
                List<string> V_ids = new List<string>();
                List<string> C_ids = new List<string>();
                List<string> A_ids = new List<string>();
                List<string> O_ids = new List<string>();

                foreach (var urlRaw in urls)
                {
                    var url = urlRaw.Split(new char[] { '?', '#' })[0];
                    string[] tmp = url.Split('/');
                    if (tmp.Length < 4) continue;

                    switch (tmp[2])
                    {
                        case "t.bilibili.com":
                            if (T_ids.Count == 0) flag += 1;
                            string tid = Get_T_Id(tmp[3]);
                            if (!string.IsNullOrEmpty(tid)) T_ids.Add(tid);
                            break;

                        case "h.bilibili.com":
                            if (H_ids.Count == 0) flag += 2;
                            H_ids.Add(tmp[3]);
                            break;

                        case "www.bilibili.com":
                            switch (tmp[3])
                            {
                                case "video":
                                    if (V_ids.Count == 0) flag += 4;
                                    V_ids.Add(tmp[4]);
                                    break;

                                case "read":
                                    if (C_ids.Count == 0) flag += 8;
                                    C_ids.Add(tmp[4]);
                                    break;

                                case "audio":
                                    if (A_ids.Count == 0) flag += 16;
                                    A_ids.Add(tmp[4]);
                                    break;

                                case "opus":
                                    if (O_ids.Count == 0) flag += 32;
                                    O_ids.Add(tmp[4]);
                                    break;

                                default:
                                    break;
                            }
                            break;

                        default:
                            break;
                    }
                }

                ViewModel.Main.PushMsg("---------抽奖设置---------");
                switch (flag)
                {
                    case 1:
                    case 32:
                        if (IsReposeEnabled && IsCommentEnabled)
                            ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", T_ids)}\r\n抽奖类型：动态转发/评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}");
                        else if (IsReposeEnabled)
                            ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", T_ids)}\r\n抽奖类型：动态转发抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        else
                            ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", T_ids)}\r\n抽奖类型：动态评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 2:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", H_ids)}\r\n抽奖类型：画簿评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 4:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", V_ids)}\r\n抽奖类型：视频评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 8:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", C_ids)}\r\n抽奖类型：专栏评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    case 16:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", A_ids)}\r\n抽奖类型：音频评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    default:
                        string str = "";
                        if (T_ids.Count > 0) str += $"动态：{string.Join(",", T_ids)}\r\n";
                        if (H_ids.Count > 0) str += $"画簿：{string.Join(",", H_ids)}\r\n";
                        if (V_ids.Count > 0) str += $"视频：{string.Join(",", V_ids)}\r\n";
                        if (C_ids.Count > 0) str += $"专栏：{string.Join(",", C_ids)}\r\n";
                        if (A_ids.Count > 0) str += $"音频：{string.Join(",", A_ids)}\r\n";
                        if (O_ids.Count > 0) str += $"综合动态：{string.Join(",", O_ids)}\r\n";
                        ViewModel.Main.PushMsg($"抽奖地址：\r\n{str}抽奖类型：综合抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;
                }

                ViewModel.Main.PushMsg("---------抽奖信息---------");

                List<Task> tasks = new List<Task>();
                if (IsReposeEnabled && T_ids.Count > 0)
                    tasks.Add(T_Raffle_rAsync(T_ids.ToArray(), OneChance));
                if (IsReposeEnabled && O_ids.Count > 0)
                    tasks.Add(O_Raffle_rAsync(O_ids.ToArray(), OneChance));

                if (IsCommentEnabled)
                {
                    if (T_ids.Count > 0)
                        tasks.Add(T_Raffle_cAsync(T_ids.ToArray(), OneChance, IsRepliesInFloors));
                    if (H_ids.Count > 0)
                        tasks.Add(H_RaffleAsync(H_ids.ToArray(), OneChance, IsRepliesInFloors));
                    if (V_ids.Count > 0)
                        tasks.Add(V_RaffleAsync(V_ids.ToArray(), OneChance, IsRepliesInFloors));
                    if (C_ids.Count > 0)
                        tasks.Add(C_RaffleAsync(C_ids.ToArray(), OneChance, IsRepliesInFloors));
                    if (A_ids.Count > 0)
                        tasks.Add(A_RaffleAsync(A_ids.ToArray(), OneChance, IsRepliesInFloors));
                    if (O_ids.Count > 0)
                        tasks.Add(O_Raffle_cAsync(O_ids.ToArray(), OneChance, IsRepliesInFloors));
                }

                Task.WaitAll(tasks.ToArray());

                string[] rs = await DoRaffleAsync(uids.ToArray(), num, CheckFollow, Filter, FilterCondition);
                ViewModel.Main.PushMsg("---------中奖名单---------");
                foreach (string i in rs)
                {
                    ViewModel.Main.PushMsg(GetUName(i) + "(uid:" + i + ")");
                }
                ViewModel.Main.PushMsg("---------抽奖结束---------");
            }
            catch (WebException wex)
            {
                if (wex.Message.Contains("412"))
                    System.Windows.Forms.MessageBox.Show($"B站服务器已拒绝访问，请稍后重试。\r\n详细信息：\r\n{wex.Message}");
                else
                    System.Windows.Forms.MessageBox.Show($"网络错误！请检查网络连接。\r\n详细信息：\r\n{wex.Message}");
            }
            catch (AggregateException aex)
            {
                string aex_detail = "";

                int count = 0;
                foreach (Exception e in aex.InnerExceptions)
                {
                    if (e.GetType() != typeof(WebException))
                    {
                    ReportableError:
                        aex_detail += $"  * {e?.GetType()}发生在{e?.TargetSite}中\r\n" +
                        $"    * 信息：{e?.Message}\r\n" +
                        $"    * 堆栈：{e?.StackTrace}\r\n" +
                        $"\r\n";
                        count++;
                    }
                    else
                    {
                        if (e.Message.Contains("412"))
                            System.Windows.Forms.MessageBox.Show($"B站服务器已拒绝访问，请稍后重试。\r\n详细信息：\r\n{e.Message}");
                        else
                            System.Windows.Forms.MessageBox.Show($"网络错误！请检查网络连接。\r\n详细信息：\r\n{e.Message}");
                    }
                }

                Github.Send(REPO, new ExceptionEx(aex.Message, aex, new object[] { urlText, num, IsReposeEnabled, IsCommentEnabled, OneChance, CheckFollow, Filter, FilterCondition, IsRepliesInFloors, aex_detail }));
            }
            catch (Exception ex)
            {
                Github.Send(REPO, new ExceptionEx(ex.Message, ex, new object[] { urlText, num, IsReposeEnabled, IsCommentEnabled, OneChance, CheckFollow, Filter, FilterCondition, IsRepliesInFloors }));
            }
        }

        #endregion Public Methods

        #region Private Methods
        
        /// <summary>
        /// 获取WBI签名所需的img_key和sub_key
        /// </summary>
        /// <returns></returns>
        private static (string, string) GetWbiKeys()
        {
            if (string.IsNullOrEmpty(_imgKey) || string.IsNullOrEmpty(_subKey))
            {
                string str = Http.GetBody("https://api.bilibili.com/x/web-interface/nav", null);
                if (!string.IsNullOrEmpty(str))
                {
                    JObject obj = JObject.Parse(str);
                    string imgUrl = obj["data"]!["wbi_img"]!["img_url"]!.ToString();
                    string subUrl = obj["data"]!["wbi_img"]!["sub_url"]!.ToString();
                    string[] temp = imgUrl.Split('/');
                    _imgKey = temp[temp.Length - 1].Split('.')[0];
                    temp = subUrl.Split('/');
                    _subKey = temp[temp.Length - 1].Split('.')[0];
                }
            }

            return (_imgKey, _subKey);
        }
        
        /// <summary>
        /// 对imgKey和subKey进行字符顺序打乱编码
        /// </summary>
        /// <param name="orig">原始字符串</param>
        /// <returns></returns>
        private static string GetMixinKey(string orig)
        {
            return MixinKeyEncTab.Aggregate("", (s, i) => s + orig[i]).Substring(0, 32);
        }
        
        /// <summary>
        /// 对请求参数进行WBI签名
        /// </summary>
        /// <param name="parameters">请求参数</param>
        /// <param name="imgKey"></param>
        /// <param name="subKey"></param>
        /// <returns>签名后的请求参数</returns>
        private static string EncryptQueryWithWbiSign(Dictionary<string, string> parameters, string imgKey = null, string subKey = null)
        {
            if (string.IsNullOrEmpty(imgKey) || string.IsNullOrEmpty(subKey))
            {
                (imgKey, subKey) = GetWbiKeys();
            }
            string mixinKey = GetMixinKey(imgKey + subKey);
            string currTime = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            // 添加 wts 字段
            parameters["wts"] = currTime;
            // 按照 key 重排参数
            parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            // 过滤 value 中的 "!'()*" 字符
            parameters = parameters.ToDictionary(
                kvp => kvp.Key,
                kvp => new string(kvp.Value.Where(chr => !"!'()*".Contains(chr)).ToArray())
            );
            // 序列化参数
            string query = new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
            //计算 w_rid
            using MD5 md5 = MD5.Create();
            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(query + mixinKey));
            string wbiSign = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            parameters["w_rid"] = wbiSign;

            return new FormUrlEncodedContent(parameters).ReadAsStringAsync().Result;
        }

        /// <summary>
        /// 音频评论抽奖
        /// </summary>
        /// <param name="ids">au</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static void A_Raffle(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            foreach (var id in ids)
            {
                H_Reply_Data obj = new H_Reply_Data();
                int i = 1, ucount = 0;
                string rid = id.Replace("au", "");
                ViewModel.Main.PushMsg($"开始收集音频au{rid}下的评论");
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=14&oid={rid}&sort=2", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                    if (!string.IsNullOrEmpty(str))
                    {
                        obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                        if (obj.code == 0)
                        {
                            if (i == 1) ViewModel.Main.PushMsg($"音频au{rid}共有{obj.data.page.count}条评论。开始统计uid...");

                            if (obj.data.replies != null && obj.data.replies.Length != 0)
                            {
                                foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                                {
                                    ucount += AddUid(reply.mid.ToString(), OneChance);

                                    if (IsRepliesInFloors)
                                    {
                                        if (reply.rcount > 0 && reply.rcount <= 3 && reply.replies != null)
                                        {
                                            foreach (H_Reply_Data.Data.Replies_Item j in reply.replies)
                                            {
                                                ucount += AddUid(j.mid, OneChance);
                                            }
                                        }
                                        else if (reply.rcount > 3)
                                        {
                                            foreach (string mid in Get_A_RepliesInFloors(rid, reply.rpid))
                                            {
                                                ucount += AddUid(mid, OneChance);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (obj.code == -404)
                        {
                            ViewModel.Main.PushMsg($"画簿{id}不存在！");
                            break;
                        }
                    }
                    i++;

                    Thread.Sleep(500);
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

                ViewModel.Main.PushMsg($"音频au{rid}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 音频评论抽奖(异步)
        /// </summary>
        /// <param name="ids">au</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static Task A_RaffleAsync(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            return Task.Run(() =>
            {
                A_Raffle(ids, OneChance, IsRepliesInFloors);
            });
        }

        /// <summary>
        /// 添加uid
        /// </summary>
        /// <param name="uid">uid</param>
        /// <param name="OneChance">验重</param>
        private static int AddUid(string uid, bool OneChance = false)
        {
            lock (LOCK_ADDUID)
            {
                if (!uids.Contains(uid) || !OneChance)
                {
                    uids.Add(uid);
                    return 1;
                }
                return 0;
            }
        }

        /// <summary>
        /// BV转AV
        /// </summary>
        /// <param name="id">bv</param>
        /// <returns>av</returns>
        private static string BV2AV(string id)
        {
            return new Regex("\"aid\":(\\d+)").Match(Http.GetBody($"https://api.bilibili.com/x/web-interface/view?bvid={id}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } })).Groups[1].Value;
        }

        /// <summary>
        /// 专栏评论抽奖
        /// </summary>
        /// <param name="ids">cv</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static void C_Raffle(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            foreach (var id in ids)
            {
                H_Reply_Data obj = new H_Reply_Data();
                int i = 1, ucount = 0;
                string rid = id.Replace("cv", "");
                ViewModel.Main.PushMsg($"开始收集专栏cv{rid}下的评论");
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=12&oid={rid}&sort=2", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                    if (!string.IsNullOrEmpty(str))
                    {
                        obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                        if (obj.code == 0)
                        {
                            if (i == 1) ViewModel.Main.PushMsg($"专栏cv{rid}共有{obj.data.page.count}条评论。开始统计uid...");

                            if (obj.data.replies != null && obj.data.replies.Length != 0)
                            {
                                foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                                {
                                    ucount += AddUid(reply.mid.ToString(), OneChance);

                                    if (IsRepliesInFloors)
                                    {
                                        if (reply.rcount > 0 && reply.rcount <= 3 && reply.replies != null)
                                        {
                                            foreach (H_Reply_Data.Data.Replies_Item j in reply.replies)
                                            {
                                                ucount += AddUid(j.mid, OneChance);
                                            }
                                        }
                                        else if (reply.rcount > 3)
                                        {
                                            foreach (string mid in Get_C_RepliesInFloors(rid, reply.rpid))
                                            {
                                                ucount += AddUid(mid, OneChance);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else if (obj.code == 12002)
                        {
                            ViewModel.Main.PushMsg($"专栏cv{rid}已被删除或评论区关闭！");
                            break;
                        }
                    }
                    i++;

                    Thread.Sleep(500);
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

                ViewModel.Main.PushMsg($"专栏cv{rid}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 专栏评论抽奖(异步)
        /// </summary>
        /// <param name="ids">cv</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static Task C_RaffleAsync(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            return Task.Run(() =>
            {
                C_Raffle(ids, OneChance, IsRepliesInFloors);
            });
        }

        /// <summary>
        /// 抽奖
        /// </summary>
        /// <param name="uids">uid数组</param>
        /// <param name="num">中奖数量</param>
        /// <param name="CheckFollow">是否检查关注</param>
        /// <param name="Filter">是否过滤抽奖号</param>
        /// <param name="FilterCondition">抽奖号阈值</param>
        /// <returns>中奖uid</returns>
        private static string[] DoRaffle(string[] uids, int num, bool CheckFollow = false, bool Filter = true, int FilterCondition = 5)
        {
            List<string> rs = new List<string>();
            List<string> duid = new List<string>(uids);

            Random random = new Random((int)DateTime.Now.Ticks);
            random.Next();
            for (int n = 0; n < num; n++)
            {
            re:
                if (duid.Count == 0)
                {
                    ViewModel.Main.PushMsg($"抽到{rs.Count}个有效中奖，有效参与数不足，抽奖已结束。");
                    return rs.ToArray();
                }
                string uid = duid[random.Next(0, duid.Count - 1)];
                duid.Remove(uid);
                if (!rs.Contains(uid))
                {
                    if (!Filter || !IsRaffleId_new(uid, FilterCondition))
                    {
                        if (CheckFollow)
                        {
                            if (IsFollowing(uid))
                            {
                                rs.Add(uid);
                                ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，有效。");
                            }
                            else
                            {
                                goto re;
                            }
                        }
                        else
                        {
                            rs.Add(uid);
                            ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，有效。");
                        }
                    }
                    else goto re;
                }
                else
                {
                    goto re;
                }
            }
            return rs.ToArray();
        }

        /// <summary>
        /// 抽奖（异步）
        /// </summary>
        /// <param name="uids">uid数组</param>
        /// <param name="num">中奖数量</param>
        /// <param name="CheckFollow">是否检查关注</param>
        /// <param name="Filter">是否过滤抽奖号</param>
        /// <param name="FilterCondition">抽奖号阈值</param>
        /// <returns>中奖uid</returns>
        private static Task<string[]> DoRaffleAsync(string[] uids, int num, bool CheckFollow = false, bool Filter = true, int FilterCondition = 5)
        {
            return Task.Run(() =>
            {
                return DoRaffle(uids, num, CheckFollow, Filter, FilterCondition);
            });
        }

        /// <summary>
        /// 获取音频评论楼中楼
        /// </summary>
        /// <param name="rid">auid</param>
        /// <param name="rpid">回复root</param>
        /// <returns>mid数组</returns>
        private static string[] Get_A_RepliesInFloors(string rid, string rpid)
        {
            int pn = 1, count = 10;
            List<string> rs = new List<string>();
            Regex reg_count = new Regex("\"count\":(\\d+)");
            do
            {
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=14&oid={rid}&root={rpid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });

                if (!string.IsNullOrEmpty(str))
                {
                    H_Reply_Data obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                    if (obj.code == 0 && obj.data.replies != null)
                    {
                        if (pn == 1) count = int.Parse(reg_count.Match(str).Groups[1].Value);

                        foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                        {
                            rs.Add(reply.mid);
                        }
                    }
                }
                pn++;

                Thread.Sleep(500);
            } while (pn * 10 < count);

            return rs.ToArray();
        }

        /// <summary>
        /// 获取专栏评论楼中楼
        /// </summary>
        /// <param name="rid">cid</param>
        /// <param name="rpid">回复root</param>
        /// <returns>mid数组</returns>
        private static string[] Get_C_RepliesInFloors(string rid, string rpid)
        {
            int pn = 1, count = 10;
            List<string> rs = new List<string>();
            Regex reg_count = new Regex("\"count\":(\\d+)");
            do
            {
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=12&oid={rid}&root={rpid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });

                if (!string.IsNullOrEmpty(str))
                {
                    H_Reply_Data obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                    if (obj.code == 0 && obj.data.replies != null)
                    {
                        if (pn == 1) count = int.Parse(reg_count.Match(str).Groups[1].Value);

                        foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                        {
                            rs.Add(reply.mid);
                        }
                    }
                }
                pn++;

                Thread.Sleep(500);
            } while (pn * 10 < count);
            return rs.ToArray();
        }

        /// <summary>
        /// 获取相簿评论楼中楼
        /// </summary>
        /// <param name="rid">相簿id</param>
        /// <param name="rpid">回复root</param>
        /// <returns>mid数组</returns>
        private static string[] Get_H_RepliesInFloors(string rid, string rpid)
        {
            int pn = 1, count = 10;
            List<string> rs = new List<string>();
            Regex reg_count = new Regex("\"count\":(\\d+)");
            do
            {
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=11&oid={rid}&root={rpid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });

                if (!string.IsNullOrEmpty(str))
                {
                    H_Reply_Data obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                    if (obj.code == 0 && obj.data.replies != null)
                    {
                        if (pn == 1) count = int.Parse(reg_count.Match(str).Groups[1].Value);

                        foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                        {
                            rs.Add(reply.mid);
                        }
                    }
                }
                pn++;

                Thread.Sleep(500);
            } while (pn * 10 < count);

            return rs.ToArray();
        }

        /// <summary>
        /// 获取Oid
        /// </summary>
        /// <param name="url">opus地址</param>
        /// <returns>Oid</returns>
        private static string Get_O_Id(string url)
        {
            string str = "";
            HttpWebRequest httpWebRequest = null;
            HttpWebResponse httpWebResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using StreamReader streamReader = new StreamReader(new GZipStream(httpWebResponse.GetResponseStream(), CompressionMode.Decompress), Encoding.UTF8);
                str = streamReader.ReadToEnd();
            }
            finally
            {
                httpWebResponse?.Close();
                httpWebRequest?.Abort();
            }
            return reg_Oid.Match(str).Groups[1].Value.ToString();
        }
        
        /// <summary>
        /// 获取oid
        /// </summary>
        /// <param name="id">动态ID</param>
        /// <returns>oid</returns>
        private static string Get_O_Id_new(string id)
        {
            string str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id={id}", GetCookies(Cookies, "api.vc.bilibili.com"), "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.vc.bilibili.com" } });
            if (string.IsNullOrEmpty(str)) return "";
            JObject o = JObject.Parse(str);

            if ((int)o["code"] == 0 && o["data"]["card"] != null)
            {
                return o["data"]["card"]["desc"]["rid"].ToString();
            }

            str = Http.GetBody($"https://api.bilibili.com/x/polymer/web-dynamic/v1/detail?id={id}", user_agent: _UserAgent);
            if (string.IsNullOrEmpty(str)) return "";
            o = JObject.Parse(str);
            if ((int)o["code"] == 0 && o["data"]["item"] != null)
            {
                return o["data"]["item"]["basic"]["rid_str"].ToString();
            }

            ViewModel.Main.PushMsg($"动态{id}不存在！");
            return "";
        }

        /// <summary>
        /// 获取真实动态id
        /// </summary>
        /// <param name="oid">oid</param>
        private static string Get_T_Id(string oid)
        {
            string str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id={oid}", GetCookies(Cookies, "api.vc.bilibili.com"), "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.vc.bilibili.com" } });
            if (string.IsNullOrEmpty(str)) return "";
            JObject o = JObject.Parse(str);

            if ((int)o["code"] == 0 && o["data"]["card"] != null)
            {
                return o["data"]["card"]["desc"]["dynamic_id_str"].ToString();
            }

            str = Http.GetBody($"https://api.bilibili.com/x/polymer/web-dynamic/v1/detail?id={oid}", user_agent: _UserAgent);
            if (string.IsNullOrEmpty(str)) return "";
            o = JObject.Parse(str);
            if ((int)o["code"] == 0 && o["data"]["item"] != null)
            {
                return o["data"]["item"]["id_str"].ToString();
            }

            ViewModel.Main.PushMsg($"动态{oid}不存在！");
            return "";
        }

        /// <summary>
        /// 获取动态评论楼中楼
        /// </summary>
        /// <param name="rid">相簿id</param>
        /// <param name="rpid">回复root</param>
        /// <returns>mid数组</returns>
        private static string[] Get_T_RepliesInFloors(string rid, string rpid)
        {
            int pn = 1, count = 10;
            List<string> rs = new List<string>();
            Regex reg_count = new Regex("\"count\":(\\d+)");
            do
            {
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=17&oid={rid}&root={rpid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });

                if (!string.IsNullOrEmpty(str))
                {
                    H_Reply_Data obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                    if (obj.code == 0 && obj.data.replies != null)
                    {
                        if (pn == 1) count = int.Parse(reg_count.Match(str).Groups[1].Value);

                        foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                        {
                            rs.Add(reply.mid);
                        }
                    }
                }
                pn++;

                Thread.Sleep(500);
            } while (pn * 10 < count);

            return rs.ToArray();
        }

        /// <summary>
        /// 获取视频评论楼中楼
        /// </summary>
        /// <param name="rid">av号</param>
        /// <param name="rpid">回复root</param>
        /// <returns>mid数组</returns>
        private static string[] Get_V_RepliesInFloors(string rid, string rpid)
        {
            int pn = 1, count = 10;
            List<string> rs = new List<string>();
            Regex reg_count = new Regex("\"count\":(\\d+)");
            do
            {
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn={pn}&type=1&oid={rid}&root={rpid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });

                if (!string.IsNullOrEmpty(str))
                {
                    V_Comment_Templete obj = JsonConvert.DeserializeObject<V_Comment_Templete>(str);
                    if (obj.code == 0 && obj.data.replies != null)
                    {
                        if (pn == 1) count = int.Parse(reg_count.Match(str).Groups[1].Value);

                        foreach (V_Comment_Templete.Data.Reply reply in obj.data.replies)
                        {
                            rs.Add(reply.member.mid);
                        }
                    }
                }
                pn++;

                Thread.Sleep(500);
            } while (pn * 10 < count);

            return rs.ToArray();
        }

        /// <summary>
        /// 获取cookies实例
        /// </summary>
        /// <param name="cookies">cookies文本</param>
        /// <param name="domain">cookies所属的域</param>
        /// <returns>cookies实例</returns>
        private static CookieCollection GetCookies(string cookies, string domain = "api.bilibili.com")
        {
            try
            {
                CookieCollection public_cookie;
                public_cookie = new CookieCollection();
                cookies = cookies.Replace(",", "%2C");//转义“，”
                string[] cookiestrs = Regex.Split(cookies, "; ");
                foreach (string i in cookiestrs)
                {
                    string[] cookie = Regex.Split(i, "=");
                    public_cookie.Add(new Cookie(cookie[0], cookie[1]) { Domain = domain });
                }
                return public_cookie;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 通过Uid获取UName
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        private static string GetUName(string uid)
        {
            string str = Http.GetBody($"https://api.bilibili.com/x/space/acc/info?mid={uid}", null, "", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/109.0.0.0 Safari/537.36 Edg/109.0.1518.78", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
            if (!string.IsNullOrEmpty(str))
            {
                if (str.Contains("\"code\":0"))
                {
                    str = str.Replace("{\"code\":-509,\"message\":\"请求过于频繁，请稍后再试\",\"ttl\":1}", "");
                }
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0)
                {
                    return obj["data"]["name"].ToString();
                }
            }
            return "";
        }

        /// <summary>
        /// 相簿评论抽奖
        /// </summary>
        /// <param name="ids">相簿id</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static void H_Raffle(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            foreach (var id in ids)
            {
                H_Reply_Data obj = new H_Reply_Data();
                int i = 1, ucount = 0;
                ViewModel.Main.PushMsg($"开始收集画簿{id}下的评论");
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=11&oid={id}&sort=2", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                    if (!string.IsNullOrEmpty(str))
                    {
                        obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                        if (obj.code == 0)
                        {
                            if (i == 1) ViewModel.Main.PushMsg($"相簿{id}共有{obj.data.page.count}条评论。开始统计uid...");

                            if (obj.data.replies != null && obj.data.replies.Length != 0)
                            {
                                foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                                {
                                    ucount += AddUid(reply.mid.ToString(), OneChance);

                                    if (IsRepliesInFloors)
                                    {
                                        if (reply.rcount > 0 && reply.rcount <= 3 && reply.replies != null)
                                        {
                                            foreach (H_Reply_Data.Data.Replies_Item j in reply.replies)
                                            {
                                                ucount += AddUid(j.mid, OneChance);
                                            }
                                        }
                                        else if (reply.rcount > 3)
                                        {
                                            foreach (string mid in Get_H_RepliesInFloors(id, reply.rpid))
                                            {
                                                ucount += AddUid(mid, OneChance);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    i++;

                    Thread.Sleep(500);
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

                ViewModel.Main.PushMsg($"相簿{id}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 相簿评论抽奖（异步）
        /// </summary>
        /// <param name="ids">相簿id</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static Task H_RaffleAsync(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            return Task.Run(() =>
            {
                H_Raffle(ids, OneChance, IsRepliesInFloors);
            });
        }

        /// <summary>
        /// 判断是否为粉丝
        /// </summary>
        /// <param name="uid">uid</param>
        /// <returns>是否</returns>
        private static bool IsFollowing(string uid)
        {
            try
            {
                if (!string.IsNullOrEmpty(Cookies))
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/space/acc/relation?mid={uid}", GetCookies(Cookies), "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject obj = JObject.Parse(str);
                        if ((int)obj["code"] == 0)
                        {
                            switch ((int)obj["data"]["be_relation"]["attribute"])
                            {
                                case 1://悄悄关注
                                case 2://关注
                                case 6://互关
                                    return true;

                                default:
                                    ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，但未关注，结果无效。(relation:{obj["data"]["be_relation"]["attribute"]})");
                                    return false;
                            }
                        }
                    }
                }
                ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，但未关注，结果无效。");
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查动态是否为抽奖转发
        /// </summary>
        /// <param name="item">动态JToken</param>
        /// <returns>是否</returns>
        private static bool IsRaffleDynamic(JToken item)
        {
            string text = null;

            switch (item["orig"]["type"].ToString())
            {
                case "DYNAMIC_TYPE_AV":
                    text = item["orig"]["modules"]["module_dynamic"]["major"]["archive"]["desc"].ToString();
                    break;

                default:
                    text = item["orig"]["modules"]["module_dynamic"]["desc"]["text"].ToString();
                    break;
            }

            if (string.IsNullOrEmpty(text))
                return false;

            foreach (Regex regex in RaffleRegexGroups)
            {
                if (regex.IsMatch(text))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否抽奖号
        /// </summary>
        /// <param name="uid">账号uid</param>
        /// <param name="condition">抽奖号阈值</param>
        /// <returns>是否</returns>
        private static bool IsRaffleId(string uid, int condition)
        {
            int raffle_count = 0;
            Regex reg = new Regex("抽奖");
            string str = Http.GetBody($"https://api.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?visitor_uid=0&host_uid={uid}&offset_dynamic_id=0", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.vc.bilibili.com" } });
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0)
                {
                    Dynamic_Data data = JsonConvert.DeserializeObject<Dynamic_Data>(obj["data"].ToString());
                    if (data.cards == null) return false;
                    int check_count = data.cards.Length >= 10 ? 10 : data.cards.Length;
                    for (int i = 0; i < check_count; i++)
                    {
                        if (reg.IsMatch(data.cards[i].card))
                        {
                            raffle_count++;
                        }
                    }
                }
            }

            if (raffle_count > condition)
            {
                ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，但判定为抽奖号，结果无效。（指数：{raffle_count}/{condition}）");
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 检查是否抽奖号
        /// </summary>
        /// <param name="uid">账号uid</param>
        /// <param name="condition">抽奖号阈值</param>
        /// <returns>是否</returns>
        private static bool IsRaffleId_new(string uid, int condition)
        {
            int raffle_count = 0;
            Regex reg = new Regex("抽奖");
            string str = Http.GetBody($"https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space?offset=&host_mid={uid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
            if (!string.IsNullOrEmpty(str))
            {
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] == 0)
                {
                    foreach (JToken item in obj["data"]["items"])
                    {
                        if (item["type"].ToString() == "DYNAMIC_TYPE_FORWARD"
                            && IsRaffleDynamic(item))
                        {
                            raffle_count++;
                        }
                    }
                }
            }

            if (raffle_count > condition)
            {
                ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，但判定为抽奖号，结果无效。（指数：{raffle_count}/{condition}）");
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 动态评论抽奖（支持普通动态和综合动态）
        /// </summary>
        /// <param name="id">动态ID</param>
        /// <param name="oneChance">只有一次机会</param>
        /// <param name="isRepliesInFloors">楼中楼</param>
        /// <returns></returns>
        private static int Dynamic_Raffle_Comment(string id, bool oneChance, bool isRepliesInFloors)
        {
            int ucount = 0;
            string oid = Get_O_Id_new(id);
            int next = 0;
            bool is_end = false;
            string offset = "";
            while (!is_end)
            {
                var query = EncryptQueryWithWbiSign(new Dictionary<string, string>
                {
                    {"oid", oid},
                    {"type", "11"},
                    {"mode", "2"},
                    {"next", next.ToString()},
                    {"pagination_str", JsonConvert.SerializeObject(new Dictionary<string, string> { { "offset", offset } })}
                });
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/wbi/main?{query}", GetCookies(Cookies), "", _UserAgent, new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                if (string.IsNullOrEmpty(str))
                {
                    continue;
                }
                JObject obj = JObject.Parse(str);
                if ((int)obj["code"] != 0)
                {
                    continue;
                }

                next = (int)obj["data"]["cursor"]["next"];
                is_end = (bool)obj["data"]["cursor"]["is_end"];
                offset = (string)obj["data"]["cursor"]["pagination_reply"]["next_offset"];

                foreach (JToken token in obj["data"]["replies"])
                {
                    ucount += AddUid(token["member"]["mid"].ToString(), oneChance);

                    if (isRepliesInFloors)
                    {
                        foreach (JToken sub_token in token["replies"])
                        {
                            ucount += AddUid(sub_token["member"]["mid"].ToString(), oneChance);
                        }
                    }
                }

                Thread.Sleep(500);
            }

            return ucount;
        }
        
        /// <summary>
        /// 综合动态评论抽奖
        /// </summary>
        /// <param name="ids">oid</param>
        /// <param name="oneChance">只有一次机会</param>
        /// <param name="isRepliesInFloors">楼中楼</param>
        private static void O_Raffle_c(string[] ids, bool oneChance, bool isRepliesInFloors)
        {
            foreach (string id in ids)
            {
                int ucount = Dynamic_Raffle_Comment(id, oneChance, isRepliesInFloors);
                ViewModel.Main.PushMsg($"综合动态{id}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 综合动态评论抽奖（异步）
        /// </summary>
        /// <param name="ids">oid</param>
        /// <param name="oneChance">只有一次机会</param>
        /// <param name="isRepliesInFloors">楼中楼</param>
        private static Task O_Raffle_cAsync(string[] ids, bool oneChance, bool isRepliesInFloors)
        {
            return Task.Run(() =>
            {
                O_Raffle_c(ids, oneChance, isRepliesInFloors);
            });
        }
        
        /// <summary>
        /// 综合动态转发抽奖
        /// </summary>
        /// <param name="ids">oid</param>
        /// <param name="oneChance">只有一次机会</param>
        private static void O_Raffle_r(string[] ids, bool oneChance)
        {
            int ucount = 0;
            foreach (string id in ids)
            {
                bool has_more = true;
                string offset = "";
                while (has_more)
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/polymer/web-dynamic/v1/detail/forward?id={id}&offset={offset}", null, "", _UserAgent, new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    JObject obj = JObject.Parse(str);
                    if ((int)obj["code"] != 0)
                    {
                        continue;
                    }

                    offset = obj["data"]["offset"].ToString();
                    has_more = (bool)obj["data"]["has_more"];

                    foreach (JToken token in obj["data"]["items"])
                    {
                        ucount += AddUid(token["user"]["mid"].ToString(), oneChance);
                    }

                    Thread.Sleep(10000);
                }

                ViewModel.Main.PushMsg($"综合动态{id}下共统计到{ucount}个（次）uid转发");
            }
        }

        private static void O_Raffle_r_new(string[] ids, bool oneChance)
        {
            foreach (string id in ids)
            {
                int ucount = 0;
                bool has_more = true;
                string offset = "";
                while (has_more)
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/polymer/web-dynamic/v1/detail/reaction?id={id}&offset={offset}", GetCookies(Cookies), "", _UserAgent, new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });
                    if (string.IsNullOrEmpty(str))
                    {
                        continue;
                    }
                    JObject obj = JObject.Parse(str);
                    if ((int)obj["code"] != 0)
                    {
                        break;
                    }
                    
                    foreach (JToken token in obj["data"]["items"])
                    {
                        if (token["action"].ToString() == "转发了")
                        {
                            ucount += AddUid(token["mid"].ToString(), oneChance);
                        }
                    }
                    
                    offset = obj["data"]["offset"].ToString();
                    has_more = (bool)obj["data"]["has_more"];

                    JObject offsetObj = JObject.Parse(offset);
                    if (offsetObj["repost"].ToString() == "-1") // 此时接口不再返回转发数据
                    {
                        break;
                    }
                    
                    Thread.Sleep(500);
                }
                ViewModel.Main.PushMsg($"综合动态{id}下共统计到{ucount}个（次）uid转发");
            }
        }

        /// <summary>
        /// 综合动态转发抽奖（异步）
        /// </summary>
        /// <param name="ids">oid</param>
        /// <param name="oneChance">只有一次机会</param>
        private static Task O_Raffle_rAsync(string[] ids, bool oneChance)
        {
            ViewModel.Main.PushMsg("请扫码登录以缩短转发列表获取时间。请注意，转发列表只能获取约500个最近转发的用户。");
            if (Cookies != null)
            {
                return Task.Run(() =>
                {
                    O_Raffle_r_new(ids, oneChance);
                });
            }
            return Task.Run(() =>
            {
                O_Raffle_r(ids, oneChance);
            });
        }

        /// <summary>
        /// 动态评论抽奖
        /// </summary>
        /// <param name="ids">动态id</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static void T_Raffle_c(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            foreach (var id in ids)
            {
                ViewModel.Main.PushMsg($"开始收集动态{id}下的评论");
                string pre_str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id={id}", GetCookies(Cookies, "api.vc.bilibili.com"), "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.vc.bilibili.com" } });
                if (string.IsNullOrEmpty(pre_str)) return;
                JObject o = JObject.Parse(pre_str);
                if ((int)o["code"] != 0) return;
                switch ((int)o["data"]["card"]["desc"]["type"])
                {
                    // case 2: //画簿
                    //     goto H_C;

                    case 8: //视频
                        goto V_C;

                    case 64://专栏
                        goto C_C;

                    case 256://音频
                        goto A_C;

                    default://一般动态
                        break;
                }

                int ucount = Dynamic_Raffle_Comment(id, OneChance, IsRepliesInFloors);
                ViewModel.Main.PushMsg($"动态{id}下共统计到{ucount}个（次）uid评论");
                return;

            V_C://视频
                ViewModel.Main.PushMsg($"动态{id}是视频，执行视频评论收集");
                V_Raffle(new string[1] { "av" + o["data"]["card"]["desc"]["rid_str"].ToString() }, OneChance, IsRepliesInFloors);
                return;

            H_C://画簿
                ViewModel.Main.PushMsg($"动态{id}是画簿，执行画簿评论收集");
                H_Raffle(new string[1] { o["data"]["card"]["desc"]["rid_str"].ToString() }, OneChance, IsRepliesInFloors);
                return;

            C_C://专栏
                ViewModel.Main.PushMsg($"动态{id}是专栏，执行专栏评论收集");
                C_Raffle(new string[1] { o["data"]["card"]["desc"]["rid_str"].ToString() }, OneChance, IsRepliesInFloors);
                return;

            A_C://音频
                ViewModel.Main.PushMsg($"动态{id}是音频，执行音频评论收集");
                A_Raffle(new string[1] { o["data"]["card"]["desc"]["rid_str"].ToString() }, OneChance, IsRepliesInFloors);
                return;
            }
        }

        /// <summary>
        /// 动态评论抽奖(异步)
        /// </summary>
        /// <param name="ids">动态id</param>
        /// <param name="OneChance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static Task T_Raffle_cAsync(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            return Task.Run(() =>
            {
                T_Raffle_c(ids, OneChance, IsRepliesInFloors);
            });
        }

        /// <summary>
        /// 动态抽奖
        /// </summary>
        /// <param name="ids">动态id(c_id)</param>
        /// <param name="OneChance">只有一次机会</param>
        private static void T_Raffle_r(string[] ids, bool OneChance = false)
        {
            foreach (var id in ids)
            {
                T_Repost_Data Data = new T_Repost_Data();
                int i = 0, ucount = 0, count = 0;
                ViewModel.Main.PushMsg($"开始收集动态{id}下的转发");
                while (Data.has_more == 1)
                {
                    string str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/repost_detail?dynamic_id={id}{(i == 0 ? "" : $"&offset={Data.offset}")}", GetCookies(Cookies), "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.vc.bilibili.com" } });
                    Debug.WriteLine(str);
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject obj = JObject.Parse(str);
                        if ((int)obj["code"] == 0)
                        {
                            Data = JsonConvert.DeserializeObject<T_Repost_Data>(obj["data"].ToString());

                            if (i == 0) ViewModel.Main.PushMsg($"动态{id} 共有{Data.total}条转发。开始统计uid...");

                            if (Data.items != null && Data.items.Length != 0)
                            {
                                foreach (T_Repost_Data.Desc comment in Data.items)
                                {
                                    ucount += AddUid(comment.desc.uid.ToString(), OneChance);
                                    count++;
                                }
                            }
                        }
                    }
                    i++;

                    Thread.Sleep(500);
                }
                ViewModel.Main.PushMsg($"动态{id}下共统计到{ucount}个uid的{count}次转发");
            }
        }

        /// <summary>
        /// 动态抽奖(异步)
        /// </summary>
        /// <param name="ids">动态id</param>
        /// <param name="OneChance">只有一次机会</param>
        private static Task T_Raffle_rAsync(string[] ids, bool OneChance = false)
        {
            return Task.Run(() =>
                T_Raffle_r(ids, OneChance)
                );
        }

        /// <summary>
        /// 视频抽奖收集
        /// </summary>
        /// <param name="ids">视频id（含av/bv)</param>
        /// <param name="onechance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static void V_Raffle(string[] ids, bool onechance = false, bool IsRepliesInFloors = true)
        {
            Regex reg = new Regex("BV[A-Za-z0-9]{10}");
            foreach (string id in ids)
            {
                ViewModel.Main.PushMsg($"开始收集视频{id}下的评论");

                string rid;
                rid = reg.IsMatch(id) ? BV2AV(id) : id.ToLower().Replace("av", "");

                if (string.IsNullOrEmpty(rid))
                {
                    ViewModel.Main.PushMsg($"视频{id}已被删除或无法访问！");
                    continue;
                }

                int pn = 1, count = 20, ucount = 0;
                Regex reg_count = new Regex("\"count\":(\\d+)");
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?pn={pn}&type=1&oid={rid}", null, "", "", new WebHeaderCollection { { HttpRequestHeader.Host, "api.bilibili.com" } });

                    if (!string.IsNullOrEmpty(str))
                    {
                        V_Comment_Templete obj = JsonConvert.DeserializeObject<V_Comment_Templete>(str);
                        if (obj.code == 0)
                        {
                            if (pn == 1)
                            {
                                count = int.Parse(reg_count.Match(str).Groups[1].Value);
                                ViewModel.Main.PushMsg($"视频{id}共有{count}条评论。开始统计uid...");
                            }
                            if (obj.data.replies != null && obj.data.replies.Length != 0)
                            {
                                foreach (V_Comment_Templete.Data.Reply reply in obj.data.replies)
                                {
                                    ucount += AddUid(reply.member.mid, onechance);

                                    if (IsRepliesInFloors)
                                    {
                                        if (reply.rcount > 0 && reply.rcount <= 3 && reply.replies != null)
                                        {
                                            foreach (V_Comment_Templete.Data.Reply j in reply.replies)
                                            {
                                                ucount += AddUid(j.member.mid, onechance);
                                            }
                                        }
                                        else if (reply.rcount > 3)
                                        {
                                            foreach (string mid in Get_V_RepliesInFloors(rid, reply.rpid))
                                            {
                                                ucount += AddUid(mid, onechance);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    pn++;

                    Thread.Sleep(500);
                } while (pn * 20 <= (count / 20 + 1) * 20);

                ViewModel.Main.PushMsg($"视频{id}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 视频抽奖收集
        /// </summary>
        /// <param name="ids">视频id（含av/bv)</param>
        /// <param name="onechance">只有一次机会</param>
        /// <param name="IsRepliesInFloors">楼中楼</param>
        private static Task V_RaffleAsync(string[] ids, bool onechance = false, bool IsRepliesInFloors = true)
        {
            return Task.Run(() =>
            {
                V_Raffle(ids, onechance, IsRepliesInFloors);
            });
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// 动态数据模板
        /// </summary>
        private class Dynamic_Data
        {
            #region Public Fields

            public Card[] cards;

            #endregion Public Fields

            #region Public Classes

            public class Card
            {
                #region Public Fields

                public string card;

                #endregion Public Fields
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 相簿评论数据模板
        /// </summary>
        private class H_Reply_Data
        {
            #region Public Fields

            public int code;
            public Data data;

            #endregion Public Fields

            #region Public Classes

            public class Data
            {
                #region Public Fields

                public Page page;
                public Replies_Item[] replies;

                #endregion Public Fields

                #region Public Classes

                public class Page
                {
                    #region Public Fields

                    public int count;
                    public int num;
                    public int size;

                    #endregion Public Fields
                }

                public class Replies_Item
                {
                    #region Public Fields

                    public Content content;
                    public string mid;
                    public int rcount;
                    public Replies_Item[] replies;
                    public string rpid;

                    #endregion Public Fields

                    #region Public Classes

                    public class Content
                    {
                        #region Public Fields

                        public string message;

                        #endregion Public Fields
                    }

                    #endregion Public Classes
                }

                #endregion Public Classes
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 动态转发数据模板
        /// </summary>
        private class T_Repost_Data
        {
            #region Public Fields

            public int has_more = 1;
            public Desc[] items;
            public string offset;
            public int total;

            #endregion Public Fields

            #region Public Classes

            public class Desc
            {
                #region Public Fields

                public comment desc;

                #endregion Public Fields

                #region Public Classes

                public class comment
                {
                    #region Public Fields

                    public Int64 uid;

                    #endregion Public Fields
                }

                #endregion Public Classes
            }

            #endregion Public Classes
        }

        /// <summary>
        /// 视频评论返回数据结构
        /// </summary>
        private class V_Comment_Templete
        {
            #region Public Fields

            public int code;
            public Data data;

            #endregion Public Fields

            #region Public Classes

            public class Data
            {
                #region Public Fields

                public Reply[] replies;

                #endregion Public Fields

                #region Public Classes

                public class Reply
                {
                    #region Public Fields

                    public Member member;
                    public int rcount;
                    public Reply[] replies;
                    public string rpid;

                    #endregion Public Fields

                    #region Public Classes

                    public class Member
                    {
                        #region Public Fields

                        public string mid;

                        #endregion Public Fields
                    }

                    #endregion Public Classes
                }

                #endregion Public Classes
            }

            #endregion Public Classes
        }

        #endregion Private Classes
    }
}