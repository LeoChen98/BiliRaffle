using DmCommons;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using static DmCommons.ErrorReport;

#pragma warning disable CS0649

namespace BiliRaffle
{
    internal class Raffle
    {
        #region Private Fields

        private static readonly object LOCK_ADDUID = new object();
        private static readonly string REPO = "LeoChen98/BiliRaffle";
        private static string _Cookies;
        private static List<string> uids;

        #endregion Private Fields

        #region Public Properties

        public static string Cookies
        {
            get
            {
                if (string.IsNullOrEmpty(_Cookies))
                {
                    if(!System.Windows.Application.Current.Dispatcher.Invoke(()=> { return (bool)LoginWindow.Instance.ShowDialog(); }))
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

                foreach (var urlRaw in urls)
                {
                    var url = urlRaw.Split(new char[]{ '?','#'})[0];
                    string[] tmp = url.Split('/');
                    if (tmp.Length < 4) continue;

                    switch (tmp[2])
                    {
                        case "t.bilibili.com":
                            if (T_ids.Count == 0) flag += 1;
                            T_ids.Add(tmp[3]);
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
                                    if (A_ids.Count == 0) flag += 13;
                                    A_ids.Add(tmp[4]);
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

                    case 13:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", A_ids)}\r\n抽奖类型：音频评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    default:
                        string str = "";
                        if (T_ids.Count > 0) str += $"动态：{string.Join(",", T_ids)}\r\n";
                        if (H_ids.Count > 0) str += $"画簿：{string.Join(",", H_ids)}\r\n";
                        if (V_ids.Count > 0) str += $"视频：{string.Join(",", V_ids)}\r\n";
                        if (C_ids.Count > 0) str += $"专栏：{string.Join(",", C_ids)}\r\n";
                        if (A_ids.Count > 0) str += $"音频：{string.Join(",", A_ids)}\r\n";
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

                foreach (var urlRaw in urls)
                {
                    var url = urlRaw.Split(new char[] { '?', '#' })[0];
                    string[] tmp = url.Split('/');
                    if (tmp.Length < 4) continue;

                    switch (tmp[2])
                    {
                        case "t.bilibili.com":
                            if (T_ids.Count == 0) flag += 1;
                            T_ids.Add(tmp[3]);
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
                                    if (A_ids.Count == 0) flag += 13;
                                    A_ids.Add(tmp[4]);
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

                    case 13:
                        ViewModel.Main.PushMsg($"抽奖地址：{string.Join(",", A_ids)}\r\n抽奖类型：音频评论抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;

                    default:
                        string str = "";
                        if (T_ids.Count > 0) str += $"动态：{string.Join(",", T_ids)}\r\n";
                        if (H_ids.Count > 0) str += $"画簿：{string.Join(",", H_ids)}\r\n";
                        if (V_ids.Count > 0) str += $"视频：{string.Join(",", V_ids)}\r\n";
                        if (C_ids.Count > 0) str += $"专栏：{string.Join(",", C_ids)}\r\n";
                        if (A_ids.Count > 0) str += $"音频：{string.Join(",", A_ids)}\r\n";
                        ViewModel.Main.PushMsg($"抽奖地址：\r\n{str}抽奖类型：综合抽奖\r\n中奖人数：{num}\r\n不统计重复：{OneChance}\r\n需要关注：{CheckFollow}\r\n过滤抽奖号：{Filter},阈值：{FilterCondition}\r\n楼中楼：{IsRepliesInFloors}");
                        break;
                }

                ViewModel.Main.PushMsg("---------抽奖信息---------");

                List<Task> tasks = new List<Task>();
                if (IsReposeEnabled && T_ids.Count > 0)
                    tasks.Add(T_Raffle_rAsync(T_ids.ToArray(), OneChance));

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
                if(wex.Message.Contains("412"))
                    System.Windows.Forms.MessageBox.Show($"B站服务器已拒绝访问，请稍后重试。\r\n详细信息：\r\n{wex.Message}");
                else
                    System.Windows.Forms.MessageBox.Show($"网络错误！请检查网络连接。\r\n详细信息：\r\n{wex.Message}");
            }
            catch (Exception ex)
            {
                Github.Send(REPO, new ExceptionEx(ex.Message, ex, new object[] { urlText, num, IsReposeEnabled, IsCommentEnabled, OneChance, CheckFollow, Filter, FilterCondition, IsRepliesInFloors }));
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// 音频评论抽奖
        /// </summary>
        /// <param name="ids">au</param>
        /// <param name="OneChance">只有一次机会</param>
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
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=14&oid={rid}&sort=2");
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
                                        if (reply.rcount > 0 && reply.rcount <= 3)
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
                    }
                    i++;
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

                ViewModel.Main.PushMsg($"音频au{rid}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 音频评论抽奖(异步)
        /// </summary>
        /// <param name="ids">au</param>
        /// <param name="OneChance">只有一次机会</param>
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
            return new Regex("\"aid\":(\\d+)").Match(Http.GetBody($"https://api.bilibili.com/x/web-interface/view?bvid={id}")).Groups[1].Value;
        }

        /// <summary>
        /// 专栏评论抽奖
        /// </summary>
        /// <param name="ids">cv</param>
        /// <param name="OneChance">只有一次机会</param>
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
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=12&oid={rid}&sort=2");
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
                                        if (reply.rcount > 0 && reply.rcount <= 3)
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
                    }
                    i++;
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

                ViewModel.Main.PushMsg($"专栏cv{rid}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 专栏评论抽奖(异步)
        /// </summary>
        /// <param name="ids">cv</param>
        /// <param name="OneChance">只有一次机会</param>
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
                    if (!Filter || !IsRaffleId(uid, FilterCondition))
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
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=14&oid={rid}&root={rpid}");

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
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=12&oid={rid}&root={rpid}");

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
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=11&oid={rid}&root={rpid}");

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
            } while (pn * 10 < count);

            return rs.ToArray();
        }

        /// <summary>
        /// 获取相簿评论楼中楼
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
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn=1&type=17&oid={rid}&root={rpid}");

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
                string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply/reply?pn={pn}&type=1&oid={rid}&root={rpid}");

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
            } while (pn * 10 < count);

            return rs.ToArray();
        }

        /// <summary>
        /// 获取cookies实例
        /// </summary>
        /// <param name="cookies">cookies文本</param>
        /// <returns>cookies实例</returns>
        private static CookieCollection GetCookies(string cookies)
        {
            try
            {
                CookieCollection public_cookie;
                Uri target = new Uri("https://api.bilibili.com/x/relation/followers");
                public_cookie = new CookieCollection();
                cookies = cookies.Replace(",", "%2C");//转义“，”
                string[] cookiestrs = Regex.Split(cookies, "; ");
                foreach (string i in cookiestrs)
                {
                    string[] cookie = Regex.Split(i, "=");
                    public_cookie.Add(new Cookie(cookie[0], cookie[1]) { Domain = target.Host });
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
            string str = Http.GetBody($"https://api.bilibili.com/x/space/acc/info?mid={uid}");
            if (!string.IsNullOrEmpty(str))
            {
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
        private static void H_Raffle(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            foreach (var id in ids)
            {
                H_Reply_Data obj = new H_Reply_Data();
                int i = 1, ucount = 0;
                ViewModel.Main.PushMsg($"开始收集画簿{id}下的评论");
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=11&oid={id}&sort=2");
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
                                        if (reply.rcount > 0 && reply.rcount <= 3)
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
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

                ViewModel.Main.PushMsg($"相簿{id}下共统计到{ucount}个（次）uid评论");
            }
        }

        /// <summary>
        /// 相簿评论抽奖（异步）
        /// </summary>
        /// <param name="ids">相簿id</param>
        /// <param name="OneChance">只有一次机会</param>
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
            if (!string.IsNullOrEmpty(Cookies))
            {
                string str = Http.GetBody($"https://api.bilibili.com/x/space/acc/relation?mid={uid}", GetCookies(Cookies));
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
                                ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，但未关注，结果无效。(relation:{obj["data"]["be_relation"]["attribute"].ToString()})");
                                return false;
                        }
                    }
                }
            }
            ViewModel.Main.PushMsg($"抽到【{GetUName(uid)}（uid:{uid}）】中奖，但未关注，结果无效。");
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
            string str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?visitor_uid=0&host_uid={uid}&offset_dynamic_id=0");
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
        /// 动态评论抽奖
        /// </summary>
        /// <param name="ids">动态id</param>
        /// <param name="OneChance">只有一次机会</param>
        private static void T_Raffle_c(string[] ids, bool OneChance = false, bool IsRepliesInFloors = true)
        {
            foreach (var id in ids)
            {
                ViewModel.Main.PushMsg($"开始收集动态{id}下的评论");
                string pre_str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id={id}");
                if (string.IsNullOrEmpty(pre_str)) return;
                JObject o = JObject.Parse(pre_str);
                if ((int)o["code"] != 0) return;
                switch ((int)o["data"]["card"]["desc"]["type"])
                {
                    case 2: //画簿
                        goto H_C;

                    case 8: //视频
                        goto V_C;

                    case 64://专栏
                        goto C_C;

                    case 256://音频
                        goto A_C;

                    default://一般动态
                        break;
                }

                H_Reply_Data obj = new H_Reply_Data();
                int i = 1, ucount = 0;
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?jsonp=json&pn={i}&type=17&oid={id}&sort=2");
                    if (!string.IsNullOrEmpty(str))
                    {
                        obj = JsonConvert.DeserializeObject<H_Reply_Data>(str);
                        if (obj.code == 0)
                        {
                            if (i == 1) ViewModel.Main.PushMsg($"动态{id}共有{obj.data.page.count}条评论。开始统计uid...");

                            if (obj.data.replies != null && obj.data.replies.Length != 0)
                            {
                                foreach (H_Reply_Data.Data.Replies_Item reply in obj.data.replies)
                                {
                                    ucount += AddUid(reply.mid.ToString(), OneChance);

                                    if (IsRepliesInFloors)
                                    {
                                        if (reply.rcount > 0 && reply.rcount <= 3)
                                        {
                                            foreach (H_Reply_Data.Data.Replies_Item j in reply.replies)
                                            {
                                                ucount += AddUid(j.mid, OneChance);
                                            }
                                        }
                                        else if (reply.rcount > 3)
                                        {
                                            foreach (string mid in Get_T_RepliesInFloors(id, reply.rpid))
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
                } while (obj.data.page.num * obj.data.page.size < obj.data.page.count);

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
                int i = 0, ucount = 0;
                ViewModel.Main.PushMsg($"开始收集动态{id}下的转发");
                while (Data.has_more)
                {
                    string str = Http.GetBody($"https://api.vc.bilibili.com/dynamic_repost/v1/dynamic_repost/view_repost?dynamic_id={id}&offset={i * 20}");
                    if (!string.IsNullOrEmpty(str))
                    {
                        JObject obj = JObject.Parse(str);
                        if ((int)obj["code"] == 0)
                        {
                            Data = JsonConvert.DeserializeObject<T_Repost_Data>(obj["data"].ToString());

                            if (i == 0) ViewModel.Main.PushMsg($"动态{id} 共有{Data.total_count}条转发。开始统计uid...");

                            if (Data.comments != null && Data.comments.Length != 0)
                            {
                                foreach (T_Repost_Data.comment comment in Data.comments)
                                {
                                    ucount += AddUid(comment.uid.ToString(), OneChance);
                                }
                            }
                        }
                    }
                    i++;
                }
                ViewModel.Main.PushMsg($"动态{id}下共统计到{ucount}个（次）uid转发");
            }
        }

        /// <summary>
        /// 动态抽奖(异步)
        /// </summary>
        /// <param name="id">动态id(c_id)</param>
        /// <param name="num">中奖人数</param>
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
        private static void V_Raffle(string[] ids, bool onechance = false, bool IsRepliesInFloors = true)
        {
            Regex reg = new Regex("BV[A-Za-z0-9]{10}");
            foreach (string id in ids)
            {
                ViewModel.Main.PushMsg($"开始收集视频{id}下的评论");

                string rid;
                rid = reg.IsMatch(id) ? BV2AV(id) : id.ToLower().Replace("av", "");

                int pn = 1, count = 20, ucount = 0;
                Regex reg_count = new Regex("\"count\":(\\d+)");
                do
                {
                    string str = Http.GetBody($"https://api.bilibili.com/x/v2/reply?pn={pn}&type=1&oid={rid}");

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
                                        if (reply.rcount > 0 && reply.rcount <= 3)
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
                } while (pn * 20 <= (count / 20 + 1) * 20);

                ViewModel.Main.PushMsg($"视频{id}下共统计到{ucount}个（次）uid评论");
            }
        }

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

            public comment[] comments;
            public bool has_more = true;
            public int total_count;

            #endregion Public Fields

            #region Public Classes

            public class comment
            {
                #region Public Fields

                public int uid;

                #endregion Public Fields
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